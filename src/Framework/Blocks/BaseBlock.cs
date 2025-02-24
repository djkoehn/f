using Godot;
using F.Game.BlockLogic;
using F.Framework.Input;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using F.Framework.Core;
using F.Game.Tokens;
using Chickensoft.Log;

namespace F.Framework.Blocks;

[Meta(typeof(IAutoNode))]
public partial class BaseBlock : Node2D, IBlock, IProvide<Node>
{
  private Area2D? _inputArea;
  private BlockLogicMachine? _logicMachine;
  private BlockState _currentState = BlockState.InToolbar;
  private BlockMetadata? _metadata;
  private bool _isInputConnected;
  private bool _isOutputConnected;
  private Node2D? _inputSocket;
  private Node2D? _outputSocket;
  private Label? _valueLabel;
  protected float _value = 1.0f;
  private string _blockName = "";

  string IBlock.Name
  {
    get => Name;
    set => Name = value;
  }

  public BlockMetadata? Metadata
  {
    get => _metadata;
    set
    {
      _metadata = value;
      GD.Print($"[BaseBlock Debug] Setting metadata for block {Name} - Value: {(value != null ? $"Id={value.Id}, SpawnOnSpace={value.SpawnOnSpace}" : "null")}");
      if (_metadata != null)
      {
        InitializeSockets();
      }
    }
  }

  public BlockState State
  {
    get => _currentState;
    protected internal set
    {
      var oldState = _currentState;
      _currentState = value;
      GD.Print($"[BaseBlock Debug] Block {Name} state changed: {oldState} -> {_currentState}");
    }
  }

  public override void _Ready()
  {
    if (_logicMachine == null)
    {
      _logicMachine = new BlockLogicMachine();
      _logicMachine.StateChanged += HandleStateChanged;
      AddChild(_logicMachine);
      GD.Print($"[BaseBlock Debug] Initialized BlockLogicMachine for block {Name}");
    }

    // Add to the Blocks group for interaction detection
    AddToGroup("Blocks");
    GD.Print($"[BaseBlock Debug] Added block {Name} to Blocks group");

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
      GD.Print($"[BaseBlock Debug] Created input area for block {Name}");
    }

    this.Provide();
    InitializeSockets();

    GD.Print($"[BaseBlock Debug] Block {Name} ready, LogicMachine: {_logicMachine != null}");
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

  public void Initialize(object config) { }

  public bool HasConnections() => HasInputConnection() || HasOutputConnection();
  public bool HasInputConnection() => _isInputConnected;
  public bool HasOutputConnection() => _isOutputConnected;
  public void SetInputConnected(bool connected) => _isInputConnected = connected;
  public void SetOutputConnected(bool connected) => _isOutputConnected = connected;

