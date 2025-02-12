using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace F;

public partial class TokenManager : Node
{
    private ConnectionLayer? _connectionLayer;
    private Node2D? _tokenLayer;
    private List<Token> _activeTokens = new();

    public TokenManager(ConnectionLayer? connectionLayer, Node2D? tokenLayer)
    {
        _connectionLayer = connectionLayer;
        _tokenLayer = tokenLayer;
    }

    public void SpawnToken(BaseBlock startBlock)
    {
        if (_connectionLayer == null || _tokenLayer == null) return;

        var tokenScene = GD.Load<PackedScene>("res://scenes/Token.tscn");
        if (tokenScene == null)
        {
            GD.PrintErr("Failed to load Token scene!");
            return;
        }

        var token = tokenScene.Instantiate<Token>();
        token.GlobalPosition = startBlock.GlobalPosition;
        token.Value = (startBlock as Blocks.Input)?.GetValue() ?? 0f;
        
        _tokenLayer.AddChild(token);
        _activeTokens.Add(token);

        token.MoveTo(startBlock);
    }

    public override void _Process(double delta)
    {
        _activeTokens.RemoveAll(t => t.IsQueuedForDeletion());
    }
} 