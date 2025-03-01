using F.Game.Connections;
using F.Game.Core;
using F.Game.Tokens;

// for IBlock, if needed

namespace F.Game.BlockLogic;
// New enum to represent the state of a block

// Marking the class as partial and implementing IBlock
public partial class BaseBlock : Node2D, IBlock
{
    private string _blockName = "";
    private ConnectionManager? _connectionManager;
    private Node2D? _inputSocket;
    private bool _isConnected;
    private bool _isInputConnected;
    private bool _isOutputConnected;
    private BlockMetadata? _metadata;
    private Node2D? _outputSocket;
    private TokenRepository? _tokenRepository;
    protected float _value = 1.0f;
    private Label? _valueLabel;

    public BlockState State { get; set; } = BlockState.InToolbar;

    string IBlock.Name
    {
        get => Name;
        set
        {
            _blockName = value;
            Name = value;
        }
    }

    public BlockMetadata? Metadata
    {
        get => _metadata;
        set
        {
            _metadata = value;
            GD.Print(
                $"[BaseBlock Debug] Setting metadata for block {Name} - Value: {(value != null ? $"Id={value.Id}, SpawnOnSpace={value.SpawnOnSpace}" : "null")}");
        }
    }

    public void Initialize(object config)
    {
        if (config is BlockConfig blockConfig)
        {
            _blockName = blockConfig.Name;
            if (blockConfig.DefaultValue.HasValue)
                _value = blockConfig.DefaultValue.Value;
        }
        else
        {
            GD.PrintErr("Invalid config passed to BaseBlock.Initialize");
        }
    }

