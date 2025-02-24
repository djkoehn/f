using Godot;
using F.Framework.Blocks;
using F.Framework.Core.SceneTree;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;

namespace F.Framework.Core;

[Meta(typeof(IAutoNode))]
public partial class Inventory : Node, IInventory, IProvide<IInventory>
{
    private event InventoryReadyEventHandler? _inventoryReady;
    event InventoryReadyEventHandler IInventory.InventoryReady
    {
        add => _inventoryReady += value;
        remove => _inventoryReady -= value;
    }

    private readonly List<BaseBlock> _baseBlocks = new();
    private readonly Dictionary<string, BlockMetadata> _blocks = new();

    public bool IsReady { get; private set; }
    public float TokenBaseValue { get; set; } = 1.0f;

    private const string INVENTORY_PATH = "res://Inventory.json";

    public override void _Ready()
    {
        this.Provide();
        InitializeInventory();
        IsReady = true;
        _inventoryReady?.Invoke();
    }

    private void InitializeInventory()
    {
        _blocks.Clear();
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

        GD.Print($"[Inventory] Loaded blocks: {string.Join(", ", _blocks.Keys)}");
    }

    public void AddBlockByType(string blockType)
    {
        var metadata = BlockMetadata.GetMetadata(blockType);
        if (metadata != null)
        {
            // Create a unique ID for each block instance
            var id = blockType + _blocks.Count;
            AddBlockMetadata(id, metadata);
        }
    }

    private void AddBlockMetadata(string id, BlockMetadata metadata)
    {
        _blocks[id] = metadata;
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

    public override void _Notification(int what) => this.Notify(what);

    IInventory IProvide<IInventory>.Value() => this;
}