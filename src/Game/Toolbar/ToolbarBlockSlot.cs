using F.Framework.Blocks;

namespace F.Game.Toolbar;

public class ToolbarBlockSlot
{
    public ToolbarBlockSlot(BaseBlock block, Vector2 position)
    {
        Block = block;
        Position = position;
    }

    public BaseBlock Block { get; }
    public Vector2 Position { get; }
}