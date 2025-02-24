using F.Framework.Logging;
using Godot;

namespace F.Framework.Blocks;

public partial class ToolbarBlock : BaseBlock
{
    public override void _Ready()
    {
        base._Ready();
        Logger.UI.Print($"ToolbarBlock {Name} ready");
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Metadata == null) return;

        if (Input.IsActionJustPressed(Metadata.SpawnHotkey))
        {
            Logger.UI.Print($"ToolbarBlock {Name} spawn hotkey pressed: {Metadata.SpawnHotkey}");
            SpawnBlock();
        }
    }

    private void SpawnBlock()
    {
        if (Metadata == null) return;

        var blockLayer = GetNode<Node>("/root/Game/BlockLayer");
        if (blockLayer == null)
        {
            Logger.UI.Err($"ToolbarBlock {Name} failed to find BlockLayer");
            return;
        }

        var block = BlockFactory.CreateBlock(Metadata, blockLayer);
        if (block == null)
        {
            Logger.UI.Err($"ToolbarBlock {Name} failed to create block");
            return;
        }

        block.GlobalPosition = GetGlobalMousePosition();
        block.SetDragging(true);
        Logger.UI.Print($"ToolbarBlock {Name} spawned block at {block.GlobalPosition}");
    }
}