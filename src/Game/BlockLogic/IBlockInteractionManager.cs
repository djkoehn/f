using F.Framework.Blocks;
using F.Framework.Core.Interfaces;

namespace F.Game.BlockLogic;

/// <summary>
/// Game-specific block interaction interface that extends the Framework's core interface
/// </summary>
public interface IBlockInteractionManager : IBlockInteractionManagerBase
{
    /// <summary>
    /// Gets a block at the specified position within a threshold
    /// </summary>
    BaseBlock? GetBlockAtPosition(Vector2 position);
}