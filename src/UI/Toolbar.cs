using Godot;
using System.Collections.Generic;

namespace F;

public partial class Toolbar : Panel
{
    private const float HOVER_THRESHOLD = 0.8f;  // Show toolbar when mouse in bottom 20% of screen
    private bool _isVisible = true;  // Start visible
    private HBoxContainer? _blockContainer;
    private CenterContainer? _centerContainer;
    private Inventory? _inventory;
    private GameManager? _gameManager;
    
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
        _centerContainer.CustomMinimumSize = new Vector2(150, 130);
        _blockContainer.AddChild(_centerContainer);
        
        // Connect to inventory ready signal
        _inventory.InventoryReady += CreateToolbarBlocks;
        
        // Create blocks if inventory is already ready
        if (_inventory.IsReady)
        {
            CreateToolbarBlocks();
        }
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
            
            // Removed the line that set block as toolbar item
        }
    }
    
    public override void _Process(double delta)
    {
        if (_blockContainer == null) return;
        
        var mousePos = GetViewport().GetMousePosition();
        var viewportSize = GetViewport().GetVisibleRect().Size;
        
        // Show toolbar when mouse is in bottom portion of screen
        var shouldBeVisible = mousePos.Y > viewportSize.Y * HOVER_THRESHOLD;
        
        if (shouldBeVisible != _isVisible)
        {
            _isVisible = shouldBeVisible;
            _blockContainer.Visible = _isVisible;
        }
    }
}
