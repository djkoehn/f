using Godot;

namespace F;

public partial class Token : Node2D
{
    public float Value { get; set; }
    private BaseBlock? _targetBlock;
    private Vector2 _targetPosition;
    private bool _isMoving;
    private const float MOVE_SPEED = 400f;

    public override void _Ready()
    {
        // Set initial appearance
        var circle = new ColorRect
        {
            Size = new Vector2(10, 10),
            Position = new Vector2(-5, -5),
            Color = new Color(1, 1, 1)
        };
        AddChild(circle);
    }

    public void MoveTo(BaseBlock nextBlock, Vector2 position)
    {
        _targetBlock = nextBlock;
        _targetPosition = position;
        _isMoving = true;
        GD.Print($"Token moving to {nextBlock.Name}, target: {position}, current: {GlobalPosition}");
    }

    public override void _Process(double delta)
    {
        if (!_isMoving || _targetBlock == null) return;

        var direction = _targetPosition - GlobalPosition;
        var distance = direction.Length();

        if (distance < 1.0f)
        {
            GlobalPosition = _targetPosition;
            _isMoving = false;
            
            GD.Print($"Token reached {_targetBlock.Name}, starting processing at value: {Value}");
            var block = _targetBlock;
            _targetBlock = null;  // Clear reference before processing
            block.ProcessToken(this);
        }
        else
        {
            GlobalPosition += direction.Normalized() * MOVE_SPEED * (float)delta;
        }
    }
}
