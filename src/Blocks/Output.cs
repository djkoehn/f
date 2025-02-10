using Godot;

namespace F;

[Tool]
public partial class Output : Node2D
{
    [Signal]
    public delegate void TokenProcessedEventHandler(float value);
    
    private Node2D? _inputSocket;
    
    public override void _Ready()
    {
        // Get input socket
        _inputSocket = GetNode<Node2D>("BlockInputSocket");
        
        
        // Disable dragging for output blocks
        SetProcess(false);
        SetProcessInput(false);
    }
    
    public void ProcessValue(float value)
    {
        var valueLabel = GetNodeOrNull<Label>("BlockValueLabel");
        if (valueLabel != null)
        {
            valueLabel.Text = value.ToString();
        }
        EmitSignal(SignalName.TokenProcessed, value);
    }
    
    public Node2D? GetInputSocket() => _inputSocket;
}
