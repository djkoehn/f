using F.Framework.Core;
using F.Framework.Logging;
using Godot;

namespace F.Framework.Blocks;

public static class BlockFactory
{
    private static int _blockIdCounter;

    public static BaseBlock? CreateBlock(BlockMetadata metadata, Node parent)
    {
        try
        {
            var scene = GD.Load<PackedScene>(metadata.Scene);
            if (scene == null)
            {
                Logger.Block.Err($"Failed to load scene at path: {metadata.Scene}");
                return null;
            }

            var block = scene.Instantiate<BaseBlock>();
            if (block == null)
            {
                Logger.Block.Err($"Failed to instantiate block from scene: {metadata.Scene}");
                return null;
            }

            if (block.IsInsideTree())
            {
                block.GetParent()?.RemoveChild(block);
                Logger.Block.Print($"Removed block {block.Name} from previous parent");
            }

            parent.AddChild(block);
            Logger.Block.Print($"Added block {block.Name} to parent {parent.Name}");

            block.Initialize(metadata);
            Logger.Block.Print($"Initialized block {block.Name} with metadata");

            return block;
        }
        catch (Exception e)
        {
            Logger.Block.Err($"Error creating block: {e.Message}", e);
            return null;
        }
    }

    private static int GetUniqueBlockId()
    {
        return _blockIdCounter++;
    }

    // Create a toolbar block by creating the block and then applying toolbar configuration
    public static BaseBlock? CreateToolbarBlock(BlockMetadata metadata, Node parent)
    {
        var block = CreateBlock(metadata, parent);
        if (block != null)
        {
            block.SetInToolbar(true);
            Logger.Block.Print($"Configured block {block.Name} as toolbar block");
        }
        return block;
    }

    // Configures common block properties for toolbar blocks
    private static void ConfigureBlock(BaseBlock block)
    {
        block.Position = Vector2.Zero;
        block.Scale = Vector2.One;
        block.Rotation = 0;
    }
}