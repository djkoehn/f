using Chickensoft.GodotNodeInterfaces;
using F.Framework.Core;
using F.Game.Tokens;

namespace F.Framework.Blocks.Interfaces;

public interface IBlock : INode
{
    new string Name { get; set; }
    BlockMetadata? Metadata { get; }
    Vector2 GlobalPosition { get; }
    void Initialize(object config);

    bool HasConnections();
    bool HasInputConnection();
    bool HasOutputConnection();
    void SetInputConnected(bool connected);
    void SetOutputConnected(bool connected);
    void ProcessToken(Token token);
    new Node GetParent();

    Vector2 GetTokenPosition();
    Node? GetInputSocket();
    Node? GetOutputSocket();

    void SetDragging(bool dragging);
    void SetPlaced(bool placed);
}