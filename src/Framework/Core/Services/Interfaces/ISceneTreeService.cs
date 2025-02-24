using Chickensoft.GodotNodeInterfaces;

namespace F.Framework.Core.Services.Interfaces;

public interface ISceneTreeService : INode
{
    string GetPath(string nodePath);
}