    public virtual void ProcessToken(IToken token)
    {
        if (token == null) return;

        GD.Print($"[BaseBlock Debug] Processing token in block {Name}, current value: {token.Value}");

        // Execute the block's token processing script
        if (!string.IsNullOrEmpty(Metadata?.ProcessTokenScript))
            try
            {
                var Value = token.Value; // Create local Value variable for the script

                // Execute the script by evaluating the expression
                var script = Metadata.ProcessTokenScript;
                GD.Print($"[BaseBlock Debug] Executing script: {script}");

                // Parse and execute the script manually since we can't use Roslyn
                if (script.Contains("Value = "))
                {
                    // Extract the value assignment
                    var valueStr = script.Split('=')[1].Trim().TrimEnd('f', ';');
                    if (float.TryParse(valueStr, out var newValue)) Value = newValue;
                }
                else if (script.Contains("Value += "))
                {
                    var valueStr = script.Split('+')[1].Trim('=', ' ').TrimEnd('f', ';');
                    if (float.TryParse(valueStr, out var addValue)) Value += addValue;
                }
                else if (script.Contains("Value *= "))
                {
                    var valueStr = script.Split('*')[1].Trim('=', ' ').TrimEnd('f', ';');
                    if (float.TryParse(valueStr, out var mulValue)) Value *= mulValue;
                }

                // Update the token value
                token.UpdateValue(Value);
                _value = Value; // Update block's internal value

                GD.Print($"[BaseBlock Debug] Block {Name} processed token. New value: {Value}");

                // Update the value label if it should be displayed
                if (Metadata?.DisplayValue == true && _valueLabel != null)
                {
                    _valueLabel.Visible = true;
                    _valueLabel.Text = Value.ToString("F1");
                    GD.Print($"[BaseBlock Debug] Updated value label to: {Value}");
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error processing token in block {Name}: {e.Message}");
            }

        // Add this block to the token's processed blocks if not already there
        if (!token.ProcessedBlocks.Contains(this))
        {
            token.ProcessedBlocks.Add(this);
        }

        if (_connectionManager == null) return;
        var (nextBlock, pipe) = _connectionManager.GetNextConnection(this);
        if (nextBlock != null)
        {
            GD.Print($"[BaseBlock Debug] Moving token to next block: {nextBlock.Name}");
            token.MoveTo(nextBlock);
        }
    }

    public Node? GetInputSocket()
    {
        return Metadata?.HasInputSocket == true ? _inputSocket : null;
    }

    public Node? GetOutputSocket()
    {
        return Metadata?.HasOutputSocket == true ? _outputSocket : null;
    }

    public bool HasConnections()
    {
        return HasInputConnection() || HasOutputConnection();
    }

    public bool HasInputConnection()
    {
        return _isInputConnected;
    }

    public bool HasOutputConnection()
    {
        return _isOutputConnected;
    }

    public void SetInputConnected(bool connected)
    {
        _isInputConnected = connected;
    }

    public void SetOutputConnected(bool connected)
    {
        _isOutputConnected = connected;
    }

    public Vector2 GetTokenPosition()
    {
        var tokenSocket = GetNodeOrNull<Node2D>("TokenSocket");
        return tokenSocket?.GlobalPosition ?? GlobalPosition;
    }

    public void SetDragging(bool dragging)
    {
        if (Metadata?.IsStationary == true) return;

        var oldState = State;
        if (dragging)
            State = BlockState.Dragging;
        else if (State == BlockState.Dragging)
            State = BlockState.Placed;
        GD.Print($"[BaseBlock Debug] Block {Name} state changed: {oldState} -> {State} (SetDragging: {dragging})");
    }

    public void SetPlaced(bool placed)
    {
        if (Metadata?.IsStationary == true) return;

        var oldState = State;
        if (placed)
            State = BlockState.Placed;
        else if (State == BlockState.Placed)
            State = BlockState.Dragging;
        GD.Print($"[BaseBlock Debug] Block {Name} state changed: {oldState} -> {State} (SetPlaced: {placed})");
    }

    public virtual float GetValue()
    {
        return _value;
    }

    public virtual void SetValue(float value)
    {
        _value = value;
    }

    public override void _Ready()
    {
        base._Ready();
        if (!string.IsNullOrEmpty(_blockName)) Name = _blockName;
        GD.Print($"[Debug BaseBlock] {Name} _Ready called, adding to Blocks group");
        GD.Print(
            $"[Debug BaseBlock] {Name} metadata: {(Metadata != null ? $"ID={Metadata.Id}, SpawnOnSpace={Metadata.SpawnOnSpace}" : "null")}");
        AddToGroup("Blocks");
        InitializeDragging();
        InitializeSockets();

        // Defer manager initialization to ensure GameManager is ready
        CallDeferred(nameof(InitializeManagers));

        SetProcess(true);
        GD.Print($"[Debug BaseBlock] {Name} initialization started, Position: {GlobalPosition}, State: {State}");
    }

    private void InitializeSockets()
    {
        _inputSocket = GetNodeOrNull<Node2D>("BlockInputSocket");
        _outputSocket = GetNodeOrNull<Node2D>("BlockOutputSocket");
        _valueLabel = GetNodeOrNull<Label>("Value");

        // Configure sockets based on metadata
        if (Metadata != null)
        {
            if (_inputSocket != null) _inputSocket.Visible = Metadata.HasInputSocket;
            if (_outputSocket != null) _outputSocket.Visible = Metadata.HasOutputSocket;
            if (_valueLabel != null)
            {
                _valueLabel.Visible = Metadata.DisplayValue;
                _valueLabel.Text = _value.ToString("F1");
                GD.Print($"[BaseBlock Debug] Block {Name} initialized value label with: {_value}");
            }
        }
    }

    private void InitializeManagers()
    {
        GD.Print($"[Debug BaseBlock] {Name} attempting to initialize managers");

        // Get the GameManager using absolute path
        var gameManager = GetNode<GameManager>(SceneNodeConfig.Main.GameManager);
        if (gameManager == null)
        {
            GD.PrintErr($"[Debug BaseBlock] {Name} failed to get GameManager reference");
            // Retry after a short delay
            CreateTimer(0.1f, nameof(InitializeManagers));
            return;
        }

        // Get ConnectionManager and TokenManager from GameManager properties
        _connectionManager = gameManager.ConnectionManager;
        _tokenRepository = gameManager.TokenRepository;

        GD.Print($"[Debug BaseBlock] {Name} managers initialized:");
        GD.Print($"  - ConnectionManager: {(_connectionManager != null ? "found" : "null")}");
        GD.Print($"  - TokenRepository: {(_tokenRepository != null ? "found" : "null")}");

        if (_tokenRepository == null)
        {
            GD.PrintErr($"[Debug BaseBlock] {Name} failed to get TokenRepository from GameManager, will retry");
            // Retry after a short delay
            CreateTimer(0.1f, nameof(InitializeManagers));
            return;
        }

        GD.Print($"[Debug BaseBlock] {Name} initialization complete");
    }

    private void CreateTimer(float delay, string method)
    {
        var timer = new Timer();
        AddChild(timer);
        timer.OneShot = true;
        timer.WaitTime = delay;
        timer.Timeout += () =>
        {
            CallDeferred(method);
            timer.QueueFree();
        };
        timer.Start();
    }

    public override void _Input(InputEvent @event)
    {
        // Only process input events for the Input block
        if (Name != "Input") return;

        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            GD.Print($"[BaseBlock Debug] Key event received for Input block. Key: {keyEvent.Keycode}");

            if (keyEvent.Keycode == Key.Space)
            {
                GD.Print("[BaseBlock Debug] Spacebar pressed for Input block, attempting to spawn token");
                SpawnToken();
            }
        }
    }

