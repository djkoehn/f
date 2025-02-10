using Godot;

namespace F;

public partial class Input : Node2D
{
    private float _value = 1.0f;
    private Node2D? _outputSocket;
    
    public override void _Ready()
    {
        // Get output socket
        _outputSocket = GetNode<Node2D>("BlockOutputSocket");

        
        // Disable dragging for input blocks
        SetProcess(false);
        SetProcessInput(false);
    }
    
    public void SetValue(float value)
    {
        _value = value;
        var valueLabel = GetNodeOrNull<Label>("BlockValueLabel");
        if (valueLabel != null)
        {
            valueLabel.Text = value.ToString();
        }
    }
    
    public float GetValue() => _value;
    
    public Node2D? GetOutputSocket() => _outputSocket;
}
