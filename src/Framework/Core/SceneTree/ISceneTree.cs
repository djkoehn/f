using Godot;
using Chickensoft.GodotNodeInterfaces;
using F.Framework.Blocks;
using F.Game.Toolbar;
using F.Game.BlockLogic;
using F.Framework.Connections;

namespace F.Framework.Core.SceneTree;

public delegate void InventoryReadyEventHandler();

public interface IConnectionManager : INode2D
{
    void ClearAllHighlights();
    ConnectionPipe? GetPipeAtPosition(Vector2 position);
    bool HandleBlockConnection(IBlock block, Vector2 position);
    void DisconnectBlock(IBlock block);
    bool IsBlockConnected(IBlock block);
    List<ConnectionPipe> GetCurrentConnections(IBlock block);
    (IBlock? nextBlock, ConnectionPipe? pipe) GetNextConnection(IBlock currentBlock);
    void ConnectBlocks(IBlock sourceBlock, IBlock targetBlock);
}

public interface IMain : INode2D
{
    IGameManager GameManager { get; }
    ISceneInitializer SceneInitializer { get; }
    IAudioManager AudioManager { get; }
    IHelperFunnel HelperFunnel { get; }
}

public interface IGameManager : INode2D
{
    IBlockLayer BlockLayer { get; }
    ITokenLayer TokenLayer { get; }
    IInventory Inventory { get; }
    F.Game.BlockLogic.IBlockInteractionManager BlockInteractionManager { get; }
    IToolbar Toolbar { get; }
    ITokenManager TokenManager { get; }
    ColorRect Background { get; }
}

public interface IBlockLayer : INode2D
{
    BaseBlock Input { get; }
    BaseBlock Output { get; }
}

public interface IBlockLayerViewport : ISubViewport
{
    IBlockLayerContent BlockLayerContent { get; }
}

public interface IBlockLayerContent : INode2D
{
    BaseBlock Input { get; }
    BaseBlock Output { get; }
}

public interface ITokenLayer : INode2D { }

public interface IInventory : INode
{
    event InventoryReadyEventHandler InventoryReady;
    bool IsReady { get; }
    float TokenBaseValue { get; set; }
    void AddBlockByType(string blockType);
    void AddBlock(BaseBlock block);
    void RemoveBlock(BaseBlock block);
    IEnumerable<BaseBlock> GetAllBlocks();
    Dictionary<string, BlockMetadata> GetBlockMetadata();
    BlockMetadata? GetBlock(string id);
}

public interface IBlockInteractionManager : INode { }

public interface IToolbar : IControl
{
    IToolbarVisuals ToolbarVisuals { get; }
    IToolbarBlockContainer BlockContainer { get; }
    void ReturnBlockToToolbar(BaseBlock block);
}

public interface IToolbarVisuals : IControl { }

public interface IToolbarBlockContainer : IContainer { }

public interface ITokenManager : INode { }

public interface IAudioManager : INode { }

public interface IHelperFunnel : INode { }

public interface ISceneInitializer : INode { }