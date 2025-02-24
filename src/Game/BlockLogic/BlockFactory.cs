using Godot;
using F.Framework.Core;
using F.Framework.Blocks;

namespace F.Game.Core;

public static class BlockFactory
{
    private static int _blockIdCounter = 0;

    public static BaseBlock? CreateBlock(BlockMetadata metadata, Node parent)
    {
        if (string.IsNullOrEmpty(metadata.ScenePath))
        {
            GD.PrintErr($"Block {metadata.Id} has no scene path");
            return null;
        }

        var scene = GD.Load<PackedScene>(metadata.ScenePath);
        if (scene == null)
        {
            GD.PrintErr($"Failed to load block scene: {metadata.ScenePath}");
            return null;
        }

        var block = scene.Instantiate<BaseBlock>();
        if (block == null)
        {
            GD.PrintErr($"Failed to instantiate block: {metadata.Id}");
            return null;
        }

        block.Name = metadata.Id + GetUniqueBlockId();
        block.Metadata = metadata;
        block.SetProcessInput(true);

        // Remove from previous parent if exists
        if (block.GetParent() != null)
        {
            GD.Print($"[BlockFactory] Removing block {block.Name} from previous parent {block.GetParent().Name}");
            block.GetParent().RemoveChild(block);
        }

        // Add to new parent
        GD.Print($"[BlockFactory] Adding block {block.Name} to parent {parent.Name}");
        parent.AddChild(block);

        return block;
    }

    private static int GetUniqueBlockId()
    {
        return _blockIdCounter++;
    }

    // Create a toolbar block by creating the block and then applying toolbar configuration.
    public static BaseBlock? CreateToolbarBlock(BlockMetadata metadata, Container toolbarContainer)
    {
        var block = CreateBlock(metadata, toolbarContainer);
        if (block != null)
        {
            GD.Print($"[BlockFactory] Configuring toolbar block {block.Name}");
            ConfigureBlock(block);
            block.SetInToolbar(true);
        }
        return block;
    }

    // Configures common block properties for toolbar blocks.
    private static void ConfigureBlock(BaseBlock block)
    {
        block.Position = Vector2.Zero;
        block.Scale = Vector2.One;
        block.Rotation = 0;
    }
}