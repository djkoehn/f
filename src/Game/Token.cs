using Godot;
using System;

namespace F;

public partial class Token : Node2D
{
    public float Value { get; set; }
    private BaseBlock? _targetBlock;
    private Vector2 _targetPosition;
    private bool _isMoving;

    public override void _Ready()
    {
        // Token is just a position marker now
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
            _targetBlock = null;
        }
        else
        {
            GlobalPosition += direction.Normalized() * GameConfig.TOKEN_MOVE_SPEED * (float)delta;
        }
    }
}
