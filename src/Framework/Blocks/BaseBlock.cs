using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using F.Framework.Core;
using F.Framework.Input;
using F.Game.Tokens;
using F.Framework.Logging;

namespace F.Framework.Blocks;

[Meta(typeof(IAutoNode))]
public partial class BaseBlock : Node2D, IBlock, IProvide<Node>
{
    private string _blockName = "";
    private BlockState _currentState = BlockState.InToolbar;
    private Area2D? _inputArea;
    private Node2D? _inputSocket;
    private bool _isInputConnected;
    private bool _isOutputConnected;
    private BlockLogicMachine? _logicMachine;
    private BlockMetadata? _metadata;
    private Node2D? _outputSocket;
    protected float _value = 1.0f;
    private Label? _valueLabel;
    private bool _isInitialized;

    public BlockState State => _logicMachine?.Value switch
    {
        _ when _logicMachine?.Value is State.InToolbarState => BlockState.InToolbar,
        _ when _logicMachine?.Value is State.DraggingState => BlockState.Dragging,
        _ when _logicMachine?.Value is State.PlacedState => BlockState.Placed,
        _ when _logicMachine?.Value is State.ConnectedState => BlockState.Connected,
        _ when _logicMachine?.Value is State.ConnectedAndDraggingState => BlockState.ConnectedAndDragging,
        _ => BlockState.InToolbar
    };

    string IBlock.Name
    {
        get => Name;
        set => Name = value;
    }

    public BlockMetadata? Metadata => _metadata;

    public virtual void Initialize(object metadata)
    {
        if (metadata is BlockMetadata blockMetadata)
        {
            Initialize(blockMetadata);
        }
        else
        {
            Logger.Block.Err($"Invalid metadata type for block {Name}");
        }
    }

