using Chickensoft.GodotNodeInterfaces;

namespace F.Framework.Core.Interfaces;

public interface IMain : INode2D
{
    IGameManager GameManager { get; }
    ISceneInitializer SceneInitializer { get; }
    IAudioManager AudioManager { get; }
    IHelperFunnel HelperFunnel { get; }
}