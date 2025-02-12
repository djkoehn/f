using Godot;
using System;

namespace F;

public partial class PipeBulgeEffect : Node
{
    private const int MAX_CURVE_POINTS = 50;
    private ShaderMaterial? _material;
    private Line2D? _line;

    public override void _Ready()
    {
        InitializeShader();
    }

    private void InitializeShader()
    {
        var shader = GD.Load<Shader>("res://assets/shaders/pipe_bulge.gdshader");
        if (shader == null)
        {
            GD.PrintErr("Failed to load pipe_bulge shader!");
            return;
        }

        _material = new ShaderMaterial
        {
            Shader = shader
        };

        // Set initial shader parameters with stronger bulge effect
        _material.SetShaderParameter("bulge_amount", 1.5f);
        _material.SetShaderParameter("bulge_softness", 0.15f);
        _material.SetShaderParameter("token_position", Vector2.Zero);
        
        // Initialize curve points array with fixed size
        var emptyPoints = new Vector2[MAX_CURVE_POINTS];
        for (int i = 0; i < MAX_CURVE_POINTS; i++)
        {
            emptyPoints[i] = Vector2.Zero;
        }
        _material.SetShaderParameter("curve_points", emptyPoints);
        _material.SetShaderParameter("point_count", 0);
    }

    public ShaderMaterial? GetShaderMaterial()
    {
        return _material;
    }

    public void SetPipePath(Line2D line)
    {
        _line = line;
        if (_material == null) return;

        var points = line.Points;
        int pointCount = Math.Min(points.Length, MAX_CURVE_POINTS);
        
        // Create fixed size array and copy points
        var shaderPoints = new Vector2[MAX_CURVE_POINTS];
        for (int i = 0; i < pointCount; i++)
        {
            shaderPoints[i] = points[i];
        }
        // Fill remaining points with last point or zero
        for (int i = pointCount; i < MAX_CURVE_POINTS; i++)
        {
            shaderPoints[i] = pointCount > 0 ? points[^1] : Vector2.Zero;
        }

        _material.SetShaderParameter("curve_points", shaderPoints);
        _material.SetShaderParameter("point_count", pointCount);
    }

    public void ClearEffect()
    {
        if (_material == null) return;
        // Move token position far away to effectively hide the bulge
        _material.SetShaderParameter("token_position", new Vector2(-10000, -10000));
    }

    public void UpdateTokenPosition(Vector2 position)
    {
        if (_material == null) return;
        _material.SetShaderParameter("token_position", position);
    }
}
