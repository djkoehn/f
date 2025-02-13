using Godot;
using System.Collections.Generic;
using System.Linq;
using F.UI;

namespace F;

public partial class Toolbar : Control
{
    private const float HOVER_THRESHOLD = 0.8f;  // Show toolbar when mouse in bottom 20% of screen
    private const float ANIMATION_DURATION = 0.3f;
    private bool _isVisible = false;  // Start visible
    private float _currentOffset = 150f;  // Match initial offset_top in scene
    private float _targetOffset = 150f;
    private float _animationTime = 0f;
    private Control? _backgroundContainer;  // Container for background/animation
    private Container? _blockContainer;  // Container for blocks that stays in place
    private Inventory? _inventory;
    private GameManager? _gameManager;
    private BaseBlock? _returningBlock;
    private Vector2 _returnStartPos;
    private Vector2 _returnStartToolbarPos;  // Add this with other private fields at top
    private Vector2 _returnTargetPos;
    private float _returnTime = 0f;
    private const float RETURN_ANIMATION_DURATION = 0.5f;
    private float _containerWidth = 0f;
    private float _targetContainerWidth = 0f;
    private float _containerAnimTime = 0f;
    private const float CONTAINER_ANIMATION_DURATION = 0.5f;
    private Animations.BlockReturnAnimation? _returnAnimation;
    
    public override void _Ready()
    {
        GD.Print("Toolbar Ready");

        // Set up the main toolbar control
        AnchorLeft = 0;
        AnchorRight = 1;
        AnchorTop = 1;
        AnchorBottom = 1;
        GrowHorizontal = GrowDirection.Both;
        GrowVertical = GrowDirection.Begin;
        CustomMinimumSize = new Vector2(0, 150);  // Match initial height
        ZIndex = AnimConfig.ZIndex.Toolbar;  // Ensure toolbar is below other layers
        
        // Create background container that will animate
        _backgroundContainer = new ColorRect();
        ((ColorRect)_backgroundContainer).Color = new Color(0.1f, 0.1f, 0.1f);
        _backgroundContainer.AnchorLeft = 0;
        _backgroundContainer.AnchorRight = 1;
        _backgroundContainer.AnchorTop = 0;
        _backgroundContainer.AnchorBottom = 1;
        _backgroundContainer.GrowHorizontal = GrowDirection.Both;
        _backgroundContainer.GrowVertical = GrowDirection.Both;
        AddChild(_backgroundContainer);
        
        // Get the existing BlockContainer from the scene
        _blockContainer = GetNode<Container>("BlockContainer");
        if (_blockContainer != null)
        {
            // Ensure proper spacing between blocks
            _blockContainer.AddThemeConstantOverride("separation", (int)AnimConfig.Toolbar.BlockSpacing);
            
            // Set up container properties for automatic centering
            _blockContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
            _blockContainer.MouseFilter = Control.MouseFilterEnum.Ignore;
            
            // Connect to container update events
            _blockContainer.ChildEnteredTree += (_) => UpdateBlockPositions();
            _blockContainer.ChildExitingTree += (_) => UpdateBlockPositions();
        }
        
        // Get required nodes
        _inventory = GetNode<Inventory>("../Inventory");
        _gameManager = GetNode<GameManager>("..");
        
        if (_inventory == null || _gameManager == null || _blockContainer == null)
        {
            GD.PrintErr("Required nodes not found!");
            return;
        }
        
        // Connect to inventory ready signal
        _inventory.InventoryReady += CreateToolbarBlocks;
        
        // Create blocks if inventory is already ready
        if (_inventory.IsReady)
        {
            CreateToolbarBlocks();
        }

        // Start fully offscreen
        _currentOffset = AnimConfig.Toolbar.OffscreenOffset;
        _targetOffset = AnimConfig.Toolbar.OffscreenOffset;
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Position = new Vector2(0, GetViewport().GetVisibleRect().Size.Y - Size.Y + _currentOffset);
    }
    
