namespace F.UI.Input;

using F.UI.Toolbar;

public partial class BlockInputHandler : Node
{
    private BlockInteractionManager? _blockManager;
    private BlockConnectionHandler? _connectionHandler;
    private BlockDragHandler? _dragHandler;
    private BlockToolbarHandler? _toolbarHandler;

    public override void _Ready()
    {
        _blockManager = GetNode<BlockInteractionManager>("/root/Main/GameManager/BlockInteractionManager");
        _toolbarHandler = GetNode<BlockToolbarHandler>("/root/Main/InputManager/BlockToolbarHandler");
        _connectionHandler = GetNode<BlockConnectionHandler>("/root/Main/InputManager/BlockConnectionHandler");
        _dragHandler = GetNode<BlockDragHandler>("/root/Main/InputManager/BlockDragHandler");
    }

    public override void _Input(InputEvent @event)
    {
        if (_blockManager == null) return;

        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Right)
                HandleRightClick(mouseEvent);
            else if (mouseEvent.ButtonIndex == MouseButton.Left) 
                HandleLeftClick(mouseEvent);
        }
        else if (@event is InputEventMouseMotion mouseMotion)
        {
            HandleMouseMotion(mouseMotion);
        }
    }

    private void HandleRightClick(InputEventMouseButton mouseEvent)
    {
        var block = _blockManager?.GetBlockAtPosition(mouseEvent.GlobalPosition);
        if (block != null)
        {
            _toolbarHandler?.ReturnBlockToToolbar(block);
            GetViewport().SetInputAsHandled();
        }
    }

    private void HandleLeftClick(InputEventMouseButton mouseEvent)
    {
        var block = _blockManager?.GetBlockAtPosition(mouseEvent.GlobalPosition);
        if (block == null) return;

        var currentlyDragging = block == _blockManager?.DraggedBlock;
        if (currentlyDragging)
        {
            var success = _connectionHandler?.TryConnectBlock(block, block.GlobalPosition) ?? false;
            if (success)
            {
                // Mark the block as placed and fully stop dragging
                block.SetPlaced(true);
                block.SetDragging(false);
                _dragHandler?.StopDragging(block);      // new: ensure dragging is stopped
                _blockManager?.SetDraggedBlock(null);     // new: clear dragged block
                GetViewport().SetInputAsHandled();
                return;
            }
            _dragHandler?.StopDragging(block);
            _blockManager?.SetDraggedBlock(null);
        }
        else
        {
            _dragHandler?.StartDragging(block);
            _blockManager?.SetDraggedBlock(block);
        }
        GetViewport().SetInputAsHandled();
    }

    private void HandleMouseMotion(InputEventMouseMotion mouseMotion)
    {
        var draggedBlock = _blockManager?.GetDraggedBlock();
        if (draggedBlock != null)
        {
            _dragHandler?.UpdateDragging(mouseMotion.GlobalPosition, draggedBlock);
            _connectionHandler?.HighlightPipeAtPosition(mouseMotion.GlobalPosition);
        }
    }
}