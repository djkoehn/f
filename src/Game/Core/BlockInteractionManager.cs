using F.UI.Input;
using F.UI.Toolbar;

namespace F.Game.Core;

public partial class BlockInteractionManager : Node
{
    private BlockConnectionHandler? _connectionHandler;
    private BaseBlock? _draggedBlock;
    private BlockDragHandler? _dragHandler;
    private GameManager? _gameManager;
    private BlockToolbarHandler? _toolbarHandler;
    private bool _isDraggingFromPipe;  // Add this to track if block was pulled from a pipe

    public bool IsDragging => _draggedBlock != null;
    public bool IsDraggingConnectedBlock => _draggedBlock != null && _isDraggingFromPipe;
    public BaseBlock? DraggedBlock => _draggedBlock;

    public override void _Ready()
    {
        base._Ready();
        _gameManager = GetNode<GameManager>("/root/Main/GameManager");
        _toolbarHandler = GetNode<BlockToolbarHandler>("/root/Main/InputManager/BlockToolbarHandler");
        _dragHandler = GetNode<BlockDragHandler>("/root/Main/InputManager/BlockDragHandler");
        _connectionHandler = GetNode<BlockConnectionHandler>("/root/Main/InputManager/BlockConnectionHandler");
        ProcessMode = ProcessModeEnum.Always;

        if (_gameManager == null) GD.PrintErr("Failed to find GameManager!");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
            if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
            {
                // Find block under mouse
                var block = FindBlockUnderMouse();
                if (block != null)
                {
                    GD.Print($"Right click detected on block: {block.Name}");
                    _toolbarHandler?.SendBlockHome(block);
                }
            }

        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            if (mouseButton.ButtonIndex == MouseButton.Right)
            {
                // Only return blocks that are in the BlockLayer (placed blocks)
                if (_draggedBlock != null)
                {
                    _toolbarHandler?.ReturnDraggedBlockToToolbar(_draggedBlock);
                    GetViewport().SetInputAsHandled();
                }
                else
                {
                    var block = GetBlockAtPosition(mouseButton.GlobalPosition);
                    if (block?.GetParent() == _gameManager?.ConnectionManager)
                    {
#pragma warning disable CS8604 // Possible null reference argument.
                        _toolbarHandler?.ReturnBlockToToolbar(block);
#pragma warning restore CS8604 // Possible null reference argument.
                        GetViewport().SetInputAsHandled();
                    }
                }
            }
            else if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                var block = GetBlockAtPosition(mouseButton.GlobalPosition);
                if (block != null)
                {
                    HandleBlockClick(block);
                    GetViewport().SetInputAsHandled();
                }
            }
        }
        else if (@event is InputEventMouseMotion mouseMotion && _draggedBlock?.IsDragging() == true)
        {
            // Always update position
            _draggedBlock.GlobalPosition = mouseMotion.GlobalPosition;

            // Only show hover effects for unconnected blocks from toolbar
            if (_draggedBlock.GetParent() is ToolbarBlockContainer)
            {
                _connectionHandler?.HighlightPipeAtPosition(mouseMotion.GlobalPosition);
            }
        }
    }

    public override void _Process(double delta)
    {
        if (_gameManager?.ConnectionManager == null) return;

        // Only show hover effects when dragging from toolbar
        if (_draggedBlock != null && _draggedBlock.IsDragging())
        {
            var isUnconnectedToolbarBlock = _draggedBlock.GetParent() is ToolbarBlockContainer;
            if (isUnconnectedToolbarBlock)
            {
                var mousePos = GetViewport().GetMousePosition();
                var pipe = _gameManager.ConnectionManager.GetPipeAtPosition(mousePos);
                _gameManager.ConnectionManager.SetHoveredPipe(pipe);
            }
            else
            {
                _gameManager.ConnectionManager.SetHoveredPipe(null);
            }
        }
        else
        {
            _gameManager.ConnectionManager.SetHoveredPipe(null);
        }
    }

    // KEEP THESE PUBLIC FOR INPUT FRIENDS
    public BaseBlock? GetBlockAtPosition(Vector2 position)
    {
        // Simple collision check for all blocks
        var query = new PhysicsPointQueryParameters2D();
        query.Position = position;
        query.CollideWithAreas = true;
        query.CollideWithBodies = false;

        var result = GetTree().Root.GetWorld2D().DirectSpaceState.IntersectPoint(query);
        foreach (var collision in result)
            if (collision["collider"].As<Node>()?.GetParent() is BaseBlock block)
                return block;

        return null;
    }

    public void HandleBlockClick(BaseBlock block)
    {
        if (block == _draggedBlock)
        {
            // Let input handler manage connections and dragging
            _dragHandler?.StopDragging(block);
            _draggedBlock = null;
        }
        else
        {
            // Start dragging
            _draggedBlock = block;
            _dragHandler?.StartDragging(block);
        }
    }

    private BaseBlock? FindBlockUnderMouse()
    {
        var mousePos = GetViewport().GetMousePosition();
        return GetBlockAtPosition(mousePos);
    }

    public BaseBlock? GetDraggedBlock()
    {
        return _draggedBlock;
    }

    public void SetDraggedBlock(BaseBlock? block)
    {
        _draggedBlock = block;
        _isDraggingFromPipe = block != null && !block.InToolbar;
    }
}