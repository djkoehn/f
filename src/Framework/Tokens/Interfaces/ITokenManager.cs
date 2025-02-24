using Chickensoft.GodotNodeInterfaces;
using F.Framework.Blocks;

namespace F.Framework.Tokens.Interfaces;

public interface ITokenManager : INode
{
    void SpawnToken(IBlock startBlock);
    void SpawnTokens(IEnumerable<IBlock> startBlocks);
    void StopAllTokens();
    void ClearAllTokens();
}