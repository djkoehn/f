namespace F.UI.Animations.UI;

public sealed partial class ToolbarHoverAnimation : Node
{
    public delegate void PositionChangedHandler(Vector2 newPosition);

    // Use config values for positioning
    private const float TOOLBAR_HIDDEN_Y = ToolbarConfig.Animation.HideY;
    private const float TOOLBAR_VISIBLE_Y = ToolbarConfig.Animation.ShowY;
    private const float BLOCKLAYER_HIDDEN_Y = 0f;
    private const float BLOCKLAYER_VISIBLE_Y = -128f; // Move up to make room for toolbar

    private const float DURATION = ToolbarConfig.Animation.Duration;
    private readonly bool _isControl;

    private readonly Node _target;
    private Vector2 _endPos;
    private Vector2 _startPos;
    private float _time;

    private ToolbarHoverAnimation(Node target, bool show)
    {
        _target = target;
        _isControl = target is Control;
        _time = 0f;

        // Initialize position to hidden state if not already set
        if (_isControl)
        {
            var control = _target as Control;
            if (control != null && control.Position.Y == 0) // If at default position
                control.Position = new Vector2(control.Position.X, TOOLBAR_HIDDEN_Y);
        }
        else
        {
            var node2D = _target as Node2D;
            if (node2D != null && node2D.Position.Y == 0) // If at default position
                node2D.Position = new Vector2(node2D.Position.X, BLOCKLAYER_HIDDEN_Y);
        }

        var currentY = _isControl
            ? (_target as Control)?.Position.Y ?? TOOLBAR_HIDDEN_Y
            : (_target as Node2D)?.Position.Y ?? BLOCKLAYER_HIDDEN_Y;

        float startY, endY;
        if (_isControl)
        {
            // When showing: start from current position and go to visible
            // When hiding: start from current position and go to hidden
            startY = currentY;
            endY = show ? TOOLBAR_VISIBLE_Y : TOOLBAR_HIDDEN_Y;
        }
        else
        {
            // Same logic for BlockLayer
            startY = currentY;
            endY = show ? BLOCKLAYER_VISIBLE_Y : BLOCKLAYER_HIDDEN_Y;
        }

        _startPos = new Vector2(0, startY);
        _endPos = new Vector2(0, endY);

        GD.Print(
            $"[ToolbarHoverAnimation] Initializing animation for {(_isControl ? "Control" : "Node2D")}, show: {show}");
        GD.Print($"[ToolbarHoverAnimation] Current Y: {currentY}, Start Y: {startY}, End Y: {endY}");
    }

    public bool IsComplete => _time >= DURATION;
    public event PositionChangedHandler? PositionChanged;

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
            node2DTarget.Position = newPosition;
        else
            _target.Set("position", newPosition);

        PositionChanged?.Invoke(newPosition);

        if (IsComplete) QueueFree();
    }
}