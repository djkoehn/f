using F.Config.UI;
using F.Config.Visual;
using F.UI.Animations;
using F.Game.Connections;
using F.Game.Core;
using InventoryType = F.Game.Core.Inventory;
using F.Game.Toolbar;
using F.Utils.Helpers;

namespace F.Game.Toolbar;

public partial class Toolbar : Control
{
    private const float HOVER_THRESHOLD = 0.8f; // Show toolbar when mouse in bottom 20% of screen
    private ToolbarBlockContainer? _blockContainer; // Now correct type!
    private GameManager? _gameManager;
    private InventoryType? _inventory;
    private bool _isVisible; // Start hidden
    private ToolbarVisuals? _visuals;
    private ToolbarHoverAnimation? _currentAnimation; // Changed from AnimationPlayer to ToolbarHoverAnimation
    private bool _isHovered;

    public override void _Ready()
    {
        base._Ready();

        // Set initial position
        Position = new Vector2(0, ToolbarConfig.Animation.HideY);

        // Bottom anchors
        // AnchorLeft = 0;
        // AnchorRight = 1;
        // AnchorTop = 1;
        // AnchorBottom = 1;
        // GrowHorizontal = GrowDirection.Both;
        // GrowVertical = GrowDirection.Begin;

        ZIndex = ZIndexConfig.Layers.Toolbar;

        // Ensure initial state matches position
        _isHovered = false;
        _isVisible = false;

        // Wait one frame to ensure all nodes are ready
        CallDeferred(nameof(InitializeGameManager));
    }

    private void InitializeGameManager()
    {
        _gameManager = GetParent<GameManager>();
        if (_gameManager is null)
        {
            GD.PrintErr("GameManager not found!");
            return;
        }

        _inventory = _gameManager.GetNode<InventoryType>("Inventory");
        if (_inventory is null)
        {
            GD.PrintErr("Inventory not found!");
            return;
        }

        // Get direct child nodes
        _visuals = GetNode<ToolbarVisuals>("ToolbarVisuals");
        if (_visuals == null)
        {
            GD.PrintErr("ToolbarVisuals not found!");
            return;
        }

        _blockContainer = GetNode<ToolbarBlockContainer>("BlockContainer");
        if (_blockContainer == null)
        {
            GD.PrintErr("BlockContainer not found!");
            return;
        }

        // Connect to inventory ready signal
        _inventory.InventoryReady += LoadBlocks;

        // Create blocks if inventory is already ready
        if (_inventory.IsReady) LoadBlocks();

        _blockContainer.UpdateBlockPositions();
    }

    private void LoadBlocks()
    {
        if (_gameManager is null || _inventory is null || _blockContainer is null) return;

        GD.Print("Creating toolbar blocks");

        // Clear existing blocks first
        _blockContainer.ClearBlocks();

        // Let BlockContainer handle block creation and management
        var blocks = _inventory.GetBlockMetadata();
        foreach (var pair in blocks)
            // Instantiate the block using BlockManager with BlockLayer as parent
            if (_gameManager.ConnectionManager is not null)
            {
                var block = _gameManager.BlockFactory?.CreateBlock(pair.Value, _gameManager.ConnectionManager);
                if (block is not null) _blockContainer.AddBlock(block);
            }
            else
            {
                GD.PrintErr("ConnectionManager is null, cannot create block!");
            }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_gameManager is null || _visuals is null) return;

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

    private void OnBlockPositionsUpdated(float width)
    {
        _visuals?.UpdateBlockPositions();
    }

    public bool IsPointInToolbar(Vector2 globalPoint)
    {
        return false;
    }

    public void ReturnBlockToToolbar(BaseBlock block)
    {
        if (_blockContainer == null || block == null) return;

        GD.Print($"Starting return journey for block {block.Name}...");
        
        // First tell family to make room!
        _blockContainer.PrepareSpaceForBlock();
        
        // Calculate where block will go
        var homePosition = _blockContainer.GetNextBlockPosition();
        
        // Create new connections BEFORE starting return animation
        var connectionManager = _gameManager?.ConnectionManager;
        if (connectionManager != null)
        {
            connectionManager.RemoveConnection(block);
        }

        // Start block's journey home
        var returnAnim = BlockReturn.Create(block, block.GlobalPosition, homePosition);
        block.GetParent().AddChild(returnAnim);

        returnAnim.ReturnCompleted += completedBlock =>
        {
            // Remove from current parent
            var currentParent = completedBlock.GetParent();
            currentParent?.RemoveChild(completedBlock);

            // Add to toolbar without animation
            _blockContainer.AddBlockWithoutAnimation(completedBlock);
            GD.Print($"Block {completedBlock.Name} has returned home safely!");
        };
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