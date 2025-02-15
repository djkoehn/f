using F.Audio;
using F.Game.BlockLogic;
using F.Game.Connections;
using Input = F.Game.BlockLogic.Input;
using ConnectionManager = F.Game.Connections.ConnectionManager;

namespace F.Game.Tokens;

public class TokenManager
{
    private readonly List<Token> _activeTokens = new();
    private readonly ConnectionManager _connectionManager;
    private readonly TokenFactory _factory;
    private readonly Node2D _tokenLayer;

    public TokenManager(ConnectionManager connectionManager, Node2D tokenLayer)
    {
        _connectionManager = connectionManager;
        _tokenLayer = tokenLayer;
        var tokenScene = GD.Load<PackedScene>("res://scenes/Token.tscn");
        _factory = new TokenFactory(_tokenLayer, tokenScene);
    }

    public void SpawnToken(IBlock startBlock)
    {
        var value = (startBlock as Input)?.GetValue() ?? 0f;
        var token = _factory.CreateToken(startBlock, value);

        if (token == null) return;

        _activeTokens.Add(token);
        AudioManager.Instance?.PlayTokenStart();

        // Get the next block and send the token there
        if (_connectionManager == null) return;
        var (nextBlock, pipe) = _connectionManager.GetNextConnection();
        if (nextBlock != null) token.MoveTo(nextBlock);
    }

    public void Update()
    {
        // Remove completed tokens
        _activeTokens.RemoveAll(token => !GodotObject.IsInstanceValid(token));
    }
}