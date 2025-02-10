using Godot;
using System.Collections.Generic;

namespace F;

public partial class Inventory : Node
{
    [Signal]
    public delegate void InventoryReadyEventHandler();
    
    private Dictionary<string, BlockMetadata> _blocks = new();
    private bool _isReady;
    
    public bool IsReady => _isReady;
    
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
