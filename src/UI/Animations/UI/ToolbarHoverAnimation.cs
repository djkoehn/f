using F.Framework.Logging;
using Godot;

namespace F.UI.Animations.UI;

public class ToolbarHoverAnimation
{
    private readonly Node2D _target;
    private readonly float _startY;
    private readonly float _endY;
    private readonly float _duration;
    private readonly Tween _tween;
    private bool _isHovering;

    public ToolbarHoverAnimation(Node2D target, float hoverDistance = 10f, float duration = 0.3f)
    {
        _target = target;
        _startY = target.GlobalPosition.Y;
        _endY = _startY - hoverDistance;
        _duration = duration;
        _tween = target.CreateTween();
    }

    public void StartHover()
    {
        if (_isHovering) return;
        _isHovering = true;

        var currentY = _target.GlobalPosition.Y;
        Logger.UI.Print($"Starting hover animation - Current Y: {currentY}, Start Y: {_startY}, End Y: {_endY}");

        _tween.Kill();
        _tween.TweenProperty(_target, "global_position:y", _endY, _duration)
            .SetTrans(Tween.TransitionType.Expo)
            .SetEase(Tween.EaseType.Out);
    }

    public void StopHover()
    {
        if (!_isHovering) return;
        _isHovering = false;

        var currentY = _target.GlobalPosition.Y;
        Logger.UI.Print($"Stopping hover animation - Current Y: {currentY}, Start Y: {_startY}, End Y: {_endY}");

        _tween.Kill();
        _tween.TweenProperty(_target, "global_position:y", _startY, _duration)
            .SetTrans(Tween.TransitionType.Expo)
            .SetEase(Tween.EaseType.Out);
    }

    public void Cancel()
    {
        _tween.Kill();
        _target.GlobalPosition = new Vector2(_target.GlobalPosition.X, _startY);
        _isHovering = false;
    }
}