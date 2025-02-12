using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace F;

public partial class Token : Node2D
{
    public float Value { get; set; }
    private BaseBlock? _targetBlock;
    private Vector2 _targetPosition;
    private bool _isMoving;
    public BaseBlock? CurrentBlock { get; private set; }
    private Sprite2D? _coinSprite;
    private Sprite2D? _glowSprite;
    private Tween? _moveTween;

    public HashSet<BaseBlock> ProcessedBlocks { get; } = new();

    public override void _Ready()
    {
        _coinSprite = GetNode<Sprite2D>("CoinSprite");
        _glowSprite = GetNode<Sprite2D>("GlowSprite");
    }

    public void MoveTo(BaseBlock nextBlock)
    {
        _targetBlock = nextBlock;
        _isMoving = true;

        var nextPosition = nextBlock.GetTokenPosition();
        _moveTween = CreateTween();
        _moveTween.TweenProperty(this, "position", nextPosition, 0.5f)
                 .SetTrans(Tween.TransitionType.Sine)
                 .SetEase(Tween.EaseType.InOut);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_isMoving || _targetBlock == null) return;

        if (_moveTween != null && !_moveTween.IsRunning())
        {
            _isMoving = false;
            CurrentBlock = _targetBlock;
            _targetBlock = null;
            
            GD.Print($"Token reached block {CurrentBlock?.Name} with value {Value}");

            // Trigger token processing at the current block
            CurrentBlock?.ProcessToken(this);
        }
    }
}
