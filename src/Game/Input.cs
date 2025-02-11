using Godot;

namespace F;

public partial class Input : BaseBlock
{
    private Node2D? _outputSocket;
    private float _value = 0f;

    public override void _Ready()
    {
        base._Ready();
        _outputSocket = GetNode<Node2D>("BlockOutputSocket");
    }

    public Node2D? GetOutputSocket() => _outputSocket;

    public void SetValue(float value)
    {
        _value = value;
    }

    public float GetValue() => _value;
}