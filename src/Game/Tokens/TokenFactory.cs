namespace F.Game.Tokens;

public sealed class TokenFactory
{
    private readonly Node2D _tokenLayer;
    private readonly PackedScene _tokenScene;

    public TokenFactory(Node2D tokenLayer, PackedScene tokenScene)
    {
        _tokenLayer = tokenLayer;
        _tokenScene = tokenScene;
    }

    public Token? CreateToken(IBlock startBlock, float value)
    {
        if (_tokenScene == null)
        {
            GD.PrintErr("[TokenFactory Debug] Token scene not found!");
            return null;
        }

        GD.Print($"[TokenFactory Debug] Creating token with value {value} for block {startBlock.Name}");
        
        var token = _tokenScene.Instantiate<Token>();
        if (token == null)
        {
            GD.PrintErr("[TokenFactory Debug] Failed to instantiate token!");
            return null;
        }

        // Add to layer first so _Ready() is called
        _tokenLayer.AddChild(token);
        
        // Initialize after adding to tree
        token.Initialize(startBlock, value);
        token.GlobalPosition = startBlock.GetTokenPosition();
        ZIndexConfig.SetZIndex(token, ZIndexConfig.Layers.Token);

        GD.Print($"[TokenFactory Debug] Token created successfully at position {token.GlobalPosition}");
        return token;
    }
}