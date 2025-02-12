using Godot;
using System.Collections.Generic;
using System.Linq;

namespace F;

public partial class Inventory : Node
{
    [Signal]
    public delegate void InventoryReadyEventHandler();
    
    private Dictionary<string, BlockMetadata> _blocks = new();
    private bool _isReady;
    private float _tokenBaseValue = 1.0f;
    
    // List of initial blocks - just add the IDs you want!
    private static readonly string[] InitialBlocks = new[]
    {
        "add",  // First Add block
        "add",
        "add",
        "add",
        "add",
        "add",
        "add",
        "add",
        "add",
    };
    
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
        foreach (var blockId in InitialBlocks)
        {
            AddBlockByType(blockId);
        }
    }
    
    private void AddBlockByType(string blockId)
    {
        // Count how many of this type we already have
        int existingCount = _blocks.Keys.Count(k => k.StartsWith(blockId));
        
        // Create the metadata
        var metadata = BlockMetadata.Create(blockId);
        if (metadata != null)
        {
            string id = existingCount == 0 ? blockId : $"{blockId}{existingCount + 1}";
            AddBlock(id, metadata);
        }
        else
        {
            GD.PrintErr($"Unknown block type: {blockId}");
        }
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
