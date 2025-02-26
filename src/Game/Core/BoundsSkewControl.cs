namespace F.Game.Core;

public partial class BoundsSkewControl : Sprite2D
{
    private const float MAX_ROTATION = 5.0f; // Maximum rotation in degrees
    private Area2D _bounds;
    private ShaderMaterial _material;

    public override void _Ready()
    {
        _bounds = GetParent<Area2D>();
        _material = _bounds.Material as ShaderMaterial;
    }

    public override void _Process(double delta)
    {
        if (_bounds == null || _material == null) return;

        var mousePos = _bounds.GetLocalMousePosition();
        var boundsSize = new Vector2(1250, 800); // From CollisionShape2D size
        var normalizedPos = new Vector2(
            mousePos.X / boundsSize.X,
            mousePos.Y / boundsSize.Y
        );

        // Apply rotation based on mouse position
        _material.SetShaderParameter("y_rot", normalizedPos.X * MAX_ROTATION);
        _material.SetShaderParameter("x_rot", normalizedPos.Y * MAX_ROTATION);
    }
}