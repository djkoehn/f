namespace F.UI.Animations;

public sealed partial class ToolbarHoverAnimation : Node
{
    public delegate void PositionChangedHandler(Vector2 newPosition);
    public event PositionChangedHandler? PositionChanged;

    private const float DURATION = 0.3f;
    private const float HIDDEN_Y = 0f;      // Changed to match BlockLayer movement
    private const float VISIBLE_Y = -256f;  // Changed to match BlockLayer movement
    private Vector2 _endPos;
    private bool _isShowing;
    private Vector2 _startPos;

    private Control _target;
    private float _time;

    private ToolbarHoverAnimation(Control target, bool show)
    {
        _target = target;
        _isShowing = show;
        _time = 0f;

        var startY = show ? HIDDEN_Y : VISIBLE_Y;
        var endY = show ? VISIBLE_Y : HIDDEN_Y;
        _startPos = new Vector2(0, startY);
        _endPos = new Vector2(0, endY);
    }

    public bool IsComplete => _time >= DURATION;

    public static ToolbarHoverAnimation Create(Control target, bool show)
    {
        var animation = new ToolbarHoverAnimation(target, show);
        target.AddChild(animation);
        return animation;
    }

    public override void _Process(double delta)
    {
        _time += (float)delta;
        var t = Mathf.Min(_time / DURATION, 1.0f);
        var easedT = Easing.OutExpo(t);

        var newPosition = _startPos.Lerp(_endPos, easedT);
        _target.Position = newPosition;
        PositionChanged?.Invoke(newPosition);

        if (IsComplete) QueueFree();
    }
}