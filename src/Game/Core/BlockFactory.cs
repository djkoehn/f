using Godot;
using F.Config;
using GM = F.Game.Core.GameManager;

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
            return _gameManager.CreateBlock(metadata, parent);
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

        // Return a block to the toolbar container, removing it from its current parent and applying configuration.
        public void ReturnBlockToToolbar(BaseBlock block, Container toolbarContainer)
        {
            var parent = block.GetParent();
            if (parent != null)
            {
                parent.RemoveChild(block);
            }
            toolbarContainer.AddChild(block);
            ConfigureBlock(block);
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