using GM = F.Game.Core.GameManager;
using F.Utils;
using Godot;

namespace F.Game.Core
{
    public class BlockFactory
    {
        private readonly GM _gameManager;

        public BlockFactory(GM gameManager)
        {
            _gameManager = gameManager;
        }

        // Create a block given metadata and a parent node.
        public BaseBlock? CreateBlock(F.Game.BlockLogic.BlockMetadata metadata, Node parent)
        {
            GD.Print($"[BlockFactory Debug] Creating block with metadata - Id: {metadata.Id}, Name: {metadata.Name}, SpawnOnSpace: {metadata.SpawnOnSpace}");
            var block = GD.Load<PackedScene>(metadata.ScenePath).Instantiate<BaseBlock>();
            block.Metadata = metadata;
            
            // Set a unique name based on the block type
            var uniqueName = $"{metadata.Name}{GetUniqueBlockId()}";
            block.Name = uniqueName;
            ((IBlock)block).Name = uniqueName;
            
            GD.Print($"[BlockFactory Debug] Created block {uniqueName} with metadata Id: {metadata.Id}, SpawnOnSpace: {metadata.SpawnOnSpace}");
            
            return block;
        }

        private int _blockIdCounter = 0;
        private int GetUniqueBlockId()
        {
            return _blockIdCounter++;
        }

        // Create a toolbar block by creating the block and then applying toolbar configuration.
        public BaseBlock? CreateToolbarBlock(F.Game.BlockLogic.BlockMetadata metadata, Container toolbarContainer)
        {
            var block = CreateBlock(metadata, toolbarContainer);
            if (block != null)
            {
                ConfigureBlock(block);
            }
            return block;
        }

        // Configures common block properties for toolbar blocks.
        private void ConfigureBlock(BaseBlock block)
        {
            block.Position = Vector2.Zero;
            block.Scale = Vector2.One;
            block.Rotation = 0;
            block.SetDragging(false);
            block.SetPlaced(false);
        }
    }
} 