namespace F.UI.Toolbar;

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
}