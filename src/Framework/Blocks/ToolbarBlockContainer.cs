using F.Framework.Core;
using F.Framework.Logging;
using Godot;

namespace F.Framework.Blocks;

public partial class ToolbarBlockContainer : Container
{
    public override void _Ready()
    {
        base._Ready();
        Logger.UI.Print($"ToolbarBlockContainer {Name} ready");
    }

    public void AddBlock(BlockMetadata metadata)
    {
        var block = BlockFactory.CreateToolbarBlock(metadata, this);
        if (block == null)
        {
            Logger.UI.Err($"ToolbarBlockContainer {Name} failed to create toolbar block");
            return;
        }

        Logger.UI.Print($"ToolbarBlockContainer {Name} added block {block.Name}");
    }
}