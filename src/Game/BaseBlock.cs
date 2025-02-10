using Godot;

namespace F;

public partial class BaseBlock : Node2D
{
    protected BlockMetadata? _metadata;
    private Node2D? _inputSocket;
    private Node2D? _outputSocket;

    [Signal]
    public delegate void BlockPlacedEventHandler(BaseBlock block);

    public override void _Ready()
    {
        base._Ready();
        _inputSocket = GetNodeOrNull<Node2D>("BlockInputSocket");
        _outputSocket = GetNodeOrNull<Node2D>("BlockOutputSocket");
    }

    public void Initialize(BlockMetadata metadata)
    {
        _metadata = metadata;
    }

    public virtual void ProcessToken(Token token)
    {
        // Default implementation (can be overridden by subclasses)
        GD.Print("Processing token in BaseBlock");
    }
}
