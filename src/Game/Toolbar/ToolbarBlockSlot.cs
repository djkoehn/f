namespace F.Game.Toolbar;

public partial class ToolbarBlockSlot : Node2D // ADD PARTIAL KEYWORD!
{
    private BaseBlock? _block;

    private bool HasBlock()
    {
        return _block != null;
    }

    private void AddBlock(BaseBlock block)
    {
        _block = block;
        AddChild(block);
        block.GlobalPosition = GlobalPosition; // Position block at slot center
    }

    private void OnBlockClicked(BaseBlock block)
    {
        if (HasBlock())
        {
            _block!.State = BlockState.Dragging; // Change to Dragging state
            _block.ZIndex = 100; // Set z-index to a high value
            // ... existing code ...
        }
    }
}