    public void SpawnToken()
    {
        GD.Print($"[BaseBlock Debug] SpawnToken called for block {Name}");
        GD.Print($"[BaseBlock Debug] TokenRepository exists: {_tokenRepository != null}");
        GD.Print($"[BaseBlock Debug] Has output connection: {HasOutputConnection()}");
        GD.Print($"[BaseBlock Debug] Output socket connected: {_isOutputConnected}");
        GD.Print(
            $"[BaseBlock Debug] Metadata: {(Metadata != null ? $"Id={Metadata.Id}, SpawnOnSpace={Metadata.SpawnOnSpace}" : "null")}");
        GD.Print($"[BaseBlock Debug] Block state: {State}");

        if (_tokenRepository == null)
        {
            GD.PrintErr("[BaseBlock Debug] Failed to spawn token - TokenRepository is null");
            return;
        }

        if (!HasOutputConnection())
        {
            GD.PrintErr("[BaseBlock Debug] Failed to spawn token - Block has no output connection");
            return;
        }

        GD.Print($"[BaseBlock Debug] Spawning token from block {Name}");
        _tokenRepository.SpawnToken(this);
    }

    public virtual void ResetConnections()
    {
        GD.Print($"[BaseBlock Debug] Block {Name} resetting connections. Current state: {State}");

        if (_inputSocket != null)
        {
            _isInputConnected = false;
            GD.Print($"[BaseBlock Debug] Block {Name} reset input socket");
        }

        if (_outputSocket != null)
        {
            _isOutputConnected = false;
            GD.Print($"[BaseBlock Debug] Block {Name} reset output socket");
        }

        GD.Print($"[BaseBlock Debug] Block {Name} finished resetting connections");
    }

    public void CompleteConnection()
    {
        var oldState = State;
        State = BlockState.Connected;
        GD.Print($"[BaseBlock Debug] Block {Name} state changed: {oldState} -> {State} (CompleteConnection)");
    }

    public void SetInToolbar(bool value)
    {
        if (Metadata?.IsStationary == true) return;

        var oldState = State;
        if (value)
            State = BlockState.InToolbar;
        else if (State == BlockState.InToolbar)
            State = BlockState.Placed;
        GD.Print($"[BaseBlock Debug] Block {Name} state changed: {oldState} -> {State} (SetInToolbar: {value})");
    }

    // Declare the partial method for initializing dragging functionality; implementation provided in BaseBlock.Dragging.cs
    partial void InitializeDragging();
}