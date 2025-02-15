using Godot;
using F.Game.BlockLogic;

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
            GD.PrintErr("Token scene not found!");
            return null;
        }

        var token = _tokenScene.Instantiate<Token>();
        if (token == null)
        {
            GD.PrintErr("Failed to instantiate token!");
            return null;
        }

        token.Initialize(startBlock, value);
        token.GlobalPosition = startBlock.GetTokenPosition();

        _tokenLayer.AddChild(token);
        return token;
    }
}