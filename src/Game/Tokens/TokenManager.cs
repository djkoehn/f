using F.Audio;
using F.Game.Core;
using ConnectionManager = F.Game.Connections.ConnectionManager;

namespace F.Game.Tokens;

public partial class TokenManager : Node
{
    private const float RETRY_INTERVAL = 0.1f;
    private const int MAX_RETRIES = 10;

    private readonly List<Token> _activeTokens = new();
    private ConnectionManager? _connectionManager;
    private TokenFactory? _factory;
    private int _initRetryCount;
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

        // Initialize connection manager
        _initRetryCount = 0;
        InitializeConnectionManager();
    }

    private void InitializeConnectionManager()
    {
        var gameManager = GetParent<GameManager>();
        if (gameManager == null)
        {
            GD.PrintErr("[TokenManager Debug] GameManager not found during initialization");
            return;
        }

        _connectionManager = gameManager.ConnectionManager;
        if (_connectionManager == null)
        {
            _initRetryCount++;
            if (_initRetryCount >= MAX_RETRIES)
            {
                GD.PrintErr("[TokenManager Debug] Failed to initialize ConnectionManager after maximum retries");
                return;
            }

            GD.Print(
                $"[TokenManager Debug] ConnectionManager not ready, retry {_initRetryCount}/{MAX_RETRIES} in {RETRY_INTERVAL}s");
            var timer = new Timer
            {
                OneShot = true,
                WaitTime = RETRY_INTERVAL
            };
            AddChild(timer);
            timer.Timeout += () =>
            {
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
        if (!ValidateComponents()) return;

        var value = (startBlock as BaseBlock)?.GetValue() ?? 1.0f;
        var token = _factory!.CreateToken(startBlock, value);

        if (token == null)
        {
            GD.PrintErr("[TokenManager Debug] Failed to create token");
            return;
        }

        _activeTokens.Add(token);
        GD.Print($"[TokenManager Debug] Token created with value {value}");
        AudioManager.Instance?.PlayTokenStart();

        MoveTokenToNextBlock(token, startBlock);
    }

    public void SpawnTokens(IEnumerable<IBlock> startBlocks)
    {
        if (!ValidateComponents()) return;

        foreach (var block in startBlocks) SpawnToken(block);
    }

    private void MoveTokenToNextBlock(Token token, IBlock currentBlock)
    {
        if (_connectionManager == null) return;

        var (nextBlock, pipe) = _connectionManager.GetNextConnection(currentBlock);
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

    private bool ValidateComponents()
    {
        if (_factory == null)
        {
            GD.PrintErr("[TokenManager Debug] TokenFactory is null");
            return false;
        }

        if (_connectionManager == null)
        {
            GD.PrintErr("[TokenManager Debug] ConnectionManager is null, token spawning aborted");
            return false;
        }

        return true;
    }

    public void StopAllTokens()
    {
        foreach (var token in _activeTokens)
            if (IsInstanceValid(token))
                token.StopMovement();
    }

    public void ClearAllTokens()
    {
        foreach (var token in _activeTokens)
            if (IsInstanceValid(token))
                token.QueueFree();

        _activeTokens.Clear();
    }

    public override void _Process(double delta)
    {
        // Remove completed tokens
        var removedCount = _activeTokens.RemoveAll(token => !IsInstanceValid(token));
        if (removedCount > 0) GD.Print($"[TokenManager Debug] Removed {removedCount} completed tokens");
    }
}