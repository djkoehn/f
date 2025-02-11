using Godot;
using System.Collections.Generic;

namespace F;

public partial class Inventory : Node
{
    [Signal]
    public delegate void InventoryReadyEventHandler();
    
    private Dictionary<string, BlockMetadata> _blocks = new();
    private bool _isReady;
    private float _tokenBaseValue = 1.0f;
    
    public bool IsReady => _isReady;
    
    public float TokenBaseValue
    {
        get => _tokenBaseValue;
        set => _tokenBaseValue = value;
    }
    
    public override void _Ready()
    {
        InitializeInventory();
        _isReady = true;
        EmitSignal(SignalName.InventoryReady);
    }
    
    private void InitializeInventory()
    {
        // Add initial blocks to inventory
        Add();  // Add block
        Add();  // Add block
    }
    
    public void Add()
    {
        var metadata = BlockMetadata.CreateAdd();
        AddBlock(metadata.Id, metadata);
    }
    
    private void AddBlock(string id, BlockMetadata metadata)
    {
        if (!_blocks.ContainsKey(id))
        {
            _blocks[id] = metadata;
        }
    }
    
    public Dictionary<string, BlockMetadata> GetAllBlocks()
    {
        return new Dictionary<string, BlockMetadata>(_blocks);
    }
    
    public BlockMetadata? GetBlock(string id)
    {
        return _blocks.GetValueOrDefault(id);
    }
}
