using F.Audio;
using F.Game.Connections;
using Input = F.Game.BlockLogic.Input;
using ConnectionManager = F.Game.Connections.ConnectionManager;

namespace F.Game.Tokens;

public partial class TokenManager : Node
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
        GD.Print("[TokenManager Debug] Initialized with ConnectionManager and TokenLayer");
    }

    public void SpawnToken(IBlock startBlock)
    {
        GD.Print("[TokenManager Debug] SpawnToken called");
        var value = (startBlock as Input)?.GetValue() ?? 0f;
        var token = _factory.CreateToken(startBlock, value);

        if (token == null)
        {
            GD.PrintErr("[TokenManager Debug] Failed to create token");
            return;
        }

        _activeTokens.Add(token);
        GD.Print($"[TokenManager Debug] Token created with value {value}");
        AudioManager.Instance?.PlayTokenStart();

        // Get the next block and send the token there
        if (_connectionManager == null)
        {
            GD.PrintErr("[TokenManager Debug] ConnectionManager is null");
            return;
        }
        var (nextBlock, pipe) = _connectionManager.GetNextConnection();
        GD.Print($"[TokenManager Debug] Next block found: {nextBlock != null}");
        if (nextBlock != null)
        {
            GD.Print("[TokenManager Debug] Moving token to next block");
            token.MoveTo(nextBlock);
        }
        else
        {
            GD.Print("[TokenManager Debug] No next block found, destroying token");
            token.QueueFree();
        }
    }

    public override void _Process(double delta)
    {
        // Remove completed tokens
        var removedCount = _activeTokens.RemoveAll(token => !GodotObject.IsInstanceValid(token));
        if (removedCount > 0)
        {
            GD.Print($"[TokenManager Debug] Removed {removedCount} completed tokens");
        }
    }
}