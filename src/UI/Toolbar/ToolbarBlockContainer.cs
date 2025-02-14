using F.Config.UI;
using F.Config.Visual;
using F.UI.Animations.UI;

namespace F.UI.Toolbar;

public partial class ToolbarBlockContainer : HBoxContainer
{
    [Signal]
    public delegate void BlockPositionsUpdatedEventHandler(float width);

    private readonly Dictionary<string, BaseBlock> _blocks = new();
    private ToolbarContainerAnimation? _currentAnimation;
    private GameManager? _gameManager;

    public override void _Ready()
    {
        base._Ready();
        _gameManager = GetNode<GameManager>("/root/Main/GameManager");

        if (_gameManager == null)
        {
            GD.PrintErr("GameManager not found in ToolbarBlockContainer!");
            return;
        }

        // Set up container properties
        Theme = new Theme();
        Theme.SetConstant("separation", "HBoxContainer", 40);
        Alignment = AlignmentMode.Center;
        MouseFilter = MouseFilterEnum.Ignore;
        SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        CustomMinimumSize = new Vector2(0, 256);
    }

    public override void _Process(double delta)
    {
        // Only update positions if we're not currently animating
        if (_currentAnimation == null || !IsInstanceValid(_currentAnimation)) UpdateBlockPositions();

        // REMOVE or comment the Y < 0 check to keep blocks in the connection layer
        /*
        foreach (var block in GetChildren().OfType<BaseBlock>())
        {
            if (block.Position.Y < 0 && block.GetParent() == this)
            {
                RemoveChild(block);
                // ...existing re-parent code...
            }
        }
        */
    }

    private void AddBlockToContainer(BaseBlock block)
    {
        if (block.GetParent() != null)
        {
            var parent = block.GetParent();
            // If block is in a CenterContainer, remove both
            if (parent is CenterContainer container)
            {
                container.GetParent()?.RemoveChild(container);
                container.QueueFree();
            }
            else
            {
                parent.RemoveChild(block);
            }
        }

        AddChild(block);
        block.SetInToolbar(true); // SET STATE WHEN ADDED!
        // Remove the forced center positioning
    }

    public void AddBlock(string blockType)
    {
        if (_gameManager == null) return;

        GD.Print("Adding block of type: " + blockType);

        var metadata = BlockMetadata.Create(blockType);
        if (metadata == null) return;

        // Load and instantiate the block scene
        var scene = GD.Load<PackedScene>(metadata.ScenePath);
        if (scene == null)
        {
            GD.PrintErr($"Failed to load block scene: {metadata.ScenePath}");
            return;
        }

        var block = scene.Instantiate<BaseBlock>();
        if (block == null)
        {
            GD.PrintErr($"Failed to instantiate block: {blockType}");
            return;
        }

        AddBlockToContainer(block);
        block.SetProcessInput(true);
        block.Scale = Vector2.One;
        block.ZIndex = ZIndexConfig.Layers.ToolbarBlock;

        _blocks[blockType] = block;
        UpdateBlockPositions();
    }

    public void AddBlock(BaseBlock block)
    {
        AddBlockToContainer(block);

        // MAKE BLOCK NORMAL SIZE AND ON TOP!
        block.Scale = Vector2.One;
        block.ZIndex = ZIndexConfig.Layers.ToolbarBlock;

        // Calculate EXACT position in line using REAL BLOCK SIZE
        var blocks = GetChildren().OfType<BaseBlock>().ToList();
        var blockIndex = blocks.Count - 1;

        var totalWidth = blocks.Count * ToolbarConfig.Block.Width +
                         (blocks.Count - 1) * ToolbarConfig.Block.Spacing;

        // Center everything using EXACT MEASUREMENTS
        var startX = -totalWidth / 2 + ToolbarConfig.Block.Width / 2;

        // Position all blocks in line with EXACT SPACING
        for (var i = 0; i < blocks.Count; i++)
        {
            blocks[i].Position = new Vector2(
                startX + i * (ToolbarConfig.Block.Width + ToolbarConfig.Block.Spacing),
                128
            );
            GD.Print($"Block {i} positioned at: {blocks[i].Position}");
        }

        _blocks[block.GetType().Name] = block;
        EmitSignal(SignalName.BlockPositionsUpdated, totalWidth);
    }

    public void AddBlockWithoutAnimation(BaseBlock block)
    {
        if (block.GetParent() == this) return;

        // SET TOOLBAR STATE IMMEDIATELY!
        block.SetInToolbar(true);

        // KEEP BLOCK WHERE IT IS AND MAKE IT PART OF TOOLBAR
        var globalPos = block.GlobalPosition;
        AddBlockToContainer(block);
        block.GlobalPosition = globalPos;

        // REST OF METHOD SAME...
        // SET FINAL RESTING POSITION WITHOUT ANIMATION
        var blocks = GetChildren().OfType<BaseBlock>().ToList();
        var totalWidth = blocks.Count * ToolbarConfig.Block.Width +
                         (blocks.Count - 1) * ToolbarConfig.Block.Spacing;
        var startX = -totalWidth / 2 + ToolbarConfig.Block.Width / 2;

        block.Position = new Vector2(
            startX + (blocks.Count - 1) * (ToolbarConfig.Block.Width + ToolbarConfig.Block.Spacing),
            ToolbarConfig.Block.YPosition
        );

        _blocks[block.GetType().Name] = block;
    }

