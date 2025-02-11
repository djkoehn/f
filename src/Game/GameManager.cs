using Godot;
using System.Collections.Generic;
using System.Linq;

namespace F;

public partial class GameManager : Node2D
{
    public static GameManager? Instance { get; private set; }
    public ConnectionLayer? ConnectionLayer { get; private set; }

    private Node2D? _blockLayer;
    private BaseBlock? _draggedBlock;
    private Inventory? _inventory;
    
    public override void _Ready()
    {
        Instance = this;
        
        // Get required components
        ConnectionLayer = GetNode<ConnectionLayer>("ConnectionLayer");
        _inventory = GetNode<Inventory>("Inventory");
        
        if (ConnectionLayer == null || _inventory == null)
        {
            GD.PrintErr("Required components not found!");
            return;
        }
        
        // Connect signals
        _inventory.InventoryReady += OnInventoryReady;
        ConnectionLayer.TokenProcessed += OnTokenProcessed;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }
    
    private void OnInventoryReady()
    {
        GD.Print("Initializing blocks from inventory");
        
        // Initialize blocks with inventory data
        foreach (var block in GetBlocks())
        {
            var metadata = _inventory?.GetBlock(block.Name);
            if (metadata != null)
            {
                GD.Print($"Initializing block {block.Name} with ID {metadata.Id}");
                block.Initialize(metadata);
            }
            else
            {
                GD.PrintErr($"No metadata found for block {block.Name}");
            }
        }
    }
    
    private void OnTokenProcessed(float value)
    {
        // Handle token completion
        GD.Print($"Token completed with value: {value}");
        // TODO: Update score, check win condition, etc.
    }
    
    public override void _Process(double delta)
    {
        UpdateDraggedBlock();
    }
    
    private void UpdateDraggedBlock()
    {
        if (_draggedBlock != null && _draggedBlock.IsBeingDragged)
        {
            var mousePos = GetGlobalMousePosition();
            _draggedBlock.GlobalPosition = mousePos;
            
            // Check for pipe hover
            if (ConnectionLayer != null)
            {
                ConnectionLayer.HandleBlockDrag(_draggedBlock, mousePos);
            }
        }
    }
    
    private IEnumerable<BaseBlock> GetBlocks()
    {
        return _blockLayer?.GetChildren().OfType<BaseBlock>() ?? Enumerable.Empty<BaseBlock>();
    }
    
    public BaseBlock? CreateBlock(BlockMetadata metadata, Node? parent = null)
    {
        parent ??= _blockLayer;
        if (parent == null) return null;
        
        var scene = GD.Load<PackedScene>(metadata.ScenePath);
        if (scene == null)
        {
            GD.PrintErr($"Failed to load scene: {metadata.ScenePath}");
            return null;
        }
        
        var instance = scene.Instantiate();
        if (instance is BaseBlock block)
        {
            block.Initialize(metadata);
            parent.AddChild(block);
            return block;
        }
        else
        {
            GD.PrintErr($"Failed to cast instantiated scene to BaseBlock: {metadata.ScenePath}");
            return null;
        }
    }
    
    public void HandleBlockDrag(BaseBlock block)
    {
        // Clear previous dragged block's state if exists
        if (_draggedBlock != null && _draggedBlock != block)
        {
            _draggedBlock.SetDragging(false);
        }

        GD.Print($"Starting drag for block: {block.Name}");
        _draggedBlock = block;
        block.SetDragging(true);
    }
    
    public void HandleBlockDrop()
    {
        if (_draggedBlock != null)
        {
            var mousePos = GetGlobalMousePosition();
            if (ConnectionLayer != null)
            {
                ConnectionLayer.HandleBlockDrop(_draggedBlock, mousePos);
            }
            _draggedBlock.SetDragging(false);
            _draggedBlock = null;
        }
    }

    public override void _Input(InputEvent @event)
    {
        // Empty or remove this method
    }

    public BaseBlock? GetDraggedBlock()
    {
        return _draggedBlock;
    }

    public ConnectionLayer? GetConnectionLayer()
    {
        return GetNode<ConnectionLayer>("ConnectionLayer");
    }
}
