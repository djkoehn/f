using F.Game.Tokens;
using Godot;

namespace F.Game.BlockLogic;

public interface IBlock
{
    string Name { get; set; }
    BlockMetadata? Metadata { get; }
    Vector2 GlobalPosition { get; }
    void Initialize(object config);

    bool HasConnections();
    bool HasInputConnection();
    bool HasOutputConnection();
    void SetInputConnected(bool connected);
    void SetOutputConnected(bool connected);
    void ProcessToken(IToken token);
    Node GetParent();

    Vector2 GetTokenPosition();
    Node? GetInputSocket();
    Node? GetOutputSocket();

    void SetDragging(bool dragging);
    void SetPlaced(bool placed);
}