    public void RemoveBlock(BaseBlock block)
    {
        // Remove the entire CenterContainer parent if it exists
        var container = block.GetParent()?.GetParent() as CenterContainer;
        if (container != null) container.QueueFree();

        var key = _blocks.FirstOrDefault(x => x.Value == block).Key;
        if (key != null) _blocks.Remove(key);

        UpdateBlockPositions();
    }

    // DELETE THIS - NO MORE LOCAL CLICK HANDLING
    // private void OnBlockClicked(BaseBlock block)
    // {
    //     if (_gameManager == null) return;
    //     ...
    // }

    public void ClearBlocks()
    {
        // Only remove BaseBlock children
        foreach (var child in GetChildren().OfType<BaseBlock>().ToList()) child.QueueFree();
        _blocks.Clear();
        UpdateBlockPositions();
    }

    private void UpdateBlockPositions()
    {
        var blocks = GetChildren().OfType<BaseBlock>()
            .Where(b => b.GetParent() == this)
            .ToList();

        var blockCount = blocks.Count;
        var totalWidth = blockCount * ToolbarConfig.Block.Width +
                         (blockCount - 1) * ToolbarConfig.Block.Spacing;

        // Calculate target positions for all blocks
        var startX = -totalWidth / 2 + ToolbarConfig.Block.Width / 2;
        var blockTargets = new List<(BaseBlock block, Vector2 targetPos)>();

        // Check which blocks need to move
        for (var i = 0; i < blockCount; i++)
        {
            var targetPos = new Vector2(
                startX + i * (ToolbarConfig.Block.Width + ToolbarConfig.Block.Spacing),
                ToolbarConfig.Block.YPosition
            );

            if (blocks[i].Position.DistanceSquaredTo(targetPos) > 1) blockTargets.Add((blocks[i], targetPos));
        }

        // If blocks need to move, animate them
        if (blockTargets.Count > 0)
        {
            if (_currentAnimation != null && IsInstanceValid(_currentAnimation)) _currentAnimation.QueueFree();
            _currentAnimation = ToolbarContainerAnimation.Create(blockTargets);
            AddChild(_currentAnimation);
        }

        CustomMinimumSize = new Vector2(totalWidth, CustomMinimumSize.Y);
        Size = CustomMinimumSize;
        ForceUpdateTransform();
    }

    public void PrepareSpaceForBlock()
    {
        var blocks = GetChildren().OfType<BaseBlock>().ToList();
        var newCount = blocks.Count + 1;

        var totalWidth = newCount * ToolbarConfig.Block.Width +
                         (newCount - 1) * ToolbarConfig.Block.Spacing;

        var startX = -totalWidth / 2 + ToolbarConfig.Block.Width / 2;
        var blockTargets = new List<(BaseBlock block, Vector2 targetPos)>();

        // Check if animation exists and is valid before trying to free it
        if (_currentAnimation != null)
        {
            if (IsInstanceValid(_currentAnimation)) _currentAnimation.QueueFree();
            _currentAnimation = null; // ALWAYS CLEAR REFERENCE
        }

        // Only create new animation if we have blocks to move
        if (blocks.Count > 0)
        {
            // Calculate targets...
            foreach (var block in blocks)
            {
                var targetPos = new Vector2(
                    startX + blocks.IndexOf(block) * (ToolbarConfig.Block.Width + ToolbarConfig.Block.Spacing),
                    ToolbarConfig.Block.YPosition
                );
                blockTargets.Add((block, targetPos));
            }

            // Create new animation
            _currentAnimation = ToolbarContainerAnimation.Create(blockTargets);
            AddChild(_currentAnimation);
        }
    }

    public Vector2 GetNextBlockPosition()
    {
        var blocks = GetChildren().OfType<BaseBlock>().ToList();
        var blockCount = blocks.Count;

        // Calculate position in container space
        var totalWidth = blockCount * ToolbarConfig.Block.Width +
                         (blockCount - 1) * ToolbarConfig.Block.Spacing;

        var startX = -totalWidth / 2 + ToolbarConfig.Block.Width / 2;
        var nextSlotX = startX + blockCount * (ToolbarConfig.Block.Width + ToolbarConfig.Block.Spacing);

        // Convert local to global using right Godot method!
        var localPos = new Vector2(nextSlotX, ToolbarConfig.Block.YPosition);
        var globalPos = GetGlobalPosition() + localPos; // USE GetGlobalPosition!

        GD.Print($"Next block home at local: {localPos}, global: {globalPos}");

        return globalPos;
    }

    public Vector2 GetNextAvailablePosition()
    {
        // Get global center position of container
        var globalContainerPos = GlobalPosition;
        var height = Size.Y / 2;
        return new Vector2(globalContainerPos.X + Size.X / 2, globalContainerPos.Y + height);
    }
}