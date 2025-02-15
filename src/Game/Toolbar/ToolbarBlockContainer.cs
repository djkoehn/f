namespace F.Game.Toolbar
{
    public partial class ToolbarBlockContainer : Control
    {
        public override void _Ready()
        {
            // Allow child block nodes to receive input events
            MouseFilter = Control.MouseFilterEnum.Ignore;
        }

        public void ClearBlocks()
        {
            // Clear child blocks
            foreach (var child in GetChildren())
            {
                if (child is Node node)
                    node.QueueFree();
            }
            UpdateBlockPositions();
        }

        public void AddBlock(BaseBlock block)
        {
            ToolbarHelper.ReturnBlockToToolbar(block, this);
            UpdateContainerSize();
        }

        public void AddBlockWithoutAnimation(BaseBlock block)
        {
            ToolbarHelper.ReturnBlockToToolbar(block, this);
        }

        public void PrepareSpaceForBlock()
        {
            // Stub method to prepare space for a block
        }

        public Vector2 GetNextBlockPosition()
        {
            var blocks = GetChildren().OfType<BaseBlock>().ToList();
            if (blocks.Count == 0)
                return GlobalPosition;
            // Assume block width of 100 and spacing of 40
            float blockWidth = 100f;
            float spacing = 40f;
            float x = blocks.Last().Position.X + blockWidth + spacing;
            return new Vector2(x, blocks.Last().Position.Y);
        }

        public void RemoveBlock(BaseBlock block)
        {
            if (block.GetParent() == this)
            {
                RemoveChild(block);
                UpdateBlockPositions();
                UpdateContainerSize();
            }
        }

        public void UpdateBlockPositions()
        {
            var blocks = GetChildren().OfType<BaseBlock>().ToList();
            float blockWidth = 100f; // default block width
            float spacing = 40f;     // default spacing
            int count = blocks.Count;
            if (count == 0) return;

            float totalWidth = count * blockWidth + (count - 1) * spacing;
            // Center the blocks in the container
            float startX = -totalWidth / 2f;
            
            for (int i = 0; i < count; i++)
            {
                // Keep current Y position
                Vector2 pos = blocks[i].Position;
                pos.X = startX + i * (blockWidth + spacing);
                blocks[i].Position = pos;
            }
        }

        private void UpdateContainerSize()
        {
            var blocks = GetChildren().OfType<BaseBlock>().ToList();
            float blockWidth = 100f; // default block width 
            float spacing = 40f;     // default spacing
            int count = blocks.Count;

            float totalWidth = count * (blockWidth + spacing);
            Size = new Vector2(totalWidth, Size.Y);

            // Center the container in the toolbar
            var toolbar = GetParent<Toolbar>();
            if (toolbar != null)
            {
                var toolbarVisuals = toolbar.GetNode<ToolbarVisuals>("ToolbarVisuals");
                if (toolbarVisuals != null)
                {
                    Position = new Vector2((toolbarVisuals.Size.X - totalWidth) / 2f, Position.Y);
                }
            }
        }
    }
} 