    private void UpdateBlockPositions()
    {
        if (_blockContainer == null) return;
        
        var blocks = _blockContainer.GetChildren().Cast<BaseBlock>().ToArray();
        
        // Calculate total width for centering
        float blockWidth = GameConfig.BLOCK_SIZE;
        float spacing = AnimConfig.Toolbar.BlockSpacing;
        float newWidth = blocks.Length > 0 
            ? (blockWidth * blocks.Length) + (spacing * (blocks.Length - 1))
            : 0f;
            
        // Start container width animation if width changed
        if (Mathf.Abs(_targetContainerWidth - newWidth) > 0.01f)
        {
            _targetContainerWidth = newWidth;
            _containerAnimTime = 0f;
        }
        
        // Always center the container horizontally in the window
        var viewportWidth = GetViewport().GetVisibleRect().Size.X;
        _blockContainer.Position = new Vector2(viewportWidth / 2f, _blockContainer.Position.Y);
        
        // Calculate current positions based on current animated width
        if (blocks.Length > 0)
        {
            float startX = -_containerWidth / 2f + blockWidth / 2f;
            for (int i = 0; i < blocks.Length; i++)
            {
                var block = blocks[i];
                // Skip position update if this block is being animated
                if (_returnAnimation?.Block == block) continue;
                
                block.Position = new Vector2(startX + (i * (blockWidth + spacing)), 0);
            }
        }
        
        // Update container size
        _blockContainer.CustomMinimumSize = new Vector2(_containerWidth, _blockContainer.CustomMinimumSize.Y);
    }
    
