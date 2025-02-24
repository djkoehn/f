using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using F.Framework.Blocks;
using F.Framework.Connections.Interfaces;
using F.Framework.Core.Services;
using F.Framework.Input.Interfaces;
using F.Framework.Logging;
using F.Framework.Core.SceneTree;
using F.Game.Core;
using GameManager = F.Framework.Core.GameManager;
using F.Framework.Core.Interfaces;
using F.Framework.Connections;
using F.Game.Toolbar;
using Godot;

namespace F.Framework.Input;

[Meta(typeof(IAutoNode))]
public partial class InputManager : Node, IProvide<InputManager>, IDependent, IInputManager
{
    private const float RETRY_INTERVAL = 0.1f;
    private const int MAX_RETRIES = 10;

    private readonly List<BaseBlock> _blocks = new();
    private ConnectionManager? _connectionManager;
    private BaseBlock? _currentDraggedBlock;
    private BaseBlock? _pendingDragBlock;
    private Vector2 _pendingDragPosition;
    private int _initRetryCount;
    private bool _pendingDragStart;
    private BaseBlock? _selectedBlock;
    private bool _isDragging;

    InputManager IProvide<InputManager>.Value()
    {
        return this;
    }

    public override void _Ready()
    {
        this.Provide();
        InitializeDependencies();
        Logger.Game.Print($"InputManager {Name} ready");
    }

    private void InitializeDependencies()
    {
        // Get dependencies from Services
        _connectionManager = Services.Instance?.Connections as ConnectionManager;

        if (_connectionManager == null)
        {
            _initRetryCount++;
            if (_initRetryCount >= MAX_RETRIES)
            {
                Logger.Game.Err("Failed to get required dependencies after maximum retries");
                return;
            }

            Logger.Game.Print($"Dependencies not ready, retry {_initRetryCount}/{MAX_RETRIES} in {RETRY_INTERVAL}s");
            var timer = new Timer
            {
                OneShot = true,
                WaitTime = RETRY_INTERVAL
            };
            AddChild(timer);
            timer.Timeout += () =>
            {
                timer.QueueFree();
                InitializeDependencies();
            };
            timer.Start();
            return;
        }

        Logger.Game.Print("Successfully initialized with required dependencies");
    }

