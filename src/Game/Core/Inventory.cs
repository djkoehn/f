namespace F.Game.Core;

public partial class Inventory : Node
{
    [Signal]
    public delegate void InventoryReadyEventHandler();

    // List of initial blocks
    private static readonly string[] InitialBlocks = new[]
    {
        "add", // First Add block
        "add", // Second Add block
        "add", // Third Add block
        "add", // Fourth Add block
        "add", // Fifth Add block
        "add", // Sixth Add block
        "add", // Seventh Add block
        "add" // Eighth Add block
    };

    private readonly List<BaseBlock> _baseBlocks = new();

    private readonly Dictionary<string, BlockMetadata> _blocks = new();

    public bool IsReady { get; private set; }

    public float TokenBaseValue { get; set; } = 1.0f;

    public override void _Ready()
    {
        InitializeInventory();
        IsReady = true;
        EmitSignal(SignalName.InventoryReady);
    }

    private void InitializeInventory()
    {
        foreach (var blockId in InitialBlocks) AddBlockByType(blockId);

        GD.Print($"Blocks in Inventory: {string.Join(", ", _blocks.Keys)}"); // Debug print
    }

    private void AddBlockByType(string blockId)
    {
        // Create the metadata
        var metadata = BlockMetadata.Create(blockId);
        GD.Print($"Creating block metadata for {blockId}: {metadata != null}"); // Debug print
        if (metadata != null)
        {
            // Create a unique ID for each block instance
            var id = blockId + _blocks.Count(kvp => kvp.Key.StartsWith(blockId));
            AddBlockMetadata(id, metadata);
        }
        else
        {
            GD.PrintErr($"Unknown block type: {blockId}");
        }
    }

    private void AddBlockMetadata(string id, BlockMetadata metadata)
    {
        if (!_blocks.ContainsKey(id)) _blocks[id] = metadata;
    }

    public Dictionary<string, BlockMetadata> GetBlockMetadata()
    {
        // Return all blocks, including duplicates
        return _blocks;
    }

    public BlockMetadata? GetBlock(string id)
    {
        return _blocks.GetValueOrDefault(id);
    }

    public void AddBlock(BaseBlock block)
    {
        if (!_baseBlocks.Contains(block))
        {
            _baseBlocks.Add(block);
            AddChild(block);
        }
    }

    public void RemoveBlock(BaseBlock block)
    {
        if (_baseBlocks.Contains(block))
        {
            _baseBlocks.Remove(block);
            RemoveChild(block);
        }
    }

    public IEnumerable<BaseBlock> GetAllBlocks()
    {
        return _baseBlocks;
    }
}