    private void CreateToolbarBlocks()
    {
        GD.Print("Creating toolbar blocks");
        
        if (_inventory == null || _blockContainer == null || _gameManager == null) return;
        
        // Clear existing blocks
        foreach (Node child in _blockContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        // Add blocks from inventory
        var blocks = _inventory.GetAllBlocks();
        var blockList = blocks.Select(kvp => (kvp.Key, kvp.Value)).ToArray();
        
        for (int i = 0; i < blockList.Length; i++)
        {
            var (id, metadata) = blockList[i];
            var block = _gameManager.CreateBlock(metadata, _blockContainer);
            if (block == null)
            {
                GD.PrintErr($"Failed to create block: {metadata.Id}");
                continue;
            }
            
            block.SetProcessInput(true);  // Enable input processing
            block.Scale = Vector2.One * 1.0f;  // Replaced AnimConfig.Toolbar.BlockScale with 1.0f
            block.ZIndex = AnimConfig.ZIndex.Block;  // Set initial z-index
            block.Connect(nameof(BaseBlock.BlockPlaced), new Callable(this, nameof(OnBlockClicked)));
        }
        
        UpdateBlockPositions();
    }

    public void ReturnBlockToToolbar(BaseBlock block)
    {
        if (_blockContainer == null) return;
        
        // First, ensure block isn't already being returned
        if (_returnAnimation?.Block == block) return;
        
        var oldParent = block.GetParent();
        if (oldParent == _blockContainer) return;

        // Clear any existing dragged block reference in GameManager
        if (_gameManager?.GetDraggedBlock() == block)
        {
            _gameManager.HandleBlockDrop();
        }

        // Reset block state
        block.SetDragging(false);
        block.SetInBlockLayer(false);
        block.SetProcessInput(true);
        block.ZIndex = AnimConfig.ZIndex.Block;

        // Get connection layer and clear connections before proceeding
        var connectionLayer = GetNode<ConnectionLayer>("../ConnectionLayer");
        if (connectionLayer != null)
        {
            connectionLayer.HandleBlockDrag(block, new Vector2(-9999, -9999));
            connectionLayer.RemoveBlockConnections(block);
        }

        // Store initial position before changing parents
        Vector2 startPos = block.GlobalPosition;

        // Remove from old parent and add to container
        if (oldParent != null)
        {
            oldParent.RemoveChild(block);
        }
        _blockContainer.AddChild(block);

        // Calculate final position
        var blockWidth = GameConfig.BLOCK_SIZE;
        var spacing = AnimConfig.Toolbar.BlockSpacing;
        var existingBlocks = _blockContainer.GetChildren().Count() - 1; // -1 because we just added the block
        var newTotalWidth = ((existingBlocks + 1) * blockWidth) + (spacing * existingBlocks);
        
        // Update container width
        _targetContainerWidth = newTotalWidth;
        _containerWidth = newTotalWidth;
        
        // Calculate target position
        float startX = -newTotalWidth / 2f + blockWidth / 2f;
        int blockIndex = existingBlocks;
        Vector2 localTargetPos = new Vector2(
            startX + (blockIndex * (blockWidth + spacing)),
            0
        );
        
        // Set initial position and create animation
        block.GlobalPosition = startPos;
        _returnAnimation = Animations.CreateBlockReturnAnimation(block, startPos, _blockContainer.GlobalPosition + localTargetPos);
        
        // Update positions of other blocks
        UpdateBlockPositions();
    }

    private void OnBlockClicked(BaseBlock block)
    {
        if (_blockContainer == null || _gameManager == null) return;
        
        GD.Print($"Handling block click: {block.Name}");
        
        var connectionLayer = _gameManager.ConnectionLayer;

        // Check if there's already a block being dragged
        if (_gameManager.GetDraggedBlock() != null)
        {
            GD.Print("Cannot pick up a new block while one is being dragged");
            return;
        }

        if (connectionLayer == null)
        {
            GD.PrintErr("ConnectionLayer not found!");
            return;
        }

        // Move block to connection layer with highest z-index initially
        _blockContainer.RemoveChild(block);
        connectionLayer.AddChild(block);
        block.GlobalPosition = GetViewport().GetMousePosition();
        block.Scale = Vector2.One;
        block.ZIndex = AnimConfig.ZIndex.DraggedBlock;  // Start with highest z-index
        
        // Set block layer state before drag state
        block.SetInBlockLayer(true);
        
        // Now start the drag
        _gameManager.HandleBlockDrag(block);
        block.SetDragging(true);
        
        GD.Print($"Block {block.Name} moved to ConnectionLayer at {block.GlobalPosition}");
    }
    
    public override void _Process(double delta)
    {
        if (_blockContainer == null || _backgroundContainer == null) return;
        
        var mousePos = GetViewport().GetMousePosition();
        var viewportSize = GetViewport().GetVisibleRect().Size;
        
        // Determine if toolbar should be visible
        var shouldBeVisible = mousePos.Y > viewportSize.Y * HOVER_THRESHOLD;
        
        if (shouldBeVisible != _isVisible)
        {
            _isVisible = shouldBeVisible;
            _targetOffset = _isVisible ? 0f : AnimConfig.Toolbar.OffscreenOffset;
            _animationTime = 0f;
        }

        // Animate toolbar position
        if (_animationTime < ANIMATION_DURATION)
        {
            _animationTime += (float)delta;
            float t = Mathf.Min(_animationTime / ANIMATION_DURATION, 1.0f);
            t = BackEaseOut(t);
            _currentOffset = Mathf.Lerp(_currentOffset, _targetOffset, t);
            UpdatePosition();
        }
        
        // Animate container width (only when not returning a block)
        if (_containerAnimTime < CONTAINER_ANIMATION_DURATION && _returnAnimation == null)
        {
            _containerAnimTime += (float)delta;
            float t = Mathf.Min(_containerAnimTime / CONTAINER_ANIMATION_DURATION, 1.0f);
            t = 1 - Mathf.Pow(1 - t, 3);
            _containerWidth = Mathf.Lerp(_containerWidth, _targetContainerWidth, t);
            UpdateBlockPositions();
        }
        else
        {
            // Always ensure container is centered even when not animating
            UpdateBlockPositions();
        }

        // Handle block return animation
        if (_returnAnimation != null)
        {
            _returnAnimation.Update((float)delta);
            if (_returnAnimation.IsComplete)
            {
                _returnAnimation = null;
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Right)
            {
                var connectionLayer = GetNode<ConnectionLayer>("../ConnectionLayer");
                if (connectionLayer == null) return;
                
                var mousePos = GetViewport().GetMousePosition();
                
                foreach (var child in connectionLayer.GetChildren())
                {
                    if (child is BaseBlock block && 
                        child.Name != "Input" && 
                        child.Name != "Output")
                    {
                        var blockRect = block.GetRect();
                        if (blockRect.HasPoint(mousePos))
                        {
                            ReturnBlockToToolbar(block);
                            GetViewport().SetInputAsHandled();
                            break;
                        }
                    }
                }
            }
            else if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                // Check if dragging block over toolbar area
                var mousePos = GetViewport().GetMousePosition();
                if (ShouldReturnBlock(mousePos) && _gameManager?.GetDraggedBlock() != null)
                {
                    if (_gameManager.GetDraggedBlock() is BaseBlock block)
                    {
                        ReturnBlockToToolbar(block);
                        GetViewport().SetInputAsHandled();
                    }
                }
            }
        }
    }

    private float BackEaseOut(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;

        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

    private bool ShouldReturnBlock(Vector2 position)
    {
        return position.Y > GetViewport().GetVisibleRect().Size.Y * AnimConfig.Toolbar.HoverThreshold;
    }
}
