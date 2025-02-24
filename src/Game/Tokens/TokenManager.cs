using F.Audio;
using F.Framework.Blocks;
using F.Framework.Connections;
using F.Framework.Core.SceneTree;
using F.Game.Core;
using GameManager = F.Framework.Core.GameManager;
using F.Framework.Core.Interfaces;
using F.Framework.Core.Services;
using F.Framework.Logging;
using F.Framework.Tokens.Interfaces;

namespace F.Game.Tokens;

public partial class TokenManager : Node, ITokenManager
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
        // Get required components from Services
        _tokenLayer = Services.Instance?.Game?.TokenLayer as Node2D;
        if (_tokenLayer == null)
        {
            Logger.Token.Err("Failed to get TokenLayer");
            _initRetryCount++;
            if (_initRetryCount >= MAX_RETRIES)
            {
                Logger.Token.Err("Failed to get TokenLayer after maximum retries");
                return;
            }

            Logger.Token.Print($"TokenLayer not ready, retry {_initRetryCount}/{MAX_RETRIES} in {RETRY_INTERVAL}s");
            var timer = new Timer
            {
                OneShot = true,
                WaitTime = RETRY_INTERVAL
            };
            AddChild(timer);
            timer.Timeout += () =>
            {
                timer.QueueFree();
                _Ready();
            };
            timer.Start();
            return;
        }

        // Load the token scene
        var tokenScene = GD.Load<PackedScene>("res://scenes/UI/Token.tscn");
        if (tokenScene == null)
        {
            Logger.Token.Err("Failed to load Token.tscn");
            return;
        }

        _factory = new TokenFactory(_tokenLayer, tokenScene);

        // Initialize connection manager
        _initRetryCount = 0;
        InitializeConnectionManager();
    }

    public override void _Process(double delta)
    {
        // Update token positions and check for completion
        foreach (var token in _activeTokens.ToList())
        {
            if (!IsInstanceValid(token))
            {
                _activeTokens.Remove(token);
                continue;
            }

            if (!token.IsMoving && token.CurrentBlock != null) MoveTokenToNextBlock(token, token.CurrentBlock);
        }
    }

    private void InitializeConnectionManager()
    {
        _connectionManager = Services.Instance?.Connections as ConnectionManager;
        if (_connectionManager == null)
        {
            _initRetryCount++;
            if (_initRetryCount >= MAX_RETRIES)
            {
                Logger.Token.Err("Failed to initialize ConnectionManager after maximum retries");
                return;
            }

            Logger.Token.Print($"ConnectionManager not ready, retry {_initRetryCount}/{MAX_RETRIES} in {RETRY_INTERVAL}s");
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

        Logger.Token.Print("Successfully initialized with ConnectionManager and TokenLayer");
    }

    public void SpawnToken(IBlock startBlock)
    {
        Logger.Token.Print("SpawnToken called");
        if (!ValidateComponents()) return;

        var value = (startBlock as BaseBlock)?.GetValue() ?? 1.0f;
        var token = _factory!.CreateToken(startBlock, value);

        if (token == null)
        {
            Logger.Token.Err("Failed to create token");
            return;
        }

        _activeTokens.Add(token);
        Logger.Token.Print($"Token created with value {value}");
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
        Logger.Token.Print($"Next block found: {nextBlock != null}, pipe: {pipe != null}");

        if (nextBlock != null)
        {
            Logger.Token.Print($"Moving token to next block: {nextBlock.Name}");
            token.MoveTo(nextBlock, pipe);
        }
        else
        {
            Logger.Token.Err("No next block found, destroying token");
            token.QueueFree();
            _activeTokens.Remove(token);
        }
    }

    private bool ValidateComponents()
    {
        if (_factory == null)
        {
            Logger.Token.Err("TokenFactory is null");
            return false;
        }

        if (_connectionManager == null)
        {
            Logger.Token.Err("ConnectionManager is null, token spawning aborted");
            return false;
        }

        return true;
    }

    public void StopAllTokens()
    {
        foreach (var token in _activeTokens.ToList())
        {
            token.QueueFree();
            _activeTokens.Remove(token);
        }
    }

    public void ClearAllTokens()
    {
        foreach (var token in _activeTokens)
            if (IsInstanceValid(token))
                token.QueueFree();

        _activeTokens.Clear();
    }
}