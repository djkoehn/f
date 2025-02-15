namespace F.UI.Animations.UI;

public sealed partial class ToolbarHoverAnimation : Node
{
    public delegate void PositionChangedHandler(Vector2 newPosition);
    public event PositionChangedHandler? PositionChanged;

    // Separate Y values for toolbar (Control) and BlockLayer (Node2D)
    private const float TOOLBAR_HIDDEN_Y = 1080f;
    private const float TOOLBAR_VISIBLE_Y = 856f;
    private const float BLOCKLAYER_HIDDEN_Y = 0f;
    private const float BLOCKLAYER_VISIBLE_Y = -50f;

    private const float DURATION = 0.3f;
    private Vector2 _startPos;
    private Vector2 _endPos;
    private float _time;

    private readonly Node _target;
    private readonly bool _isControl;

    private ToolbarHoverAnimation(Node target, bool show)
    {
        _target = target;
        _isControl = target is Control;
        _time = 0f;

        float startY, endY;
        if (_target is Control)
        {
            // For toolbar (Control): if showing, animate from hidden to visible
            startY = show ? TOOLBAR_HIDDEN_Y : TOOLBAR_VISIBLE_Y;
            endY = show ? TOOLBAR_VISIBLE_Y : TOOLBAR_HIDDEN_Y;
        }
        else if (_target is Node2D)
        {
            // For blocklayer (Node2D)
            startY = show ? BLOCKLAYER_HIDDEN_Y : BLOCKLAYER_VISIBLE_Y;
            endY = show ? BLOCKLAYER_VISIBLE_Y : BLOCKLAYER_HIDDEN_Y;
        }
        else
        {
            startY = 0f;
            endY = 0f;
        }

        _startPos = new Vector2(0, startY);
        _endPos = new Vector2(0, endY);
    }

    public bool IsComplete => _time >= DURATION;

    public static ToolbarHoverAnimation Create(Node target, bool show)
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

        if (_target is Node2D node2DTarget)
        {
            node2DTarget.Position = newPosition;
        }
        else
        {
            _target.Set("position", newPosition);
        }

        PositionChanged?.Invoke(newPosition);

        if (IsComplete) QueueFree();
    }
}