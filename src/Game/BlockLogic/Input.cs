using F.Game.Tokens;
using F.Game.Connections;

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
        AddToGroup("Blocks");
        InitializeSocket();
        
        // Defer manager initialization to ensure GameManager is ready
        CallDeferred("InitializeManagers");
        
        SetProcessInput(true); // Enable input processing
        GD.Print("[Input Debug] Input processing enabled");
    }

    private void InitializeManagers()
    {
        // Get the GameManager first
        var gameManager = GetNode<F.Game.Core.GameManager>("/root/Main/GameManager");
        if (gameManager != null)
        {
            _tokenManager = gameManager.TokenManager;
            _connectionManager = gameManager.ConnectionManager;
            GD.Print($"[Input Debug] Initialized with TokenManager: {_tokenManager != null}, ConnectionManager: {_connectionManager != null}");
        }
        else
        {
            GD.PrintErr("Failed to find GameManager!");
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Space)
        {
            GD.Print("[Input Debug] Spacebar pressed");
            var hasConnections = HasConnections();
            GD.Print($"[Input Debug] Has connections: {hasConnections}");
            if (hasConnections)
            {
                GD.Print("[Input Debug] Spawning token");
                SpawnToken();
            }
        }
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
        if (_connectionManager == null)
        {
            GD.Print("[Input Debug] ConnectionManager is null");
            return false;
        }
        
        var outputSocket = GetOutputSocket();
        if (outputSocket == null)
        {
            GD.Print("[Input Debug] Output socket is null");
            return false;
        }

        // Use the output socket's position to check for connections
        var socketPosition = (outputSocket as Node2D)?.GlobalPosition ?? GlobalPosition;
        GD.Print($"[Input Debug] Checking for connections at socket position: {socketPosition}");
        var pipe = _connectionManager.GetPipeAtPosition(socketPosition);
        GD.Print($"[Input Debug] Found pipe: {pipe != null}");
        return pipe != null;
    }

    public Vector2 GetTokenPosition()
    {
        var tokenSocket = GetNode<Node2D>("TokenSocket");
        if (tokenSocket != null)
        {
            GD.Print($"[Input Debug] Using token socket position: {tokenSocket.GlobalPosition}");
            return tokenSocket.GlobalPosition;
        }
        
        GD.Print($"[Input Debug] Token socket not found, using block position: {GlobalPosition}");
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