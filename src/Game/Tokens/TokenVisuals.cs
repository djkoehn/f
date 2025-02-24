using F.Framework.Logging;
using Godot;

namespace F.Game.Tokens;

public partial class TokenVisuals : Node2D
{
    [Signal]
    public delegate void MovementCompleteEventHandler();

    [Signal]
    public delegate void MovementStartEventHandler();

    private Tween? _animationTween;

    private float _currentValue;
    private ShaderMaterial? _glowMaterial;
    private bool _isMoving;
    private Label? _label;
    private Tween? _movementTween;
    private Sprite2D? _sprite;

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
                Logger.Token.Err("Glow shader material not found on token sprite!");
                return;
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
            Disconnect(SignalName.MovementComplete, new Callable(this, nameof(OnMovementComplete)));
        if (IsConnected(SignalName.MovementStart, new Callable(this, nameof(OnMovementStart))))
            Disconnect(SignalName.MovementStart, new Callable(this, nameof(OnMovementStart)));
    }

    public void MoveTo(Vector2 targetPosition, float duration = 0.5f)
    {
        _movementTween?.Kill();
        _movementTween = CreateTween();

        _movementTween.TweenProperty(this, "global_position", targetPosition, duration)
            .SetTrans(Tween.TransitionType.Expo)
            .SetEase(Tween.EaseType.Out);

        _movementTween.TweenCallback(Callable.From(() => EmitSignal(SignalName.MovementComplete)));

        EmitSignal(SignalName.MovementStart);
        Logger.Token.Print($"Token started moving to {targetPosition}");
    }

    private void OnMovementComplete()
    {
        _isMoving = false;
        Logger.Token.Print($"Token completed movement at {GlobalPosition}");
    }

    private void OnMovementStart()
    {
        _isMoving = true;
        Logger.Token.Print($"Token started movement from {GlobalPosition}");
    }

    public void TriggerAnimation()
    {
        if (_sprite == null) return;

        _animationTween?.Kill();
        _animationTween = CreateTween();
        _animationTween.SetTrans(Tween.TransitionType.Elastic);
        _animationTween.SetEase(Tween.EaseType.Out);
        _animationTween.TweenProperty(_sprite, "scale", Vector2.One * TokenConfig.Animation.ScaleFactor,
            TokenConfig.Animation.ScaleDuration);
        _animationTween.TweenProperty(_sprite, "scale", Vector2.One, TokenConfig.Animation.ScaleDuration);
    }

    public void TriggerHitEffect()
    {
        if (_sprite == null) return;

        _animationTween?.Kill();
        _animationTween = CreateTween();
        _animationTween.SetTrans(Tween.TransitionType.Bounce);
        _animationTween.SetEase(Tween.EaseType.Out);
        _animationTween.TweenProperty(_sprite, "scale", Vector2.One * TokenConfig.Animation.HitScaleFactor,
            TokenConfig.Animation.HitScaleDuration);
        _animationTween.TweenProperty(_sprite, "scale", Vector2.One, TokenConfig.Animation.HitScaleDuration * 2);
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