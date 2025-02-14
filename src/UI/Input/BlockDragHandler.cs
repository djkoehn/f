using F.Config.Visual;

namespace F.UI.Input;

public partial class BlockDragHandler : Node
{
    private BlockInteractionManager? _blockManager;
    private GameManager? _gameManager;

    public override void _Ready()
    {
        _blockManager = GetNode<BlockInteractionManager>("/root/Main/GameManager/BlockInteractionManager");
        _gameManager = GetNode<GameManager>("/root/Main/GameManager");
    }

    public void StartDragging(BaseBlock block)
    {
        if (_blockManager == null || _gameManager?.ConnectionManager == null) return;

        var wasInToolbar = block.InToolbar; // Use the property instead of checking parent

        block.SetPlaced(false);
        block.SetDragging(true);

        // Move to connection layer and update block manager
        var currentParent = block.GetParent();
        currentParent?.RemoveChild(block);
        _gameManager.ConnectionManager.AddChild(block);
        _blockManager.SetDraggedBlock(block); // Tell manager about dragged block

        block.ZIndex = ZIndexConfig.Layers.DraggedBlock; // Always use dragged Z-index
        block.ZAsRelative = false;

        // Set position to mouse
        block.GlobalPosition = _blockManager.GetViewport().GetMousePosition();

        // Disable collision
        var collision = block.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        if (collision != null) collision.Disabled = true;
    }

    public void UpdateDragging(Vector2 position, BaseBlock block)
    {
        block.GlobalPosition = position;
    }

    public void StopDragging(BaseBlock block)
    {
        if (_blockManager == null) return;

        block.SetDragging(false);
        block.SetPlaced(true);
        _blockManager.SetDraggedBlock(null); // Clear dragged block in manager

        var collision = block.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");
        if (collision != null) collision.Disabled = false;
    }
}