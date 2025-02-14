using F.Config.Connection;
using F.UI.Animations;

namespace F.Game.Connections;

public partial class PipeVisuals : Node2D
{
    private float _animationTime;
    private PipeBulge? _bulgeEffect;
    private bool _isAnimating;
    private bool _isHovered;
    private Vector2[] _oldPoints = Array.Empty<Vector2>();
    private Line2D? _visualPipe;

    public override void _Ready()
    {
        _visualPipe = GetNode<Line2D>("VisualPipe");
        _bulgeEffect = GetNode<PipeBulge>("VisualPipe/PipeBulge");

        if (_visualPipe == null)
        {
            GD.PrintErr("VisualPipe node not found!");
            return;
        }

        if (_bulgeEffect == null)
        {
            GD.PrintErr("PipeBulge node not found!");
            return;
        }

        InitializeVisualPipe();
    }

    private void InitializeVisualPipe()
    {
        _visualPipe!.Width = 4.0f;
        _visualPipe.DefaultColor = PipeConfig.Visual.LineColor;
        _visualPipe.JointMode = Line2D.LineJointMode.Round;
        _visualPipe.BeginCapMode = Line2D.LineCapMode.Round;
        _visualPipe.EndCapMode = Line2D.LineCapMode.Round;
        _visualPipe.Antialiased = true;
    }

    public void UpdateVisuals(Vector2 startPoint, Vector2 endPoint)
    {
        if (_visualPipe == null) return;

        _visualPipe.ClearPoints();
        _visualPipe.AddPoint(startPoint);
        _visualPipe.AddPoint(endPoint);

        _bulgeEffect?.UpdatePoints();
    }

    public void SetHovered(bool isHovered)
    {
        _isHovered = isHovered;
        if (_visualPipe != null)
            _visualPipe.DefaultColor = _isHovered ? PipeConfig.Visual.HoverColor : PipeConfig.Visual.LineColor;
    }

    public void StartReconnectAnimation(Vector2[] oldPoints)
    {
        if (_visualPipe == null) return;

        _isAnimating = true;
        _animationTime = 0;
        _oldPoints = oldPoints;
    }

    public void ClearBulgeEffect()
    {
        _bulgeEffect?.SetSpeed(0);
    }

    public void UpdateTokenPosition(Vector2 position)
    {
        _bulgeEffect?.SetSpeed(1.0f);
    }

    public Vector2 GetPositionAlongCurve(float t)
    {
        if (_visualPipe == null || _visualPipe.Points.Length < 2) return Vector2.Zero;
        return _visualPipe.Points[0].Lerp(_visualPipe.Points[1], t);
    }

    public override void _Process(double delta)
    {
        if (!_isAnimating || _visualPipe == null) return;

        _animationTime += (float)delta;
        var t = Mathf.Min(_animationTime / PipeConfig.Animation.ReconnectDuration, 1.0f);

        if (t >= 1.0f)
        {
            _isAnimating = false;
            return;
        }

        // Update points based on animation progress
        _visualPipe.ClearPoints();
        for (var i = 0; i < _oldPoints.Length; i++)
        {
            var point = _oldPoints[i];
            var targetPoint = i < _visualPipe.Points.Length ? _visualPipe.Points[i] : _visualPipe.Points[^1];
            _visualPipe.AddPoint(point.Lerp(targetPoint, t));
        }

        _bulgeEffect?.UpdatePoints();
    }
}