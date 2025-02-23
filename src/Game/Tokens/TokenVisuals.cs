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
    private Sprite2D? _sprite;
    private ShaderMaterial? _glowMaterial;

    public bool IsMovementComplete => !_isMoving;

    public override void _Ready()
    {
        base._Ready();
        _sprite = GetNode<Sprite2D>("Sprite");
        _label = GetNode<Label>("Label");
        _glowMaterial = GetNode<Sprite2D>("Sprite").Material as ShaderMaterial;
        UpdateValue(0);
        Connect(SignalName.MovementComplete, new Callable(this, nameof(OnMovementComplete)));
        Connect(SignalName.MovementStart, new Callable(this, nameof(OnMovementStart)));
    }

    public void StartMovement(Vector2 targetPosition)
    {
        if (_movementTween != null && _movementTween.IsValid()) _movementTween.Kill();

        _isMoving = true;
        _movementTween = CreateTween();
        _movementTween.SetTrans(Tween.TransitionType.Linear);
        _movementTween.SetEase(Tween.EaseType.InOut);

        // Set glow parameters
        _glowMaterial?.SetShaderParameter("glow_width", TokenConfig.Visual.GlowWidth);
        _glowMaterial?.SetShaderParameter("glow_intensity", TokenConfig.Visual.GlowIntensity);

        // Set movement speed
        var distance = (targetPosition - GlobalPosition).Length();
        var duration = distance / TokenConfig.Movement.MoveSpeed;
        duration = Math.Min(duration, TokenConfig.Animation.MovementDuration);

        // Create parallel tweens for both this node and its parent
        _movementTween.Parallel();
        _movementTween.TweenProperty(this, "global_position", targetPosition, duration);
        _movementTween.TweenProperty(GetParent<Node2D>(), "global_position", targetPosition, duration);

        _movementTween.TweenCallback(Callable.From(() => EmitSignal(SignalName.MovementComplete)));

        EmitSignal(SignalName.MovementStart);
    }

    public void StopMovement()
    {
        if (_movementTween != null && _movementTween.IsValid()) 
        {
            _movementTween.Kill();
            _isMoving = false;
        }
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

        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Elastic);
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenProperty(_sprite, "scale", Vector2.One * 1.5f, 0.3f);
        tween.TweenProperty(_sprite, "scale", Vector2.One, 0.3f);
    }

    public void TriggerHitEffect()
    {
        if (_sprite == null) return;

        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Bounce);
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenProperty(_sprite, "scale", Vector2.One * 1.2f, 0.1f);
        tween.TweenProperty(_sprite, "scale", Vector2.One, 0.2f);
    }

    public void UpdateValue(float value)
    {
        _currentValue = value;
        if (_label != null) _label.Text = value.ToString("F1");
    }

    public float GetValue()
    {
        return _currentValue;
    }
}