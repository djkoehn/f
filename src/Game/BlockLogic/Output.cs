using F.Game.Tokens;

namespace F.Game.BlockLogic;

public partial class Output : Node2D, IBlock
{
    private Node2D? _inputSocket;
    private float _value;

    public new string Name { get; set; } = "Output";
    
    public override void _Ready()
    {
        base._Ready();
        _inputSocket = GetNode<Node2D>("BlockInputSocket");
        if (_inputSocket == null)
        {
            GD.PrintErr("Input socket not found for Output block. Ensure 'BlockInputSocket' exists in the scene.");
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