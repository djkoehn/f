namespace F.Game.Core;

public partial class Inventory : Node
{
    [Signal]
    public delegate void InventoryReadyEventHandler();

    private readonly List<BaseBlock> _baseBlocks = new();
    private readonly Dictionary<string, F.Game.BlockLogic.BlockMetadata> _blocks = new();

    public bool IsReady { get; private set; }
    public float TokenBaseValue { get; set; } = 1.0f;

    private const string INVENTORY_PATH = "res://Inventory.json";

    public override void _Ready()
    {
        InitializeInventory();
        IsReady = true;
        EmitSignal(SignalName.InventoryReady);
    }

    private void InitializeInventory()
    {
        var jsonText = FileAccess.GetFileAsString(INVENTORY_PATH);
        var json = new Json();
        var parseResult = json.Parse(jsonText);
        
        if (parseResult != Error.Ok)
        {
            GD.PrintErr($"Failed to parse Inventory.json: {json.GetErrorMessage()}");
            return;
        }

        var data = json.Data.AsGodotDictionary();
        if (!data.ContainsKey("blocks"))
        {
            GD.PrintErr("Inventory.json is missing 'blocks' array");
            return;
        }

        var blocks = data["blocks"].AsStringArray();
        foreach (var blockId in blocks)
        {
            AddBlockByType(blockId);
        }

        GD.Print($"Blocks in Inventory: {string.Join(", ", _blocks.Keys)}"); // Debug print
    }

    public void AddBlockByType(string blockType)
    {
        BlockMetadata? metadata = BlockMetadata.GetMetadata(blockType);
        if (metadata != null)
        {
            // Create a unique ID for each block instance
            var id = blockType + _blocks.Count;
            AddBlockMetadata(id, metadata);
        }
    }

    private void AddBlockMetadata(string id, F.Game.BlockLogic.BlockMetadata metadata)
    {
        if (!_blocks.ContainsKey(id)) _blocks[id] = metadata;
    }

    public Dictionary<string, F.Game.BlockLogic.BlockMetadata> GetBlockMetadata()
    {
        // Return all blocks, including duplicates
        return _blocks;
    }

    public F.Game.BlockLogic.BlockMetadata? GetBlock(string id)
    {
        return _blocks.GetValueOrDefault(id);
    }

    public void AddBlock(BaseBlock block)
    {
        if (!_baseBlocks.Contains(block))
        {
            _baseBlocks.Add(block);
        }
    }

    public void RemoveBlock(BaseBlock block)
    {
        if (_baseBlocks.Contains(block))
        {
            _baseBlocks.Remove(block);
        }
    }

    public IEnumerable<BaseBlock> GetAllBlocks()
    {
        return _baseBlocks;
    }
}
 