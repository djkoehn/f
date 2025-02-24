using Chickensoft.GodotNodeInterfaces;
using F.Framework.Blocks;

namespace F.Framework.Core.Interfaces;

public interface IBlockLayer : INode2D
{
    BaseBlock Input { get; }
    BaseBlock Output { get; }
}