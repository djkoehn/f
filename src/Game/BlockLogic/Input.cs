using Godot;
using F.Game.Tokens;
using F.Game.BlockLogic;
using F.Game.Connections;
using F.Config;

namespace F.Game.BlockLogic;

public partial class Input : Node2D, IBlock
{
    private Node? _outputSocket;
    protected ConnectionManager? _connectionManager;
    private float _value = 1.0f;
    private TokenManager? _tokenManager;

    public new string Name { get; set; } = "Input";

    public override void _Ready()
    {
        base._Ready();
        InitializeSocket();
        _tokenManager = GetNode<TokenManager>("/root/Main/GameManager/TokenManager");
    }

    private void InitializeSocket()
    {
        _outputSocket = GetNodeOrNull<Node>("BlockOutputSocket/Socket");
        if (_outputSocket != null)
        {
            var outputArea = new Area2D { Name = "OutputArea" };
            var outputShape = new CollisionShape2D();
            var outputCircle = new CircleShape2D { Radius = 10.0f };
            outputShape.Shape = outputCircle;
            outputArea.AddChild(outputShape);
            _outputSocket.AddChild(outputArea);
        }
        else
        {
            GD.PrintErr($"Output socket not found for Input block");
        }
    }

    public Node? GetInputSocket()
    {
        return null; // Input block has no input socket
    }

    public Node? GetOutputSocket()
    {
        return _outputSocket;
    }

    public bool HasConnections()
    {
        if (_connectionManager == null) return false;
        var pipe = _connectionManager.GetPipeAtPosition(GlobalPosition);
        return pipe != null;
    }

    public Vector2 GetTokenPosition()
    {
        return GlobalPosition;
    }

    public void Initialize(BlockConfig config)
    {
        if (config.DefaultValue.HasValue) _value = config.DefaultValue.Value;
    }

    public float GetValue()
    {
        return _value;
    }

    public void SetValue(float value)
    {
        _value = value;
    }

    public void ProcessToken(Token token)
    {
        if (token == null) return;

        token.Value = _value;
        token.ProcessedBlocks.Add(this);

        if (_connectionManager == null) return;
        var (nextBlock, pipe) = _connectionManager.GetNextConnection();
        if (nextBlock != null)
            token.MoveTo(nextBlock);
        else
            token.QueueFree();
    }

    public void SpawnToken()
    {
        _tokenManager?.SpawnToken(this);
    }

    void IBlock.Initialize(object config)
    {
        if (config is BlockConfig blockConfig)
            Initialize(blockConfig);
        else
            GD.PrintErr("Invalid config passed to Input.Initialize");
    }

    // Implement IBlock methods for dragging. Since Input is stationary, these are no-ops.
    public void SetDragging(bool dragging)
    {
        // No-op: Input block is stationary
    }

    public void SetPlaced(bool placed)
    {
        // No-op: Input block is stationary
    }
}