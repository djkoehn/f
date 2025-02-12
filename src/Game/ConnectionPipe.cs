using Godot;
using System;
using System.Collections.Generic;

namespace F;

public partial class ConnectionPipe : Node2D
{
    private Node2D? _fromSocket;
    private Node2D? _toSocket;
    private bool _isHovered;
    private bool _isAnimating;
    private float _animationTime;
    private Vector2[] _oldPoints = Array.Empty<Vector2>();

    public Node2D? FromSocket => _fromSocket;
    public Node2D? ToSocket => _toSocket;

    public void Initialize(Node2D fromSocket, Node2D toSocket)
    {
        _fromSocket = fromSocket;
        _toSocket = toSocket;
    }

    public override void _Process(double delta)
    {
        if (_isAnimating)
        {
            _animationTime += (float)delta;
            QueueRedraw();

            if (_animationTime >= AnimConfig.Pipe.SpringDuration)
            {
                _isAnimating = false;
                _oldPoints = GeneratePoints(
                    ToLocal(_fromSocket!.GlobalPosition),
                    ToLocal(_toSocket!.GlobalPosition)
                );
            }
        }
        else
        {
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        if (_fromSocket == null || _toSocket == null) return;

        var startPos = ToLocal(_fromSocket.GlobalPosition);
        var endPos = ToLocal(_toSocket.GlobalPosition);
        List<Vector2> points;

        if (_isAnimating)
        {
            points = new List<Vector2>();
            float t = _animationTime / AnimConfig.Pipe.SpringDuration;
            float spring = 1 + Mathf.Sin(t * Mathf.Pi * 3) * Mathf.Pow(1 - t, 2) * AnimConfig.Pipe.SpringStrength;

            var targetPoints = GeneratePoints(startPos, endPos);
            for (int i = 0; i < _oldPoints.Length; i++)
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

        var color = _isHovered ? AnimConfig.Pipe.HoverColor : AnimConfig.Pipe.LineColor;
        DrawPolyline(points.ToArray(), color, AnimConfig.Pipe.LineWidth);
    }

    private Vector2[] GeneratePoints(Vector2 start, Vector2 end)
    {
        var points = new List<Vector2>();
        var verticalDiff = end.Y - start.Y;
        var horizontalDist = end.X - start.X;

        for (int i = 0; i <= AnimConfig.Pipe.CurveResolution; i++)
        {
            float t = i / (float)AnimConfig.Pipe.CurveResolution;
            var cp1 = new Vector2(start.X + horizontalDist * 0.33f, start.Y);
            var cp2 = new Vector2(end.X - horizontalDist * 0.33f, end.Y);
            points.Add(CubicBezier(start, cp1, cp2, end, t));
        }

        return points.ToArray();
    }

    private Vector2 CubicBezier(Vector2 start, Vector2 cp1, Vector2 cp2, Vector2 end, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        return uuu * start +
               3 * uu * t * cp1 +
               3 * u * tt * cp2 +
               ttt * end;
    }

    public void StartReconnectAnimation()
    {
        _isAnimating = true;
        _animationTime = 0f;
        _oldPoints = GeneratePoints(
            ToLocal(_fromSocket!.GlobalPosition),
            ToLocal(_toSocket!.GlobalPosition)
        );
        QueueRedraw();
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

    public void SetHovered(bool hovered)
    {
        _isHovered = hovered;
        QueueRedraw();
    }

    public (Node2D from, Node2D to) GetSockets()
    {
        return (_fromSocket!, _toSocket!);
    }

    public Vector2 GetPointAlongPipe(float t)
    {
        if (_fromSocket == null || _toSocket == null) return Vector2.Zero;

        var startPos = ToLocal(_fromSocket.GlobalPosition);
        var endPos = ToLocal(_toSocket.GlobalPosition);

        var localPoint = GetPointAlongCurve(startPos, endPos, t);
        var globalPoint = ToGlobal(localPoint);
        
        GD.Print($"GetPointAlongPipe(t={t}) returning {globalPoint}");
        return globalPoint;
    }

    private Vector2 GetPointAlongCurve(Vector2 start, Vector2 end, float t)
    {
        var points = GeneratePoints(start, end);
        int index = Mathf.FloorToInt(t * (points.Length - 1));
        return points[index];
    }
}
