using F.Framework.Logging;
using Godot;

namespace F.UI.Controls;

public partial class BlockLayerSkewControl : Sprite2D
{
    private const float MAX_ROTATION = 5.0f; // Maximum rotation in degrees
    private ShaderMaterial? _material;

    public override void _Ready()
    {
        base._Ready();
        _material = Material as ShaderMaterial;
        if (_material == null) Logger.UI.Err("Sprite does not have a ShaderMaterial assigned");
    }

    public override void _Process(double delta)
    {
        if (_material == null) return;

        var mousePos = GetViewport().GetMousePosition();
        var viewportSize = GetViewport().GetVisibleRect().Size;
        var normalizedX = mousePos.X / viewportSize.X;

        _material.SetShaderParameter("skew_amount", (normalizedX - 0.5f) * 0.1f);
    }
}