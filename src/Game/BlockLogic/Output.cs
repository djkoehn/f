using F.Game.Tokens;

namespace F.Game.BlockLogic;

public partial class Output : Node2D, IBlock
{
    private Node2D? _inputSocket;
    private float _value;
    private Label? _valueLabel;

    public new string Name { get; set; } = "Output";
    
    public override void _Ready()
    {
        base._Ready();
        _inputSocket = GetNode<Node2D>("BlockInputSocket");
        _valueLabel = GetNode<Label>("Value");
        
        if (_inputSocket == null)
        {
            GD.PrintErr("Input socket not found for Output block. Ensure 'BlockInputSocket' exists in the scene.");
        }
        
        if (_valueLabel == null)
        {
            GD.PrintErr("Value label not found for Output block. Ensure 'Value' label exists in the scene.");
        }
        else
        {
            _valueLabel.Show(); // Make sure the label is visible
        }
    }

    public Node? GetInputSocket()
    {
        return _inputSocket;
    }

    public Node? GetOutputSocket()
    {
        return null;
    }

    public bool HasConnections()
    {
        return false;
    }

    public Vector2 GetTokenPosition()
    {
        return GlobalPosition;
    }

    public void ProcessToken(Token token)
    {
        _value = token.Value;
        GD.Print($"[Output Debug] Received token with value: {_value}");
        
        if (_valueLabel != null)
        {
            _valueLabel.Text = _value.ToString("F1");
            GD.Print($"[Output Debug] Updated value label to: {_valueLabel.Text}");
        }
        else
        {
            GD.PrintErr("[Output Debug] Value label is null, cannot display value!");
        }
        
        token.QueueFree();
    }

    void IBlock.Initialize(object config)
    {
        if (config is BlockConfig blockConfig)
        {
            // No metadata initialization needed for Output block
        }
        else
        {
            GD.PrintErr("Invalid config passed to Output.Initialize");
        }
    }

    // Since Output blocks are stationary, these dragging methods are no-ops
    public void SetDragging(bool dragging) { /* No-op: Output is stationary */ }

    public void SetPlaced(bool placed) { /* No-op: Output is stationary */ }
}