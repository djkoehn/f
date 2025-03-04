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
            var block = GD.Load<PackedScene>(metadata.ScenePath).Instantiate<BaseBlock>();
            block.Metadata = metadata;
            return block;
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