using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace F;

public partial class ConnectionPipe : Node2D
{
    private Node2D? _fromSocket;
    private Node2D? _toSocket;
    private bool _isHovered;
    private bool _isAnimating;
    private float _animationTime;
    private Vector2[] _oldPoints = Array.Empty<Vector2>();
    private PipeBulge? _bulgeEffect;
    private Line2D? _visualPipe;

    public Node2D? FromSocket => _fromSocket;
    public Node2D? ToSocket => _toSocket;

    public override void _Ready()
    {
        _bulgeEffect = GetNode<PipeBulge>("PipeBulge");
        _visualPipe = GetNode<Line2D>("VisualPipe");
        
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

        // Set initial Line2D properties with wider width
        _visualPipe.Width = 64.0f;
        _visualPipe.DefaultColor = AnimConfig.Pipe.LineColor;
        _visualPipe.JointMode = Line2D.LineJointMode.Round;
        _visualPipe.BeginCapMode = Line2D.LineCapMode.Round;
        _visualPipe.EndCapMode = Line2D.LineCapMode.Round;
        _visualPipe.Antialiased = true;

        // Set up shader material
        var material = _bulgeEffect.GetShaderMaterial();
        if (material != null)
        {
            _visualPipe.Material = material;
        }
    }

    public void Initialize(Node2D fromSocket, Node2D toSocket)
    {
        _fromSocket = fromSocket;
        _toSocket = toSocket;
        UpdateVisuals();
    }

    public override void _Process(double delta)
    {
        if (_fromSocket == null || _toSocket == null) return;

        if (_isAnimating)
        {
            _animationTime += (float)delta;
            UpdateVisuals();

            if (_animationTime >= AnimConfig.Pipe.SpringDuration)
            {
                _isAnimating = false;
                _oldPoints = GeneratePoints(
                    ToLocal(_fromSocket.GlobalPosition),
                    ToLocal(_toSocket.GlobalPosition)
                );
            }
        }
        else
        {
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {
        if (_fromSocket == null || _toSocket == null || _visualPipe == null) return;

        var startPos = ToLocal(_fromSocket.GlobalPosition);
        var endPos = ToLocal(_toSocket.GlobalPosition);
        List<Vector2> points;

        if (_isAnimating)
        {
            points = new List<Vector2>();
            float t = _animationTime / AnimConfig.Pipe.SpringDuration;
            float spring = 1 + Mathf.Sin(t * (float)Math.PI * 3) * Mathf.Pow(1 - t, 2) * AnimConfig.Pipe.SpringStrength;

            var targetPoints = GeneratePoints(startPos, endPos);
            for (int i = 0; i < _oldPoints.Length && i < targetPoints.Length; i++)
            {
                var oldPoint = _oldPoints[i];
                var targetPoint = targetPoints[i];
                var diff = targetPoint - oldPoint;
                points.Add(oldPoint + diff * spring);
            }
        }
        else
        {
            points = new List<Vector2>(GeneratePoints(startPos, endPos));
        }

        if (points.Count > 0)
        {
            _visualPipe.Points = points.ToArray();
            
            // Update shader material color based on hover state
            if (_bulgeEffect?.GetShaderMaterial() is ShaderMaterial material)
            {
                material.SetShaderParameter("line_color", _isHovered ? AnimConfig.Pipe.HoverColor : AnimConfig.Pipe.LineColor);
            }

            // Update shader with current points
            if (_bulgeEffect != null)
            {
                _bulgeEffect.SetPipePath(_visualPipe);
            }
        }
    }

    private Vector2[] GeneratePoints(Vector2 start, Vector2 end)
    {
        var points = new List<Vector2>();
        var verticalDiff = end.Y - start.Y;
        var horizontalDist = end.X - start.X;

        // Add start point
        points.Add(start);

        // Add control points for smoother curve
        var cp1 = new Vector2(start.X + horizontalDist * 0.33f, start.Y);
        var cp2 = new Vector2(end.X - horizontalDist * 0.33f, end.Y);

        // Generate curve points
        for (int i = 1; i < AnimConfig.Pipe.CurveResolution; i++)
        {
            float t = i / (float)AnimConfig.Pipe.CurveResolution;
            points.Add(CubicBezier(start, cp1, cp2, end, t));
        }

        // Add end point
        points.Add(end);

        return points.ToArray();
    }

    private Vector2 CubicBezier(Vector2 start, Vector2 cp1, Vector2 cp2, Vector2 end, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        return start * uuu
             + cp1 * (3 * uu * t)
             + cp2 * (3 * u * tt)
             + end * ttt;
    }

    public Vector2 GetPositionAlongCurve(float progress)
    {
        if (_fromSocket == null || _toSocket == null) return Vector2.Zero;

        var start = ToLocal(_fromSocket.GlobalPosition);
        var end = ToLocal(_toSocket.GlobalPosition);
        var horizontalDist = end.X - start.X;

        // Use same control points as in GeneratePoints
        var cp1 = new Vector2(start.X + horizontalDist * 0.33f, start.Y);
        var cp2 = new Vector2(end.X - horizontalDist * 0.33f, end.Y);

        return CubicBezier(start, cp1, cp2, end, progress);
    }

    public void UpdateTokenPosition(Vector2 globalPosition, float progress)
    {
        if (_bulgeEffect != null)
        {
            // Use the progress to get position along curve instead of raw position
            var curvePosition = GetPositionAlongCurve(progress);
            _bulgeEffect.UpdateTokenPosition(curvePosition);
        }
    }

    public void UpdateTokenPosition(Vector2 position)
    {
        if (_bulgeEffect != null)
        {
            _bulgeEffect.UpdateTokenPosition(ToLocal(position));
        }
    }

    public void SetHovered(bool hovered)
    {
        _isHovered = hovered;
    }

    public bool IsPointNearPipe(Vector2 point)
    {
        if (_fromSocket == null || _toSocket == null) return false;

        var localPoint = ToLocal(point);
        var startPos = ToLocal(_fromSocket.GlobalPosition);
        var endPos = ToLocal(_toSocket.GlobalPosition);

        // Check if point is within rectangle formed by start and end, expanded for easier selection
        var rect = new Rect2(startPos, endPos - startPos).Abs();
        rect = rect.Grow(AnimConfig.Pipe.HoverDistance);
        return rect.HasPoint(localPoint);
    }

    public (Node2D from, Node2D to) GetSockets()
    {
        if (_fromSocket == null || _toSocket == null)
        {
            throw new InvalidOperationException("Cannot get sockets when they are null");
        }
        return (_fromSocket, _toSocket);
    }

    public void StartReconnectAnimation()
    {
        if (_fromSocket == null || _toSocket == null) return;

        _isAnimating = true;
        _animationTime = 0f;
        _oldPoints = GeneratePoints(
            ToLocal(_fromSocket.GlobalPosition),
            ToLocal(_toSocket.GlobalPosition)
        );
    }

    public void ClearBulgeEffect()
    {
        if (_bulgeEffect != null)
        {
            // Move token position far away to effectively hide the bulge
            _bulgeEffect.UpdateTokenPosition(new Vector2(-10000, -10000));
        }
    }
}
