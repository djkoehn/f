using F.Config.Game;
using F.Game.Connections;

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

    public bool IsMovementComplete => !_isMoving;

    public override void _Ready()
    {
        base._Ready();
        _sprite = GetNode<Sprite2D>("Sprite");
        _label = GetNode<Label>("Label");
        UpdateValue(0);
        Connect(SignalName.MovementComplete, new Callable(this, nameof(OnMovementComplete)));
        Connect(SignalName.MovementStart, new Callable(this, nameof(OnMovementStart)));
    }

    public void StartMovement(ConnectionPipe pipe, Vector2 startPos, Vector2 endPos)
    {
        if (_movementTween != null && _movementTween.IsValid()) _movementTween.Kill();

        _isMoving = true;
        GlobalPosition = startPos;
        _movementTween = CreateTween();
        _movementTween.SetTrans(Tween.TransitionType.Linear);
        _movementTween.SetEase(Tween.EaseType.InOut);
        _movementTween.TweenProperty(this, "global_position", endPos, TokenConfig.Animation.MovementDuration);
        _movementTween.TweenCallback(Callable.From(() => EmitSignal(SignalName.MovementComplete)));

        EmitSignal(SignalName.MovementStart);
    }

    public void CompleteMovement()
    {
        if (_movementTween != null && _movementTween.IsValid()) _movementTween.Kill();
        _isMoving = false;
        EmitSignal(SignalName.MovementComplete);
    }

    private void OnMovementComplete()
    {
        _isMoving = false;
    }

    private void OnMovementStart()
    {
        _isMoving = true;
        GD.Print("Movement started");
    }

    public void TriggerAnimation()
    {
        if (_sprite == null) return;

        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Elastic);
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenProperty(_sprite, "scale", Vector2.One * 1.2f, 0.3f);
        tween.TweenProperty(_sprite, "scale", Vector2.One, 0.3f);
    }

    public void TriggerHitEffect()
    {
        if (_sprite == null) return;

        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Bounce);
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenProperty(_sprite, "rotation", Mathf.Pi * 0.1f, 0.2f);
        tween.TweenProperty(_sprite, "rotation", 0f, 0.2f);
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