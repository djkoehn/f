using Godot;
using F.Framework.Blocks;

namespace F.Game.Toolbar;

public class ToolbarBlockSlot
{
    public BaseBlock Block { get; }
    public Vector2 Position { get; }

    public ToolbarBlockSlot(BaseBlock block, Vector2 position)
    {
        Block = block;
        Position = position;
    }
}