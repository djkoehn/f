using F.UI.Animations;
using F.Game.Connections;
using F.Game.Core;
using F.Game.Toolbar;
using BlockReturn = F.UI.Animations.Blocks.BlockReturn;
using ToolbarHoverAnimation = F.UI.Animations.UI.ToolbarHoverAnimation;
using Godot;
using F.Framework.Core;
using F.Framework.Blocks;
using F.UI.Animations.Blocks;
using F.UI.Animations.UI;
using F.Framework.Core.SceneTree;

namespace F.Game.Toolbar;

public partial class Toolbar : Control, IToolbar
{
	private const float HOVER_THRESHOLD = 0.8f; // Show toolbar when mouse in bottom 20% of screen
	private ToolbarBlockContainer? _blockContainer;
	private bool _isVisible; // Start hidden
	private ToolbarVisuals? _visuals;
	private ToolbarHoverAnimation? _currentAnimation; // Changed from AnimationPlayer to ToolbarHoverAnimation
	private bool _isHovered;
	private Tween _showHideAnimation;
	private HashSet<string> _loadedBlocks = new HashSet<string>();

	public IToolbarVisuals ToolbarVisuals => _visuals ?? throw new System.Exception("ToolbarVisuals not initialized");
	public IToolbarBlockContainer BlockContainer => _blockContainer ?? throw new System.Exception("BlockContainer not initialized");

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

	private void InitializeGameManager()
	{
		_blockContainer = GetNode<ToolbarBlockContainer>("BlockContainer");
		_visuals = GetNode<ToolbarVisuals>("ToolbarVisuals");

		if (_blockContainer == null || _visuals == null)
		{
			GD.PrintErr("Required components not found!");
			return;
		}

		// Set spacing between blocks
		_blockContainer.AddThemeConstantOverride("separation", (int)ToolbarConfig.Block.Spacing);

		// Subscribe to inventory ready event
		Services.Instance.Inventory.InventoryReady += LoadBlocks;

		// Create blocks if inventory is already ready
		if (Services.Instance.Inventory.IsReady)
		{
			LoadBlocks();
		}
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

	public override void _ExitTree()
	{
		if (Services.Instance?.Inventory != null)
		{
			Services.Instance.Inventory.InventoryReady -= LoadBlocks;
		}
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

		// Handle connections before starting the return journey
		var connectionManager = Services.Instance.Connections;
		if (connectionManager != null)
		{
			// Get the input and output blocks before disconnecting
			var inputBlock = connectionManager.GetNode<Node>("Input") as IBlock;
			var outputBlock = connectionManager.GetNode<Node>("Output") as IBlock;

			// Disconnect the block being returned
			connectionManager.DisconnectBlock(block);

			// Re-establish the Input->Output connection if we have both blocks
			if (inputBlock != null && outputBlock != null)
			{
				connectionManager.ConnectBlocks(inputBlock, outputBlock);
				GD.Print($"Re-established connection between Input and Output blocks");
			}
		}

		// Calculate where block will go for animation
		var homePosition = _blockContainer.GetNextBlockPosition();

		// Start block's journey home
		var returnAnim = BlockReturn.Create(block, block.GlobalPosition, homePosition);
		block.GetParent().AddChild(returnAnim);

		returnAnim.ReturnCompleted += completedBlock =>
		{
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
