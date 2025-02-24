using Godot;
using F.Framework.Blocks;
using F.Framework.Core.SceneTree;

namespace F.Game.BlockLogic;

public interface IBlockInteractionManager : F.Framework.Core.SceneTree.IBlockInteractionManager
{
    BaseBlock? GetBlockAtPosition(Vector2 position);
}