    public void HandleInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            HandleMouseButtonEvent(mouseEvent);
        }
        else if (@event is InputEventMouseMotion motionEvent)
        {
            HandleMouseMotionEvent(motionEvent);
        }
        else if (@event is InputEventKey keyEvent)
        {
            HandleKeyEvent(keyEvent);
        }
    }

    public override void _Input(InputEvent @event)
    {
        HandleInput(@event);
    }

    private void HandleMouseButtonEvent(InputEventMouseButton @event)
    {
        if (@event.ButtonIndex != MouseButton.Left) return;

        if (@event.Pressed)
        {
            if (_selectedBlock != null)
            {
                _isDragging = true;
                _selectedBlock.SetDragging(true);
                Logger.Game.Print($"Started dragging block {_selectedBlock.Name}");
            }
        }
        else
        {
            if (_selectedBlock != null && _isDragging)
            {
                _isDragging = false;
                _selectedBlock.SetDragging(false);
                Logger.Game.Print($"Stopped dragging block {_selectedBlock.Name}");
            }
        }
    }

    private void HandleMouseMotionEvent(InputEventMouseMotion @event)
    {
        if (_selectedBlock != null && _isDragging)
        {
            _selectedBlock.GlobalPosition = @event.GlobalPosition;
            Logger.Game.Print($"Moved block {_selectedBlock.Name} to {_selectedBlock.GlobalPosition}");
        }
    }

    private void HandleKeyEvent(InputEventKey @event)
    {
        if (!@event.Pressed) return;

        var blocks = GetTree().GetNodesInGroup("blocks").Cast<BaseBlock>();
        foreach (var block in blocks)
        {
            if (block.Metadata?.SpawnHotkey == @event.AsText().ToLower())
            {
                Logger.Game.Print($"Spawning block {block.Name} on hotkey {block.Metadata.SpawnHotkey}");
                block.SpawnToken();
            }
        }
    }

    public void HandleSpacePress()
    {
        Logger.Game.Print("Space pressed, triggering token spawn");
        var blocks = GetTree().GetNodesInGroup("Blocks").OfType<BaseBlock>();
        foreach (var block in blocks)
            if (block.Metadata?.SpawnOnSpace == true)
                block.SpawnToken();
    }

    public void HandleRightClick(InputEventMouseButton mouseEvent)
    {
        var mousePos = GetViewport().GetMousePosition();
        var blocks = GetTree().GetNodesInGroup("Blocks").OfType<BaseBlock>();
        var block = blocks.FirstOrDefault(b => b.GetRect().HasPoint(b.ToLocal(mousePos)));

        if (block != null)
        {
            var blockName = block.Name;

            if (block.State == BlockState.Dragging)
            {
                Logger.Game.Print($"Block '{blockName}' is dragging; ending drag before returning to toolbar.");
                block.SetDragging(false);
            }

            Logger.Game.Print($"Right-click on block: '{blockName}'. Returning to toolbar.");
            block.SetInToolbar(true);
        }
    }

    public void HandleLeftClick(InputEventMouseButton mouseEvent)
    {
        var mousePos = GetViewport().GetMousePosition();
        var blocks = GetTree().GetNodesInGroup("Blocks").OfType<BaseBlock>();
        var block = blocks.FirstOrDefault(b => b.GetRect().HasPoint(b.ToLocal(mousePos)));

        if (block != null)
        {
            if (block.State == BlockState.Dragging)
            {
                PlaceBlock(block, mousePos);
            }
            else
            {
                QueueDragStart(block);
            }
        }
    }

    public void HandleLeftRelease(InputEventMouseButton mouseEvent)
    {
        // Nothing to do on release
    }

    public void HandleMouseMotion(InputEventMouseMotion mouseEvent)
    {
        var blocks = GetTree().GetNodesInGroup("Blocks").OfType<BaseBlock>();
        var block = blocks.FirstOrDefault(b => b.State == BlockState.Dragging);

        if (block != null)
        {
            block.GlobalPosition = mouseEvent.GlobalPosition;
            HandleBlockHover(block, mouseEvent.GlobalPosition);
        }
    }

    public override void _Process(double delta)
    {
        if (_pendingDragStart && _pendingDragBlock != null)
        {
            var block = _pendingDragBlock;
            _pendingDragBlock = null;
            _pendingDragStart = false;
            StartDragging(block);
        }
    }

    public void HandleBlockHover(BaseBlock block, Vector2 position)
    {
        if (_connectionManager == null) return;

        var effectivePos = position;
        Logger.Game.Print($"Checking for pipe at token position: {effectivePos}");

        var pipe = _connectionManager.GetPipeAtPosition(effectivePos);
        if (pipe != null)
        {
            block.NotifyHoveredOverPipe();
            pipe.SetInsertionHighlight(true);
        }
        else
        {
            _connectionManager.ClearInsertionHighlights();
        }
    }

    public void QueueDragStart(BaseBlock block)
    {
        if (block.State == BlockState.InToolbar)
        {
            Logger.Game.Print($"Block {block.Name} not ready, queuing drag start");
            _pendingDragBlock = block;
            _pendingDragStart = true;
        }
    }

    public void StartDragging(BaseBlock block)
    {
        if (block.State == BlockState.InToolbar)
        {
            Logger.Game.Print($"Started dragging block {block.Name}");
            block.SetDragging(true);
        }
    }

    public void PlaceBlock(BaseBlock block, Vector2 position)
    {
        var blockName = block.Name;
        Logger.Game.Print($"Placing block {blockName} at {position}");

        if (_connectionManager == null) return;

        var pipe = _connectionManager.GetPipeAtPosition(position);
        if (pipe != null)
        {
            if (_connectionManager.InsertBlockIntoPipe(block, pipe))
            {
                Logger.Game.Print($"Successfully connected block {blockName}");
                return;
            }

            Logger.Game.Print($"Failed to connect block {blockName}");
        }

        block.GlobalPosition = position;
        block.SetPlaced(true);
    }

    public void ReturnBlockToToolbar(BaseBlock block)
    {
        if (block.State == BlockState.Dragging)
        {
            block.SetDragging(false);
        }

        block.SetInToolbar(true);
        Logger.Game.Print($"Returned block {block.Name} to toolbar");
    }

    public override void _Notification(int what)
    {
        this.Notify(what);
    }

    public void SetSelectedBlock(BaseBlock block)
    {
        _selectedBlock = block;
        Logger.Game.Print($"Selected block {block.Name}");
    }

    public void ClearSelectedBlock()
    {
        if (_selectedBlock != null)
        {
            Logger.Game.Print($"Cleared selected block {_selectedBlock.Name}");
            _selectedBlock = null;
        }
    }
}