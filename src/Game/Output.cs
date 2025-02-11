using Godot;

namespace F;

public partial class Output : BaseBlock
{
    private Node2D? _inputSocket;

    public override void _Ready()
    {
        base._Ready();
        _inputSocket = GetNode<Node2D>("BlockInputSocket");
    }

    public Node2D? GetInputSocket() => _inputSocket;

    public void ProcessValue(float value)
    {
        EmitSignal(SignalName.TokenProcessed, value);
    }

    [Signal]
    public delegate void TokenProcessedEventHandler(float value);
}