  public virtual void ProcessToken(Token token)
  {
    if (token == null || Metadata == null) return;

    GD.Print($"[BaseBlock Debug] Processing token in block {Name}, current value: {token.Value}");

    if (!string.IsNullOrEmpty(Metadata.ProcessTokenScript))
    {
      try
      {
        float Value = token.Value;
        var script = Metadata.ProcessTokenScript;

        if (script.Contains("Value = "))
        {
          var valueStr = script.Split('=')[1].Trim().TrimEnd('f', ';');
          if (float.TryParse(valueStr, out float newValue))
          {
            Value = newValue;
          }
        }
        else if (script.Contains("Value += "))
        {
          var valueStr = script.Split('+')[1].Trim('=', ' ').TrimEnd('f', ';');
          if (float.TryParse(valueStr, out float addValue))
          {
            Value += addValue;
          }
        }
        else if (script.Contains("Value *= "))
        {
          var valueStr = script.Split('*')[1].Trim('=', ' ').TrimEnd('f', ';');
          if (float.TryParse(valueStr, out float mulValue))
          {
            Value *= mulValue;
          }
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
        GD.PrintErr($"Error processing token in block {Name}: {e.Message}");
      }
    }
  }

  public Node? GetInputSocket() => _inputSocket;
  public Node? GetOutputSocket() => _outputSocket;
  public Vector2 GetTokenPosition() => GlobalPosition;

  private void OnAreaInputEvent(Node viewport, InputEvent @event, long shapeIdx)
  {
    if (_logicMachine == null) return;

    if (@event.IsActionPressed(InputActions.Interact))
    {
      GD.Print($"[BaseBlock Debug] Block {Name} received interact action");
      _logicMachine.Send(new BlockLogicInput.Interact());
    }
    else if (@event.IsActionPressed(InputActions.ReturnBlock))
    {
      GD.Print($"[BaseBlock Debug] Block {Name} received return action");
      _logicMachine.Send(new BlockLogicInput.ReturnBlock());
    }
  }

  public BlockLogicMachine? GetLogicMachine()
  {
    if (_logicMachine == null)
    {
      // If _Ready hasn't been called yet, call it manually
      _Ready();
    }
    return _logicMachine;
  }

  private void HandleStateChanged(BlockLogicMachine.LogicBlockImpl.BlockLogicState state)
  {
    var oldState = _currentState;
    try
    {
      _currentState = state switch
      {
        BlockLogicMachine.LogicBlockImpl.BlockLogicState.InToolbarState => BlockState.InToolbar,
        BlockLogicMachine.LogicBlockImpl.BlockLogicState.DraggingState => BlockState.Dragging,
        BlockLogicMachine.LogicBlockImpl.BlockLogicState.PlacedState => BlockState.Placed,
        BlockLogicMachine.LogicBlockImpl.BlockLogicState.ConnectedState => BlockState.Connected,
        BlockLogicMachine.LogicBlockImpl.BlockLogicState.ConnectedAndDraggingState => BlockState.Connected,
        _ => _currentState
      };

      GD.Print($"[BaseBlock Debug] Block {Name} attempting state change from {oldState} to {_currentState}");

      var globalPos = GlobalPosition;
      var currentParent = GetParent();

      // Get nodes by direct path
      var blockLayer = GetNode<Node>("/root/Main/BlockLayer");
      var toolbar = GetNode<Node>("/root/Main/Toolbar/BlockContainer");

      if (_currentState == BlockState.InToolbar && toolbar == null)
      {
        GD.PrintErr($"[BaseBlock Debug] Could not find toolbar at /root/Main/Toolbar/BlockContainer");
        _currentState = oldState;
        return;
      }
      else if (_currentState != BlockState.InToolbar && blockLayer == null)
      {
        GD.PrintErr($"[BaseBlock Debug] Could not find block layer at /root/Main/BlockLayer");
        _currentState = oldState;
        return;
      }

      // Always remove from current parent first
      if (currentParent != null)
      {
        currentParent.RemoveChild(this);
      }

      // Then add to appropriate new parent
      if (_currentState == BlockState.InToolbar && toolbar != null)
      {
        toolbar.AddChild(this);
        GlobalPosition = globalPos;
        ZIndex = ZIndexConfig.Layers.ToolbarBlock;
        GD.Print($"[BaseBlock Debug] Block {Name} moved to toolbar");
      }
      else if (blockLayer != null)
      {
        blockLayer.AddChild(this);
        GlobalPosition = globalPos;
        ZIndex = _currentState == BlockState.Dragging ?
            ZIndexConfig.Layers.DraggedBlock :
            ZIndexConfig.Layers.PlacedBlock;
        GD.Print($"[BaseBlock Debug] Block {Name} moved to block layer");
      }
    }
    catch (Exception e)
    {
      GD.PrintErr($"[BaseBlock Debug] Error handling state change for block {Name}: {e.Message}");
      // Revert state if something went wrong
      _currentState = oldState;
    }
  }

  public void NotifyHoveredOverPipe()
  {
    if (_logicMachine == null) return;
    _logicMachine.Send(new BlockLogicInput.HoveredOverPipe());
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

  public override void _Notification(int what) => this.Notify(what);

  Node IProvide<Node>.Value() => this;

  // Declare the partial method for initializing dragging functionality
  partial void InitializeDragging();

  public virtual void SetDragging(bool dragging)
  {
    GD.Print($"[BaseBlock Debug] Block {Name} SetDragging({dragging}) called. Metadata?.IsStationary: {Metadata?.IsStationary}, _logicMachine: {_logicMachine != null}");
    if (Metadata?.IsStationary == true || _logicMachine == null)
    {
      GD.Print($"[BaseBlock Debug] Block {Name} SetDragging early return. IsStationary: {Metadata?.IsStationary}, _logicMachine null: {_logicMachine == null}");
      return;
    }
    GD.Print($"[BaseBlock Debug] Block {Name} sending Interact input to state machine");
    _logicMachine.Send(new BlockLogicInput.Interact());
  }

  public virtual void SetPlaced(bool placed)
  {
    if (Metadata?.IsStationary == true || _logicMachine == null) return;
    _logicMachine.Send(new BlockLogicInput.Interact());
  }

  public virtual void SetInToolbar(bool inToolbar)
  {
    if (Metadata?.IsStationary == true || _logicMachine == null) return;
    _logicMachine.Send(new BlockLogicInput.ReturnBlock());
  }

  public void ResetConnections()
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

  // Virtual methods for sub-framework interactions
  public virtual void SpawnToken() { }
  public virtual void CompleteConnection() { }
  public virtual float GetValue() => _value;
}