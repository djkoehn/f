using Godot;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using F.Framework.Blocks;
using F.Framework.Connections;
using F.Framework.Core;
using F.Framework.Core.SceneTree;
using F.Game.Core;
using Chickensoft.GodotNodeInterfaces;

namespace F.Framework.Input;

[Meta(typeof(IAutoNode))]
public partial class InputManager : Node, IProvide<InputManager>, IDependent
{
    private IConnectionManager? _connectionManager;
    private IGameManager? _gameManager;

    public override void _Ready()
    {
        this.Provide();

        // Get dependencies
        _connectionManager = GetNode<ConnectionManager>("../ConnectionManager");
        _gameManager = GetNode<GameManager>("/root/Main");

        if (_connectionManager == null || _gameManager == null)
        {
            GD.PrintErr("[InputManager] Failed to get required dependencies");
            return;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Space)
        {
            HandleSpacePress();
        }
        else if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
            {
                HandleRightClick(mouseEvent);
            }
            else if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (mouseEvent.Pressed)
                {
                    HandleLeftClick(mouseEvent);
                }
                else
                {
                    HandleLeftRelease(mouseEvent);
                }
            }
        }
        else if (@event is InputEventMouseMotion mouseMotion && _currentDraggedBlock != null)
        {
            HandleMouseMotion(mouseMotion);
        }
    }

    private void HandleSpacePress()
    {
        var inputBlock = _gameManager?.BlockLayer.Input;
        if (inputBlock != null)
        {
            GD.Print("[InputManager Debug] Space pressed, triggering token spawn");
            inputBlock.SpawnToken();
            GetViewport().SetInputAsHandled();
        }
    }

    private void HandleRightClick(InputEventMouseButton mouseEvent)
    {
        if (_gameManager == null) return;

        var block = _gameManager.BlockInteractionManager.GetBlockAtPosition(mouseEvent.GlobalPosition);
        if (block != null)
        {
            string blockName = block.Name ?? "unknown";

            if (block.State == BlockState.Dragging)
            {
                GD.Print($"[InputManager Debug] Block '{blockName}' is dragging; ending drag before returning to toolbar.");
                EndDrag(block);
                _currentDraggedBlock = null;
            }

            GD.Print($"[InputManager Debug] Right-click on block: '{blockName}'. Returning to toolbar.");
            ReturnBlockToToolbar(block);
            GetViewport().SetInputAsHandled();
        }
    }

    private void HandleLeftClick(InputEventMouseButton mouseEvent)
    {
        if (_gameManager == null) return;

        var block = _gameManager.BlockInteractionManager.GetBlockAtPosition(mouseEvent.GlobalPosition);
        if (block == null) return;

        string blockName = block.Name ?? "unknown";

        if (_currentDraggedBlock == block)
        {
            PlaceBlock(block, mouseEvent.GlobalPosition);
            return;
        }

        StartDrag(block, mouseEvent.GlobalPosition);
        GetViewport().SetInputAsHandled();
    }

    private void HandleLeftRelease(InputEventMouseButton mouseEvent)
    {
        GetViewport().SetInputAsHandled();
    }

    private void HandleMouseMotion(InputEventMouseMotion mouseEvent)
    {
        if (_currentDraggedBlock != null)
        {
            UpdateDrag(_currentDraggedBlock, mouseEvent.GlobalPosition);

            if (!_currentDraggedBlock.HasConnections())
            {
                Vector2 effectivePos = _currentDraggedBlock.GetTokenPosition();
                GD.Print($"[InputManager Debug] Checking for pipe at token position: {effectivePos}");
                HighlightPipeAtPosition(effectivePos);
            }
            else
            {
                ClearPipeHighlights();
            }
        }
    }

    private BaseBlock? _currentDraggedBlock;
    private BaseBlock? _pendingDragBlock;
    private Vector2 _pendingDragPosition;

    public override void _Process(double delta)
    {
        if (_pendingDragBlock != null)
        {
            if (_pendingDragBlock.GetLogicMachine() != null)
            {
                _currentDraggedBlock = _pendingDragBlock;
                _pendingDragBlock.SetDragging(true);
                _pendingDragBlock.GlobalPosition = _pendingDragPosition;
                _pendingDragBlock.ZIndex = ZIndexConfig.Layers.DraggedBlock;
                GD.Print($"[InputManager Debug] Started dragging block {_pendingDragBlock.Name}");
                _pendingDragBlock = null;
            }
        }
    }

    private void StartDrag(BaseBlock block, Vector2 position)
    {
        if (_gameManager == null) return;

        if (block.GetLogicMachine() == null)
        {
            GD.Print($"[InputManager Debug] Block {block.Name} not ready, queuing drag start");
            _pendingDragBlock = block;
            _pendingDragPosition = position;
            return;
        }

        _currentDraggedBlock = block;
        block.SetDragging(true);
        block.GlobalPosition = position;
        block.ZIndex = ZIndexConfig.Layers.DraggedBlock;
        GD.Print($"[InputManager Debug] Started dragging block {block.Name}");
    }

    private void UpdateDrag(BaseBlock block, Vector2 position)
    {
        block.GlobalPosition = position;
    }

    private void EndDrag(BaseBlock block)
    {
        block.SetDragging(false);
        block.ZIndex = ZIndexConfig.Layers.PlacedBlock;
        _currentDraggedBlock = null;
    }

    private void PlaceBlock(BaseBlock block, Vector2 position)
    {
        string blockName = block.Name ?? "unknown";
        GD.Print($"[InputManager Debug] Placing block {blockName} at {position}");

        Vector2 effectivePos = block.GetTokenPosition();
        bool connected = TryConnectBlock(block, effectivePos);

        if (connected)
        {
            GD.Print($"[InputManager Debug] Successfully connected block {blockName}");
            block.ZIndex = ZIndexConfig.Layers.PlacedBlock;
        }
        else
        {
            GD.Print($"[InputManager Debug] Failed to connect block {blockName}");
            block.SetPlaced(true);
            block.ZIndex = ZIndexConfig.Layers.PlacedBlock;
        }

        EndDrag(block);
    }

    private bool TryConnectBlock(BaseBlock block, Vector2 position)
    {
        if (_connectionManager == null) return false;

        var pipe = _connectionManager.GetPipeAtPosition(position);
        if (pipe == null) return false;

        return _connectionManager.HandleBlockConnection(block, position);
    }

    private void HighlightPipeAtPosition(Vector2 position)
    {
        if (_connectionManager == null) return;

        _connectionManager.ClearAllHighlights();
        var pipe = _connectionManager.GetPipeAtPosition(position);
        if (pipe != null)
        {
            pipe.SetInsertionHighlight(true);
        }
    }

    private void ClearPipeHighlights()
    {
        _connectionManager?.ClearAllHighlights();
    }

    private void ReturnBlockToToolbar(BaseBlock block)
    {
        if (_gameManager?.Toolbar == null) return;

        _gameManager.Toolbar.ReturnBlockToToolbar(block);
        GD.Print($"[InputManager Debug] Returned block {block.Name} to toolbar");
    }

    public override void _Notification(int what) => this.Notify(what);

    InputManager IProvide<InputManager>.Value() => this;
}