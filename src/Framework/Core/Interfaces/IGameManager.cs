using Chickensoft.GodotNodeInterfaces;
using F.Game.BlockLogic;
using Godot;
using F.Framework.Blocks;
using F.Framework.Tokens.Interfaces;

namespace F.Framework.Core.Interfaces;

public interface IGameManager : INode2D
{
    IBlockLayer BlockLayer { get; }
    ITokenLayer TokenLayer { get; }
    IInventory Inventory { get; }
    IBlockInteractionManager BlockInteractionManager { get; }
    IToolbar Toolbar { get; }
    ITokenManager TokenManager { get; }
    ColorRect Background { get; }
    void Initialize();
    void HandleBlockPlaced(BaseBlock block);
    void HandleBlockRemoved(BaseBlock block);
    void HandleBlockDragStarted(BaseBlock block);
    void HandleBlockDragEnded(BaseBlock block);
}