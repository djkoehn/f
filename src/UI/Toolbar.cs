using Godot;
using System.Collections.Generic;

namespace F;

public partial class Toolbar : Control
{
    private const float HOVER_THRESHOLD = 0.8f;  // Show toolbar when mouse in bottom 20% of screen
    private const float ANIMATION_DURATION = 0.3f;
    private bool _isVisible = false;  // Start visible
    private float _currentOffset = 150f;  // Match initial offset_top in scene
    private float _targetOffset = 150f;
    private float _animationTime = 0f;
    private HBoxContainer? _blockContainer;
    private CenterContainer? _centerContainer;
    private Inventory? _inventory;
    private GameManager? _gameManager;
    private BaseBlock? _returningBlock;
    private Vector2 _returnStartPos;
    private Vector2 _returnTargetPos;
    private float _returnTime = 0f;
    private const float RETURN_ANIMATION_DURATION = 0.5f;
    
    public override void _Ready()
    {
        GD.Print("Toolbar Ready");
        
        // Get required nodes
        _blockContainer = GetNode<HBoxContainer>("BlockContainer");
        _inventory = GetNode<Inventory>("../Inventory");
        _gameManager = GetNode<GameManager>("..");
        
        if (_blockContainer == null || _inventory == null || _gameManager == null)
        {
            GD.PrintErr("Required nodes not found!");
            return;
        }
        
        // Create center container for blocks
        _centerContainer = new CenterContainer();
        _centerContainer.SizeFlagsHorizontal = Control.SizeFlags.Fill;
        _centerContainer.SizeFlagsVertical = Control.SizeFlags.Fill;
        _blockContainer.AddChild(_centerContainer);
        
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
        Position = new Vector2(0, GetViewport().GetVisibleRect().Size.Y);
    }
    
    private void CreateToolbarBlocks()
    {
        GD.Print("Creating toolbar blocks");
        
        if (_inventory == null || _centerContainer == null || _gameManager == null) return;
        
        // Clear existing blocks
        foreach (Node child in _centerContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        // Add blocks from inventory
        var blocks = _inventory.GetAllBlocks();
        
        foreach (var (id, metadata) in blocks)
        {
            var block = _gameManager.CreateBlock(metadata, _centerContainer);
            if (block == null)
            {
                GD.PrintErr($"Failed to create block: {metadata.Id}");
                continue;
            }
            
            block.SetProcessInput(true);  // Enable input processing
            block.Scale = Vector2.One * AnimConfig.Toolbar.BlockScale;
            block.ZIndex = AnimConfig.ZIndex.Block;  // Set initial z-index
            block.Connect(nameof(BaseBlock.BlockPlaced), new Callable(this, nameof(OnBlockClicked)));
        }
    }

    private void ReturnBlockToToolbar(BaseBlock block)
    {
        if (_centerContainer == null || block.GetParent() == _centerContainer) return;
        
        // Important: First disconnect the block's pipes
        var connectionLayer = GetNode<ConnectionLayer>("../ConnectionLayer");
        if (connectionLayer != null)
        {
            // Handle pipe cleanup BEFORE starting return animation
            connectionLayer.HandleBlockRemoved(block);
            
            // Now start the return animation
            _returningBlock = block;
            _returnStartPos = block.GlobalPosition;
            _returnTime = 0f;
            
            // Calculate target position relative to viewport center
            Vector2 targetPos = new Vector2(
                GetViewportRect().Size.X / 2,
                GetViewportRect().Size.Y - (GetRect().Size.Y / 2)
            );
            
            _returnTargetPos = targetPos;
            _gameManager?.HandleBlockDrop();
        }
    }

    private void OnBlockClicked(BaseBlock block)
    {
        if (_centerContainer == null || _gameManager == null) return;
        
        GD.Print($"Handling block click: {block.Name}");
        
        var connectionLayer = _gameManager.ConnectionLayer;
        if (connectionLayer == null)
        {
            GD.PrintErr("ConnectionLayer not found!");
            return;
        }
        
        // Remove from toolbar container
        _centerContainer.RemoveChild(block);
        
        // Add to connection layer
        connectionLayer.AddChild(block);
        
        // Configure block
        block.GlobalPosition = GetViewport().GetMousePosition();
        block.Scale = Vector2.One;
        block.SetInBlockLayer(true);
        block.ZIndex = AnimConfig.ZIndex.DraggedBlock;
        
        // Start dragging
        _gameManager.HandleBlockDrag(block);
        
        GD.Print($"Block {block.Name} moved to ConnectionLayer at {block.GlobalPosition}");
    }
    
    public override void _Process(double delta)
    {
        if (_blockContainer == null) return;
        
        var mousePos = GetViewport().GetMousePosition();
        var viewportSize = GetViewport().GetVisibleRect().Size;
        
        // Determine if toolbar should be visible
        var shouldBeVisible = mousePos.Y > viewportSize.Y * HOVER_THRESHOLD;
        
        if (shouldBeVisible != _isVisible)
        {
            _isVisible = shouldBeVisible;
            _targetOffset = _isVisible ? 0f : 150f;
            _animationTime = 0f;
        }

        // Animate toolbar position
        if (_animationTime < ANIMATION_DURATION)
        {
            _animationTime += (float)delta;
            float t = Mathf.Min(_animationTime / ANIMATION_DURATION, 1.0f);
            // Back-easing animation
            t = BackEaseOut(t);
            _currentOffset = Mathf.Lerp(_currentOffset, _targetOffset, t);
            
            Position = new Vector2(0, GetViewport().GetVisibleRect().Size.Y - Size.Y + _currentOffset);
        }

        // Handle block return animation
        if (_returningBlock != null)
        {
            _returnTime += (float)delta;
            float t = Mathf.Min(_returnTime / RETURN_ANIMATION_DURATION, 1.0f);
            
            if (t < 1.0f)
            {
                float progress = Easing.OutElastic(t);
                
                // Calculate current toolbar offset for synchronized animation
                float toolbarOffset = Position.Y - (GetViewport().GetVisibleRect().Size.Y - Size.Y);
                Vector2 currentTargetPos = _returnTargetPos + new Vector2(0, toolbarOffset);
                
                _returningBlock.GlobalPosition = _returnStartPos.Lerp(currentTargetPos, progress);
                _returningBlock.Scale = Vector2.One.Lerp(Vector2.One * AnimConfig.Toolbar.BlockScale, progress);
            }
            else
            {
                // Animation complete, actually return the block
                var oldParent = _returningBlock.GetParent();
                oldParent?.RemoveChild(_returningBlock);
                _centerContainer?.AddChild(_returningBlock);
                
                _returningBlock.SetInBlockLayer(false);
                _returningBlock.SetProcessInput(true);
                _returningBlock.ResetState();
                _returningBlock.Position = Vector2.Zero;
                
                _returningBlock = null;
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
                        block != connectionLayer.InputBlock && 
                        block != connectionLayer.OutputBlock)
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
