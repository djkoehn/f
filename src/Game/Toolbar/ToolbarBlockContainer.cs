using F.Framework.Blocks;
using F.Framework.Core.SceneTree;
using F.Framework.Core.Interfaces;
using F.Framework.Logging;

namespace F.Game.Toolbar;

public partial class ToolbarBlockContainer : HBoxContainer, IToolbarBlockContainer
{
    private readonly List<BaseBlock> _blocks = new();

    public override void _Ready()
    {
        // Allow child block nodes to receive input events
        MouseFilter = MouseFilterEnum.Pass;

        // Set initial size based on config
        CustomMinimumSize = new Vector2(0, ToolbarConfig.Block.Height);

        Logger.UI.Print("Initialized with input pass-through enabled");
    }

    public void ClearBlocks()
    {
        // Clear child blocks
        foreach (var child in GetChildren())
            if (child is Node node)
            {
                if (child is BaseBlock block)
                {
                    block.SetInToolbar(false);
                    _blocks.Remove(block);
                }

                node.QueueFree();
            }

        UpdateBlockPositions();
    }

    public void AddBlock(BaseBlock block)
    {
        if (!HasBlock(block))
        {
            AddChild(block);
            if (block.Metadata != null) Logger.UI.Print($"Added block {block.Name} to toolbar");
        }
        else
        {
            Logger.UI.Err($"Block {block.Name} is already in the toolbar");
        }
    }

    public void AddBlockWithoutAnimation(BaseBlock block)
    {
        // Add the block to the scene tree first
        if (block.GetParent() != null) block.GetParent().RemoveChild(block);

        if (!_blocks.Contains(block))
        {
            _blocks.Add(block);
            AddChild(block);
            block.ZIndex = ZIndexConfig.Layers.ToolbarBlock;
            block.SetInToolbar(true);
            UpdateBlockPositions();
        }
        else
        {
            Logger.UI.Err($"Block {block.Name} is already in the toolbar");
        }
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

        var x = blocks.Last().Position.X + ToolbarConfig.Block.Width + ToolbarConfig.Block.Spacing;
        return new Vector2(x, blocks.Last().Position.Y);
    }

    public void RemoveBlock(BaseBlock block)
    {
        if (HasBlock(block))
        {
            RemoveChild(block);
        }
        else
        {
            Logger.UI.Err($"Block {block.Name} is already in the toolbar");
        }
    }

    public void UpdateBlockPositions()
    {
        var blocks = GetChildren().OfType<BaseBlock>().ToList();
        var count = blocks.Count;
        if (count == 0) return;

        var totalWidth = count * ToolbarConfig.Block.Width + (count - 1) * ToolbarConfig.Block.Spacing;
        // Center the blocks in the container
        var startX = -totalWidth / 2f;

        for (var i = 0; i < count; i++)
            // Set X position for spacing and Y to 0
            blocks[i].Position = new Vector2(
                startX + i * (ToolbarConfig.Block.Width + ToolbarConfig.Block.Spacing),
                ToolbarConfig.Block.Height / 2f
            );
    }

    public void UpdateContainerSize()
    {
        var blocks = GetChildren().OfType<BaseBlock>().ToList();
        var count = blocks.Count;

        var totalWidth = count * (ToolbarConfig.Block.Width + ToolbarConfig.Block.Spacing);
        Size = new Vector2(totalWidth, ToolbarConfig.Block.Height);

        // Center the container in the toolbar
        var toolbar = GetParent<Toolbar>();
        if (toolbar != null)
        {
            var toolbarVisuals = toolbar.GetNode<ToolbarVisuals>("ToolbarVisuals");
            if (toolbarVisuals != null)
                Position = new Vector2(
                    toolbarVisuals.Size.X / 2f + ToolbarConfig.Block.Spacing,
                    ToolbarConfig.Layout.ContainerOffset
                );
        }
    }

    public bool HasBlock(BaseBlock block)
    {
        return block.GetParent() == this;
    }
}