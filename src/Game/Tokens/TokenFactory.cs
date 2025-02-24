using F.Framework.Blocks;
using F.Framework.Logging;
using Godot;

namespace F.Game.Tokens;

public sealed class TokenFactory
{
    private const int INITIAL_POOL_SIZE = 10;
    private const int MAX_POOL_SIZE = 50;

    private readonly Node2D _tokenLayer;
    private readonly Queue<Token> _tokenPool = new();
    private readonly PackedScene _tokenScene;
    private int _totalTokensCreated;

    public TokenFactory(Node2D tokenLayer, PackedScene tokenScene)
    {
        _tokenLayer = tokenLayer;
        _tokenScene = tokenScene;

        // Pre-populate pool
        for (var i = 0; i < INITIAL_POOL_SIZE; i++) CreatePooledToken();
    }

    public Token? CreateToken(IBlock startBlock, float value)
    {
        if (_tokenScene == null)
        {
            Logger.Token.Err("Failed to load token scene");
            return null;
        }

        Logger.Token.Print($"Creating token with value {value} for block {startBlock.Name}");

        Token token;
        if (_tokenPool.Count > 0)
        {
            // Reuse token from pool
            token = _tokenPool.Dequeue();
            token.Visible = true;
        }
        else if (_totalTokensCreated < MAX_POOL_SIZE)
        {
            // Create new token if under limit
            token = CreatePooledToken();
            if (token == null) return null;
        }
        else
        {
            // Create temporary token if pool is full
            token = _tokenScene.Instantiate<Token>();
            if (token == null)
            {
                Logger.Token.Err("Failed to instantiate token");
                return null;
            }

            _tokenLayer.AddChild(token);
        }

        // Initialize token
        token.Initialize(startBlock, value);
        token.GlobalPosition = startBlock.GetTokenPosition();
        ZIndexConfig.SetZIndex(token, ZIndexConfig.Layers.Token);

        Logger.Token.Print($"Token created successfully at position {token.GlobalPosition}");
        return token;
    }

    private Token? CreatePooledToken()
    {
        var token = _tokenScene.Instantiate<Token>();
        if (token == null)
        {
            Logger.Token.Err("Failed to instantiate pooled token!");
            return null;
        }

        _tokenLayer.AddChild(token);
        token.Visible = false; // Hide initially
        _totalTokensCreated++;
        _tokenPool.Enqueue(token);
        return token;
    }

    public void ReturnToPool(Token token)
    {
        if (_totalTokensCreated >= MAX_POOL_SIZE)
        {
            // If we're at max pool size, just destroy the token
            token.QueueFree();
            return;
        }

        // Reset token state
        token.Visible = false;
        token.GlobalPosition = Vector2.Zero;
        token.StopMovement();

        // Return to pool
        _tokenPool.Enqueue(token);
    }
}