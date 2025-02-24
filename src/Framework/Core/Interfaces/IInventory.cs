using Chickensoft.GodotNodeInterfaces;
using F.Framework.Blocks;

namespace F.Framework.Core.Interfaces;

public delegate void InventoryReadyEventHandler();

public interface IInventory : INode
{
    bool IsReady { get; }
    float TokenBaseValue { get; set; }
    event InventoryReadyEventHandler InventoryReady;
    void AddBlockByType(string blockType);
    void AddBlock(BaseBlock block);
    void RemoveBlock(BaseBlock block);
    IEnumerable<BaseBlock> GetAllBlocks();
    Dictionary<string, BlockMetadata> GetBlockMetadata();
    BlockMetadata? GetBlock(string id);
}