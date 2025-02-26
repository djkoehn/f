namespace F.Game.Toolbar;

public partial class ToolbarBlockContainer : Control, IBlockContainer
{
    public Vector2 GetNextBlockPosition()
    {
        var blocks = GetChildren().OfType<BaseBlock>().ToList();
        if (blocks.Count == 0)
            return GlobalPosition;

        var x = blocks.Last().Position.X + ToolbarConfig.Block.Width + ToolbarConfig.Block.Spacing;
        return new Vector2(x, blocks.Last().Position.Y);
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

    public override void _Ready()
    {
        // Allow child block nodes to receive input events
        MouseFilter = MouseFilterEnum.Ignore;

        // Set initial size based on config
        CustomMinimumSize = new Vector2(0, ToolbarConfig.Block.Height);
    }

    public void ClearBlocks()
    {
        // Clear child blocks
        foreach (var child in GetChildren())
            if (child is Node node)
                node.QueueFree();
        UpdateBlockPositions();
    }

    public void AddBlock(BaseBlock block)
    {
        // Add the block to the scene tree first
        AddChild(block);
        block.ZIndex = ZIndexConfig.Layers.ToolbarBlock;
        block.SetInToolbar(true);

        // Position the block
        UpdateBlockPositions();

        // Ensure the block's name is preserved
        if (block.Metadata != null) GD.Print($"[ToolbarBlockContainer] Added block {block.Name} to toolbar");

        UpdateContainerSize();
    }

    public void AddBlockWithoutAnimation(BaseBlock block)
    {
        // Add the block to the scene tree first
        AddChild(block);
        block.ZIndex = ZIndexConfig.Layers.ToolbarBlock;

        var hf = HelperFunnel.GetInstance();
        var toolbarHelper = hf?.GetNodeOrNull<ToolbarHelper>("ToolbarHelper");
        if (toolbarHelper != null)
            toolbarHelper.ReturnBlockToToolbar(block, this);
        else
            GD.PrintErr("ToolbarHelper instance not found in ToolbarBlockContainer.");
    }

    public void PrepareSpaceForBlock()
    {
        // Stub method to prepare space for a block
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
}