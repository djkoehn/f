using F.Framework.Blocks;
using F.Framework.Core.Interfaces;
using F.Framework.Core;
using F.Framework.Core.SceneTree;
using F.Framework.Core.Services;
using F.Game.Core;
using BlockReturn = F.UI.Animations.Blocks.BlockReturn;
using ToolbarHoverAnimation = F.UI.Animations.UI.ToolbarHoverAnimation;
using F.Framework.Logging;
using Godot;

namespace F.Game.Toolbar;

public partial class Toolbar : Control, IToolbar
{
    private const float HOVER_THRESHOLD = 0.8f; // Show toolbar when mouse in bottom 20% of screen
    private readonly HashSet<string> _loadedBlocks = new();
    private ToolbarBlockContainer? _blockContainer;
    private ToolbarHoverAnimation? _currentAnimation; // Changed from AnimationPlayer to ToolbarHoverAnimation
    private bool _isHovered;
    private bool _isVisible; // Start hidden
    private Tween _showHideAnimation;
    private ToolbarVisuals? _visuals;
    private Inventory? _inventory;

    public IToolbarVisuals ToolbarVisuals => _visuals ?? throw new Exception("ToolbarVisuals not initialized");

    public IToolbarBlockContainer BlockContainer =>
        _blockContainer ?? throw new Exception("BlockContainer not initialized");

    public override void _Ready()
    {
        base._Ready();

        // Bottom anchors
        AnchorLeft = 0;
        AnchorRight = 1;
        AnchorTop = 1;
        AnchorBottom = 1;
        GrowHorizontal = GrowDirection.Both;
        GrowVertical = GrowDirection.Begin;

        ZIndex = ZIndexConfig.Layers.Toolbar;

        // Ensure initial state matches position
        _isHovered = false;
        _isVisible = false;

        // Wait one frame to ensure all nodes are ready
        CallDeferred(nameof(InitializeGameManager));
    }

    public override void _ExitTree()
    {
        if (Services.Instance?.Inventory != null) Services.Instance.Inventory.InventoryReady -= LoadBlocks;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_visuals is null) return;

        // Get mouse position in viewport coordinates
        var mousePos = GetViewport().GetMousePosition();
        var viewportSize = GetViewport().GetVisibleRect().Size;

        // Calculate relative Y position (0 = top, 1 = bottom)
        var relativeY = mousePos.Y / viewportSize.Y;

        // Show toolbar when mouse is in bottom portion of screen
        var shouldShow = relativeY > HOVER_THRESHOLD;

        if (shouldShow != _isHovered)
        {
            _isHovered = shouldShow;

            _visuals?.StartHoverAnimation(shouldShow);
        }
    }

    public void ReturnBlockToToolbar(BaseBlock block)
    {
        Logger.UI.Print($"Starting return journey for block {block.Name}...");

        // First, check if this is a block that should be returned
        if (block.Metadata?.IsToolbarBlock != true)
        {
            Logger.UI.Print($"Block {block.Name} is not a toolbar block, skipping return");
            return;
        }

        // Reset the block's connections
        block.ResetConnections();

        // Move the block back to its original position
        block.SetInToolbar(true);
        Logger.UI.Print($"Block {block.Name} has returned home safely!");
    }

    private void InitializeGameManager()
    {
        _blockContainer = GetNode<ToolbarBlockContainer>("BlockContainer");
        _visuals = GetNode<ToolbarVisuals>("ToolbarVisuals");

        if (_blockContainer == null || _visuals == null)
        {
            Logger.UI.Err("Required components not found!");
            return;
        }

        // Set spacing between blocks
        _blockContainer.AddThemeConstantOverride("separation", (int)ToolbarConfig.Block.Spacing);

        // Wait for Services.Instance to be available
        if (Services.Instance?.Inventory == null)
        {
            Logger.UI.Print("Services.Instance.Inventory not available yet, deferring initialization");
            CallDeferred(nameof(InitializeGameManager));
            return;
        }

        // Subscribe to inventory ready event
        Services.Instance.Inventory.InventoryReady += LoadBlocks;

        // Create blocks if inventory is already ready
        if (Services.Instance.Inventory.IsReady) LoadBlocks();
    }

    private void LoadBlocks()
    {
        if (_blockContainer == null) return;

        // Clear existing blocks
        _blockContainer.ClearBlocks();
        _loadedBlocks.Clear();

        var blockMetadata = Services.Instance.Inventory.GetBlockMetadata();
        foreach (var pair in blockMetadata)
        {
            var block = BlockFactory.CreateBlock(pair.Value, _blockContainer);
            if (block != null)
            {
                block.Name = pair.Key;
                _blockContainer.AddBlock(block);
            }
        }

        _blockContainer.UpdateBlockPositions();
    }

    private void OnBlockPositionsUpdated(float width)
    {
        _visuals?.UpdateBlockPositions();
    }

    public bool IsPointInToolbar(Vector2 globalPoint)
    {
        return false;
    }

    public void AddBlock(BaseBlock block)
    {
        if (_blockContainer != null) _blockContainer.AddBlock(block);
    }

    public void RemoveBlock(BaseBlock block)
    {
        if (_blockContainer != null) _blockContainer.RemoveBlock(block);
    }

    public void ClearBlocks()
    {
        if (_blockContainer != null) _blockContainer.ClearBlocks();
    }

    private Vector2 GetBlockSpawnPosition()
    {
        if (_blockContainer == null)
            // Fallback position if container not found
            return GlobalPosition;

        // Return position relative to the block container
        return _blockContainer.GlobalPosition + new Vector2(0, -100);
    }
}