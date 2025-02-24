using Chickensoft.GodotNodeInterfaces;
using F.Framework.Blocks;

namespace F.Framework.Connections.Interfaces;

public interface IConnectionManager : INode2D
{
    void ClearAllHighlights();
    ConnectionPipe? GetPipeAtPosition(Vector2 position);
    bool HandleBlockConnection(IBlock block, Vector2 position);
    void DisconnectBlock(IBlock block);
    bool IsBlockConnected(IBlock block);
    List<ConnectionPipe> GetCurrentConnections(IBlock block);
    (IBlock? nextBlock, ConnectionPipe? pipe) GetNextConnection(IBlock currentBlock);
    void ConnectBlocks(IBlock sourceBlock, IBlock targetBlock);
}