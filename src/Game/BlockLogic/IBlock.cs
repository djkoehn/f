using F.Game.Tokens;
using F.Framework.Core;

namespace F.Game.BlockLogic
{
    public interface IBlock
    {
        string Name { get; set; }
        void Initialize(object config);
        BlockMetadata? Metadata { get; }

        bool HasConnections();
        bool HasInputConnection();
        bool HasOutputConnection();
        void SetInputConnected(bool connected);
        void SetOutputConnected(bool connected);
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