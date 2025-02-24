using F.Framework.Blocks;
using F.Framework.Core.Interfaces;
using F.Framework.Core.SceneTree;

namespace F.Framework.Core;

public partial class BlockLayer : Node2D, IBlockContainer, IBlockLayer
{
    private const float ROTATION_SPEED = 0.5f;
    private const float MAX_ROTATION = 0.2f;
    private float _currentRotation;
    private ShaderMaterial? _material;

    public BaseBlock Input => GetNode<BaseBlock>("Input") ?? throw new Exception("Input block not found");
    public BaseBlock Output => GetNode<BaseBlock>("Output") ?? throw new Exception("Output block not found");

    public void UpdateBlockPositions()
    {
        // In BlockLayer, blocks can be positioned anywhere, so we don't need to
        // enforce any specific positioning logic. Each block maintains its own position.
    }

    public Vector2 GetNextBlockPosition()
    {
        // In BlockLayer, blocks can be positioned anywhere, so we return the center
        // of the viewport as a reasonable default position
        var viewport = GetViewport();
        if (viewport != null)
        {
            var rect = viewport.GetVisibleRect();
            return rect.GetCenter();
        }

        return Vector2.Zero;
    }

    public override void _Ready()
    {
        // Load and apply the shader
        var shader = GD.Load<Shader>("res://src/UI/Shaders/UI/BlockLayerSkew.gdshader");
        _material = new ShaderMaterial
        {
            Shader = shader
        };

        // Set initial shader parameters
        _material.SetShaderParameter("fov", 90.0f);
        _material.SetShaderParameter("cull_back", true);
        _material.SetShaderParameter("y_rot", 0.0f);
        _material.SetShaderParameter("x_rot", 0.0f);
        _material.SetShaderParameter("inset", 0.1f);
        _material.SetShaderParameter("jpeg_quality", 10.0f);
        _material.SetShaderParameter("block_size", 3.0f);
        _material.SetShaderParameter("strobe_speed", 0.18f);

        // Apply material to this node
        Material = _material;
    }

    public override void _Process(double delta)
    {
        if (_material == null) return;

        // Update rotation for subtle animation
        _currentRotation = Mathf.Sin(Time.GetTicksMsec() / 1000.0f * ROTATION_SPEED) * MAX_ROTATION;
        _material.SetShaderParameter("y_rot", _currentRotation);
    }
}