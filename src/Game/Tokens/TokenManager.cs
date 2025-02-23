using F.Audio;
using F.Game.Connections;
using F.Game.Core;
using ConnectionManager = F.Game.Connections.ConnectionManager;

namespace F.Game.Tokens;

public partial class TokenManager : Node
{
    private readonly List<Token> _activeTokens = new();
    private ConnectionManager? _connectionManager;
    private TokenFactory? _factory;
    private Node2D? _tokenLayer;

    public override void _Ready()
    {
        // Get required components
        var gameManager = GetParent<GameManager>();
        if (gameManager == null)
        {
            GD.PrintErr("[TokenManager Debug] Failed to get GameManager");
            return;
        }

        _tokenLayer = gameManager.GetNode<Node2D>("TokenLayer");
        if (_tokenLayer == null)
        {
            GD.PrintErr("[TokenManager Debug] Failed to get TokenLayer");
            return;
        }

        // Load the token scene
        var tokenScene = GD.Load<PackedScene>("res://scenes/Token.tscn");
        if (tokenScene == null)
        {
            GD.PrintErr("[TokenManager Debug] Failed to load Token.tscn");
            return;
        }
        
        _factory = new TokenFactory(_tokenLayer, tokenScene);

        // Defer ConnectionManager setup
        CallDeferred(nameof(InitializeConnectionManager));
    }

    private void InitializeConnectionManager()
    {
        var gameManager = GetParent<GameManager>();
        if (gameManager == null) return;

        _connectionManager = gameManager.ConnectionManager;
        if (_connectionManager == null)
        {
            GD.PrintErr("[TokenManager Debug] ConnectionManager still not ready, retrying in 0.1s");
            var timer = new Timer();
            AddChild(timer);
            timer.OneShot = true;
            timer.WaitTime = 0.1f;
            timer.Timeout += () => {
                timer.QueueFree();
                InitializeConnectionManager();
            };
            timer.Start();
            return;
        }

        GD.Print("[TokenManager Debug] Successfully initialized with ConnectionManager and TokenLayer");
    }

    public void SpawnToken(IBlock startBlock)
    {
        GD.Print("[TokenManager Debug] SpawnToken called");
        if (_factory == null)
        {
            GD.PrintErr("[TokenManager Debug] TokenFactory is null");
            return;
        }

        if (_connectionManager == null)
        {
            GD.PrintErr("[TokenManager Debug] ConnectionManager is null, token spawning aborted");
            return;
        }

        var value = (startBlock as BaseBlock)?.GetValue() ?? 1.0f;
        var token = _factory.CreateToken(startBlock, value);

        if (token == null)
        {
            GD.PrintErr("[TokenManager Debug] Failed to create token");
            return;
        }

        _activeTokens.Add(token);
        GD.Print($"[TokenManager Debug] Token created with value {value}");
        AudioManager.Instance?.PlayTokenStart();

        var (nextBlock, pipe) = _connectionManager.GetNextConnection(startBlock);
        GD.Print($"[TokenManager Debug] Next block found: {nextBlock != null}, pipe: {pipe != null}");
        
        if (nextBlock != null)
        {
            GD.Print($"[TokenManager Debug] Moving token to next block: {nextBlock.Name}");
            token.MoveTo(nextBlock, pipe);
        }
        else
        {
            GD.PrintErr("[TokenManager Debug] No next block found, destroying token");
            token.QueueFree();
            _activeTokens.Remove(token);
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