using Godot;
using System.Collections.Generic;
using System.Linq;

namespace F;

public partial class GameManager : Node2D
{
    private Node2D? _blockLayer;
    private ConnectionLayer? _connectionLayer;
    private BaseBlock? _draggedBlock;
    private Inventory? _inventory;
    
    public override void _Ready()
    {
        // Get required layers
        _blockLayer = GetNode<Node2D>("BlockLayer");
        _connectionLayer = GetNode<ConnectionLayer>("ConnectionLayer");
        _inventory = GetNode<Inventory>("Inventory");
        
        if (_blockLayer == null || _connectionLayer == null || _inventory == null)
        {
            GD.PrintErr("Required layers or inventory not found!");
            return;
        }
        
        // Connect to signals
        _inventory.InventoryReady += OnInventoryReady;
        _connectionLayer.TokenProcessed += OnTokenProcessed;
        
        // Initialize if inventory is already ready
        if (_inventory.IsReady)
        {
            OnInventoryReady();
        }
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
        if (_draggedBlock != null)
        {
            _draggedBlock.GlobalPosition = GetGlobalMousePosition();
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
        _draggedBlock = block;
    }
    
    public void HandleBlockDrop()
    {
        if (_draggedBlock != null)
        {
            _draggedBlock.EmitSignal(BaseBlock.SignalName.BlockPlaced, _draggedBlock);
            _draggedBlock = null;
        }
    }
}
