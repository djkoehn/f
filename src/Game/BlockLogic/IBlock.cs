using F.Game.Tokens;

namespace F.Game.BlockLogic
{
    public interface IBlock
    {
        string Name { get; set; }
        void Initialize(object config);

        bool HasConnections();
        void ProcessToken(Token token);
        Vector2 GlobalPosition { get; }
        Node GetParent();

        Vector2 GetTokenPosition();
        Node? GetInputSocket();
        Node? GetOutputSocket();

        void SetDragging(bool dragging);
        void SetPlaced(bool placed);
    }
} 