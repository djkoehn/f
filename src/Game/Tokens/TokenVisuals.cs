namespace F.Game.Tokens;

public partial class TokenVisuals : Node2D
{
    [Signal]
    public delegate void MovementCompleteEventHandler();

    [Signal]
    public delegate void MovementStartEventHandler();

    private float _currentValue;
    private bool _isMoving;
    private Label? _label;
    private Tween? _movementTween;
    private Tween? _animationTween;
    private Sprite2D? _sprite;
    private ShaderMaterial? _glowMaterial;

    public bool IsMovementComplete => !_isMoving;

    public override void _Ready()
    {
        base._Ready();
        
        // Get required nodes
        _sprite = GetNode<Sprite2D>("Sprite");
        _label = GetNode<Label>("Label");
        
        // Setup shader material
        if (_sprite != null)
        {
            _glowMaterial = _sprite.Material as ShaderMaterial;
            if (_glowMaterial == null)
            {
                GD.PrintErr("Glow shader material not found on token sprite!");
            }
        }
        
        UpdateValue(0);
        Connect(SignalName.MovementComplete, new Callable(this, nameof(OnMovementComplete)));
        Connect(SignalName.MovementStart, new Callable(this, nameof(OnMovementStart)));
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        
        // Kill any active tweens
        _movementTween?.Kill();
        _animationTween?.Kill();
        
        // Cleanup signals
        if (IsConnected(SignalName.MovementComplete, new Callable(this, nameof(OnMovementComplete))))
        {
            Disconnect(SignalName.MovementComplete, new Callable(this, nameof(OnMovementComplete)));
        }
        if (IsConnected(SignalName.MovementStart, new Callable(this, nameof(OnMovementStart))))
        {
            Disconnect(SignalName.MovementStart, new Callable(this, nameof(OnMovementStart)));
        }
    }

    public void StartMovement(Vector2 targetPosition)
    {
        // Kill any existing movement tween
        _movementTween?.Kill();

        _isMoving = true;
        _movementTween = CreateTween();
        _movementTween.SetTrans(Tween.TransitionType.Linear);
        _movementTween.SetEase(Tween.EaseType.InOut);

        // Set glow parameters if shader is available
        if (_glowMaterial != null)
        {
            _glowMaterial.SetShaderParameter("glow_width", TokenConfig.Visual.GlowWidth);
            _glowMaterial.SetShaderParameter("glow_intensity", TokenConfig.Visual.GlowIntensity);
        }

        // Calculate movement duration based on distance
        var distance = (targetPosition - GlobalPosition).Length();
        var duration = Math.Min(
            distance / TokenConfig.Movement.MoveSpeed,
            TokenConfig.Animation.MovementDuration
        );

        // Create parallel tweens for both this node and its parent
        _movementTween.Parallel();
        _movementTween.TweenProperty(this, "global_position", targetPosition, duration);
        _movementTween.TweenProperty(GetParent<Node2D>(), "global_position", targetPosition, duration);

        _movementTween.TweenCallback(Callable.From(() => EmitSignal(SignalName.MovementComplete)));

        EmitSignal(SignalName.MovementStart);
    }

    public void StopMovement()
    {
        _movementTween?.Kill();
        _isMoving = false;
    }

    private void OnMovementComplete()
    {
        _isMoving = false;
    }

    private void OnMovementStart()
    {
        _isMoving = true;
    }

    public void TriggerAnimation()
    {
        if (_sprite == null) return;

        _animationTween?.Kill();
        _animationTween = CreateTween();
        _animationTween.SetTrans(Tween.TransitionType.Elastic);
        _animationTween.SetEase(Tween.EaseType.Out);
        _animationTween.TweenProperty(_sprite, "scale", Vector2.One * TokenConfig.Animation.ScaleFactor, TokenConfig.Animation.ScaleDuration);
        _animationTween.TweenProperty(_sprite, "scale", Vector2.One, TokenConfig.Animation.ScaleDuration);
    }

    public void TriggerHitEffect()
    {
        if (_sprite == null) return;

        _animationTween?.Kill();
        _animationTween = CreateTween();
        _animationTween.SetTrans(Tween.TransitionType.Bounce);
        _animationTween.SetEase(Tween.EaseType.Out);
        _animationTween.TweenProperty(_sprite, "scale", Vector2.One * TokenConfig.Animation.HitScaleFactor, TokenConfig.Animation.HitScaleDuration);
        _animationTween.TweenProperty(_sprite, "scale", Vector2.One, TokenConfig.Animation.HitScaleDuration * 2);
    }

    public void UpdateValue(float value)
    {
        _currentValue = value;
        if (_label != null)
        {
            _label.Text = value.ToString("F1");
        }
    }

    public float GetValue()
    {
        return _currentValue;
    }
}