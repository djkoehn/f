namespace F.Game.Core;

public partial class BlockLayerSkewControl : Sprite2D
{
    private const float MAX_ROTATION = 5.0f; // Maximum rotation in degrees
    private ShaderMaterial _material;

    public override void _Ready()
    {
        // 'this' is the Sprite2D itself
        _material = Material as ShaderMaterial;
        if (_material == null) GD.PrintErr("Sprite does not have a ShaderMaterial assigned");
    }

    public override void _Process(double delta)
    {
        if (_material == null)
            return;

        // Get mouse position in the local coordinate space of this Sprite2D
        var localMouse = GetLocalMousePosition();

        // Use the texture's size if available, otherwise use a fallback
        var texSize = Texture != null ? Texture.GetSize() : new Vector2(1920, 1080);

        // Normalize mouse position
        var norm = localMouse / texSize;

        // Update shader parameters
        _material.SetShaderParameter("y_rot", norm.X * MAX_ROTATION);
        _material.SetShaderParameter("x_rot", norm.Y * MAX_ROTATION);
    }
}