    public void Initialize(BlockMetadata metadata)
    {
        if (_isInitialized)
        {
            Logger.Block.Print($"Block {Name} already initialized");
            return;
        }

        _metadata = metadata;
        _isInitialized = true;

        if (_metadata.HasInput)
        {
            var inputArea = GetNode<Area2D>("InputArea");
            if (inputArea == null)
            {
                Logger.Block.Err($"Block {Name} is missing InputArea node");
                return;
            }
            inputArea.Visible = true;
        }

        if (_metadata.HasOutput)
        {
            var outputArea = GetNode<Area2D>("OutputArea");
            if (outputArea == null)
            {
                Logger.Block.Err($"Block {Name} is missing OutputArea node");
                return;
            }
            outputArea.Visible = true;
        }

        if (_metadata.IsToolbarBlock)
        {
            SetInToolbar(true);
        }
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

    public virtual void ProcessToken(Token token)
    {
        if (token == null || Metadata == null) return;

        Logger.Block.Print($"Processing token in block {Name}, current value: {token.Value}");

        if (!string.IsNullOrEmpty(Metadata.ProcessTokenScript))
            try
            {
                var Value = token.Value;
                var script = Metadata.ProcessTokenScript;

                if (script.Contains("Value = "))
                {
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

                token.Value = Value;
                _value = Value;

                if (Metadata.DisplayValue && _valueLabel != null)
                {
                    _valueLabel.Visible = true;
                    _valueLabel.Text = Value.ToString("F1");
                }
            }
            catch (Exception e)
            {
                Logger.Block.Err($"Error processing token in block {Name}: {e.Message}");
            }
    }

    public Node? GetInputSocket()
    {
        return _inputSocket;
    }

    public Node? GetOutputSocket()
    {
        return _outputSocket;
    }

    public Vector2 GetTokenPosition()
    {
        return GlobalPosition;
    }

    public void SetDragging(bool dragging)
    {
        if (_logicMachine == null) return;

        if (dragging && State != BlockState.Dragging)
        {
            _logicMachine.Send(new Input.StartDragging());
        }
        else if (!dragging && State == BlockState.Dragging)
        {
            _logicMachine.Send(new Input.StopDragging());
        }
    }

    public void SetPlaced(bool placed)
    {
        if (_logicMachine == null) return;

        if (placed && State != BlockState.Placed)
        {
            _logicMachine.Send(new Input.Place());
        }
        else if (!placed && State == BlockState.Placed)
        {
            _logicMachine.Send(new Input.Unplace());
        }
    }

    Node IProvide<Node>.Value()
    {
        return this;
    }

    public override void _Ready()
    {
        base._Ready();
        _logicMachine = GetNode<BlockLogicMachine>("LogicMachine");
        if (_logicMachine == null)
        {
            Logger.Block.Err($"Block {Name} is missing LogicMachine node");
            return;
        }

        // Add to the Blocks group for interaction detection
        AddToGroup("Blocks");
        Logger.Block.Print($"Added block {Name} to Blocks group");

        // Ensure input events are handled
        ProcessMode = ProcessModeEnum.Always;
        SetProcess(true);
        SetProcessInput(true);
        SetProcessUnhandledInput(true);
        SetProcessUnhandledKeyInput(true);

        // Create an Area2D for input detection if it doesn't exist
        _inputArea = GetNodeOrNull<Area2D>("InputArea");
        if (_inputArea == null)
        {
            _inputArea = new Area2D { Name = "InputArea" };
            var shape = new CollisionShape2D { Name = "Shape" };
            var rect = new RectangleShape2D();
            rect.Size = new Vector2(100, 100); // Adjust size as needed
            shape.Shape = rect;
            _inputArea.AddChild(shape);
            AddChild(_inputArea);
            _inputArea.InputPickable = true;
            _inputArea.InputEvent += OnAreaInputEvent;
            Logger.Block.Print($"Created input area for block {Name}");
        }

        this.Provide();
        InitializeSockets();

        Logger.Block.Print($"Block {Name} ready, LogicMachine: {_logicMachine != null}");
    }

    private void InitializeSockets()
    {
        _inputSocket = GetNodeOrNull<Node2D>("BlockInputSocket");
        _outputSocket = GetNodeOrNull<Node2D>("BlockOutputSocket");
        _valueLabel = GetNodeOrNull<Label>("Value");

        if (Metadata != null)
        {
            if (_inputSocket != null) _inputSocket.Visible = Metadata.HasInputSocket;
            if (_outputSocket != null) _outputSocket.Visible = Metadata.HasOutputSocket;
            if (_valueLabel != null)
            {
                _valueLabel.Visible = Metadata.DisplayValue;
                _valueLabel.Text = _value.ToString("F1");
            }
        }
    }

    private void OnAreaInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (_logicMachine == null) return;

        if (@event.IsActionPressed(InputActions.Interact))
        {
            Logger.Block.Print($"Block {Name} received interact action");
            _logicMachine.Send(new Input.Interact());
        }
        else if (@event.IsActionPressed(InputActions.ReturnBlock))
        {
            Logger.Block.Print($"Block {Name} received return action");
            _logicMachine.Send(new Input.ReturnBlock());
        }
    }

    public BlockLogicMachine? GetLogicMachine() => _logicMachine;

    public void NotifyHoveredOverPipe()
    {
        if (_logicMachine == null) return;
        _logicMachine.Send(new Input.HoveredOverPipe());
    }

    public override void _ExitTree()
    {
        if (_logicMachine != null)
        {
            _logicMachine.StateChanged -= HandleStateChanged;
            if (_logicMachine.IsInsideTree())
            {
                RemoveChild(_logicMachine);
                _logicMachine.QueueFree();
            }

            _logicMachine = null;
        }

        if (_inputArea != null)
        {
            _inputArea.InputEvent -= OnAreaInputEvent;
            if (_inputArea.IsInsideTree())
            {
                RemoveChild(_inputArea);
                _inputArea.QueueFree();
            }

            _inputArea = null;
        }
    }

    public override void _Notification(int what)
    {
        this.Notify(what);
    }

    // Declare the partial method for initializing dragging functionality
    partial void InitializeDragging();

    public virtual void SetInToolbar(bool value)
    {
        if (_logicMachine == null) return;

        if (value && State != BlockState.InToolbar)
        {
            _logicMachine.Send(new Input.ReturnBlock());
        }
    }

    public void ResetConnections()
    {
        Logger.Block.Print($"Block {Name} resetting connections. Current state: {State}");

        if (_inputSocket != null)
        {
            _isInputConnected = false;
            Logger.Block.Print($"Block {Name} reset input socket");
        }

        if (_outputSocket != null)
        {
            _isOutputConnected = false;
            Logger.Block.Print($"Block {Name} reset output socket");
        }

        Logger.Block.Print($"Block {Name} finished resetting connections");
    }

    // Virtual methods for sub-framework interactions
    public virtual void SpawnToken()
    {
    }

    public virtual void CompleteConnection()
    {
    }

    public virtual float GetValue()
    {
        return _value;
    }

    private void HandleStateChanged(State state)
    {
        var oldState = _currentState;
        try
        {
            _currentState = state switch
            {
                _ when state is State.InToolbarState => BlockState.InToolbar,
                _ when state is State.DraggingState => BlockState.Dragging,
                _ when state is State.PlacedState => BlockState.Placed,
                _ when state is State.ConnectedState => BlockState.Connected,
                _ when state is State.ConnectedAndDraggingState => BlockState.ConnectedAndDragging,
                _ => BlockState.InToolbar
            };

            Logger.Block.Print($"Block {Name} attempting state change from {oldState} to {_currentState}");

            var globalPos = GlobalPosition;
            var currentParent = GetParent();

            // Get nodes by direct path
            var blockLayer = GetNode<Node>("/root/Main/BlockLayer");
            var toolbar = GetNode<Node>("/root/Main/Toolbar/BlockContainer");

            if (_currentState == BlockState.InToolbar && toolbar == null)
            {
                Logger.Block.Err("Could not find toolbar at /root/Main/Toolbar/BlockContainer");
                _currentState = oldState;
                return;
            }

            if (_currentState != BlockState.InToolbar && blockLayer == null)
            {
                Logger.Block.Err("Could not find block layer at /root/Main/BlockLayer");
                _currentState = oldState;
                return;
            }

            // Always remove from current parent first
            if (currentParent != null) currentParent.RemoveChild(this);

            // Then add to appropriate new parent
            if (_currentState == BlockState.InToolbar && toolbar != null)
            {
                toolbar.AddChild(this);
                GlobalPosition = globalPos;
                ZIndex = ZIndexConfig.Layers.ToolbarBlock;
                Logger.Block.Print($"Block {Name} moved to toolbar");
            }
            else if (blockLayer != null)
            {
                blockLayer.AddChild(this);
                GlobalPosition = globalPos;
                ZIndex = _currentState == BlockState.Dragging
                    ? ZIndexConfig.Layers.DraggedBlock
                    : ZIndexConfig.Layers.PlacedBlock;
                Logger.Block.Print($"Block {Name} moved to block layer");
            }
        }
        catch (Exception e)
        {
            Logger.Block.Err($"Error during state change for block {Name}: {e.Message}");
            _currentState = oldState;
        }
    }
}