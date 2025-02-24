using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using F.Framework.Blocks;
using F.Framework.Core.Interfaces;
using F.Framework.Core.SceneTree;
using F.Framework.Logging;
using Godot;
using System.Text.Json;

namespace F.Framework.Core;

[Meta(typeof(IAutoNode))]
public partial class Inventory : Node, IInventory, IProvide<IInventory>
{
    private const string INVENTORY_PATH = "res://src/Config/Blocks/Inventory.json";

    private readonly List<BaseBlock> _baseBlocks = new();
    private readonly Dictionary<string, BlockMetadata> _blocks = new();

    event InventoryReadyEventHandler IInventory.InventoryReady
    {
        add => _inventoryReady += value;
        remove => _inventoryReady -= value;
    }

    public bool IsReady { get; private set; }
    public float TokenBaseValue { get; set; } = 1.0f;

    public override void _Ready()
    {
        this.Provide();
        InitializeInventory();
        IsReady = true;
        _inventoryReady?.Invoke();
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
        if (!_baseBlocks.Contains(block)) _baseBlocks.Add(block);
    }

    public void RemoveBlock(BaseBlock block)
    {
        if (_baseBlocks.Contains(block)) _baseBlocks.Remove(block);
    }

    public IEnumerable<BaseBlock> GetAllBlocks()
    {
        return _baseBlocks;
    }

    public override void _Notification(int what)
    {
        this.Notify(what);
    }

    IInventory IProvide<IInventory>.Value()
    {
        return this;
    }

    private event InventoryReadyEventHandler? _inventoryReady;

    private void InitializeInventory()
    {
        _blocks.Clear();
        var jsonText = FileAccess.GetFileAsString(INVENTORY_PATH);
        var json = new Json();
        var parseResult = json.Parse(jsonText);

        if (parseResult != Error.Ok)
        {
            Logger.Game.Err($"Failed to parse Inventory.json: {json.GetErrorMessage()}");
            return;
        }

        var data = json.Data.AsGodotDictionary();
        if (!data.ContainsKey("blocks"))
        {
            Logger.Game.Err("Inventory.json is missing 'blocks' array");
            return;
        }

        var blocks = data["blocks"].AsStringArray();
        foreach (var blockId in blocks) AddBlockByType(blockId);

        Logger.Game.Print($"[Inventory] Loaded blocks: {string.Join(", ", _blocks.Keys)}");
    }

    private void AddBlockMetadata(string id, BlockMetadata metadata)
    {
        _blocks[id] = metadata;
    }

    public void LoadBlocks()
    {
        var jsonPath = "res://metadata/blocks/";
        var dir = DirAccess.Open(jsonPath);
        if (dir == null)
        {
            Logger.Game.Err($"Failed to open directory: {jsonPath}");
            return;
        }

        dir.ListDirBegin();
        var fileName = dir.GetNext();
        while (!string.IsNullOrEmpty(fileName))
        {
            if (!fileName.EndsWith(".json"))
            {
                fileName = dir.GetNext();
                continue;
            }

            var blockId = fileName.Replace(".json", "");
            var metadata = BlockMetadata.GetMetadata(blockId);
            if (metadata != null)
            {
                _blocks[blockId] = metadata;
                Logger.Game.Print($"Loaded block metadata: {blockId}");
            }
            else
            {
                Logger.Game.Err($"Failed to load block metadata: {blockId}");
            }

            fileName = dir.GetNext();
        }
        dir.ListDirEnd();

        Logger.Game.Print($"Loaded blocks: {string.Join(", ", _blocks.Keys)}");
    }

    public BlockMetadata? GetBlockMetadata(string blockId)
    {
        if (_blocks.TryGetValue(blockId, out var metadata))
        {
            return metadata;
        }

        Logger.Game.Err($"Block metadata not found: {blockId}");
        return null;
    }

    public IEnumerable<BlockMetadata> GetAllBlocksMetadata()
    {
        return _blocks.Values;
    }
}