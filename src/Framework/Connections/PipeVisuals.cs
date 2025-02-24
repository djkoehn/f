using F.UI.Animations;

namespace F.Framework.Connections;

public partial class PipeVisuals : Node2D
{
    private float _animationTime;
    private bool _isAnimating;
    private bool _isHovered;
    private Vector2[] _oldPoints = Array.Empty<Vector2>();
    private Line2D? _visualPipe;
    private Line2D? _bulgeEffect;
    private ShaderMaterial? _shaderMaterial;
    private float _tokenProgress = 0f;
    private bool _tokenActive = false;
    private float _shaderTime = 0f;

    private void SetShaderParameters(ShaderMaterial material)
    {
        // Set basic parameters
        material.SetShaderParameter("token_progress", _tokenProgress);
        material.SetShaderParameter("token_active", _tokenActive);
        material.SetShaderParameter("line_color", Colors.White);
        material.SetShaderParameter("base_width", PipeConfig.Visual.ShaderEffects.PipeWidth);
        material.SetShaderParameter("bulge_amount", PipeConfig.Visual.ShaderEffects.BulgeAmount);
        material.SetShaderParameter("bulge_softness", PipeConfig.Visual.ShaderEffects.BulgeSoftness);
        material.SetShaderParameter("TIME", _shaderTime);

        GD.Print("[PipeVisuals Debug] Set shader parameters:");
        GD.Print($"  token_progress: {_tokenProgress}");
        GD.Print($"  token_active: {_tokenActive}");
        GD.Print($"  bulge_amount: {PipeConfig.Visual.ShaderEffects.BulgeAmount}");
        GD.Print($"  bulge_softness: {PipeConfig.Visual.ShaderEffects.BulgeSoftness}");
        GD.Print($"  shader_time: {_shaderTime}");
    }

    public override void _Ready()
    {
        _visualPipe = GetNode<Line2D>("VisualPipe");
        _bulgeEffect = GetNode<Line2D>("BulgeEffect");

        if (_bulgeEffect != null)
        {
            _shaderMaterial = _bulgeEffect.Material as ShaderMaterial;
            if (_shaderMaterial != null)
            {
                GD.Print("[PipeVisuals Debug] Found existing shader material on BulgeEffect");
                SetShaderParameters(_shaderMaterial);

                // Verify shader is properly loaded
                if (_shaderMaterial.Shader == null)
                {
                    GD.PrintErr("[PipeVisuals Debug] Shader is null on material!");
                }
                else
                {
                    GD.Print("[PipeVisuals Debug] Shader is properly loaded");
                }
            }
            else
            {
                GD.PrintErr("[PipeVisuals Debug] No shader material found on BulgeEffect!");
            }
        }
        else
        {
            GD.PrintErr("[PipeVisuals Debug] BulgeEffect node not found!");
        }

        InitializeVisualPipe();
    }

    private void InitializeVisualPipe()
    {
        if (_visualPipe == null || _bulgeEffect == null)
        {
            GD.PrintErr("[PipeVisuals Debug] Cannot initialize null VisualPipe or BulgeEffect!");
            return;
        }

        _visualPipe.Width = PipeConfig.Visual.ShaderEffects.PipeWidth;
        _visualPipe.DefaultColor = PipeConfig.Visual.LineColor;
        _visualPipe.JointMode = Line2D.LineJointMode.Round;
        _visualPipe.BeginCapMode = Line2D.LineCapMode.Round;
        _visualPipe.EndCapMode = Line2D.LineCapMode.Round;
        _visualPipe.Antialiased = true;

        _bulgeEffect.Width = PipeConfig.Visual.ShaderEffects.PipeWidth * 2;  // Base width for shader
        _bulgeEffect.DefaultColor = Colors.White;
        _bulgeEffect.JointMode = Line2D.LineJointMode.Round;
        _bulgeEffect.BeginCapMode = Line2D.LineCapMode.Round;
        _bulgeEffect.EndCapMode = Line2D.LineCapMode.Round;
        _bulgeEffect.TextureMode = Line2D.LineTextureMode.Stretch;
        _bulgeEffect.TextureRepeat = Line2D.TextureRepeatEnum.Enabled;
        _bulgeEffect.Antialiased = true;

        GD.Print("[PipeVisuals Debug] VisualPipe and BulgeEffect initialized");
        GD.Print($"  VisualPipe width: {_visualPipe.Width}");
        GD.Print($"  BulgeEffect width: {_bulgeEffect.Width}");
    }

    public void UpdateVisuals(Vector2 startPoint, Vector2 endPoint)
    {
        if (_visualPipe == null || _bulgeEffect == null) return;

        // Update both lines with the same points
        _visualPipe.ClearPoints();
        _bulgeEffect.ClearPoints();

        // Add the points
        _visualPipe.AddPoint(startPoint);
        _visualPipe.AddPoint(endPoint);
        _bulgeEffect.AddPoint(startPoint);
        _bulgeEffect.AddPoint(endPoint);

        // Update shader parameters
        if (_shaderMaterial != null)
        {
            SetShaderParameters(_shaderMaterial);
        }
    }

    public void SetHovered(bool isHovered)
    {
        _isHovered = isHovered;
        if (_visualPipe != null)
            _visualPipe.DefaultColor = _isHovered ? PipeConfig.Visual.HoverColor : PipeConfig.Visual.LineColor;
    }

    public void StartReconnectAnimation(Vector2[] oldPoints)
    {
        if (_visualPipe == null || _bulgeEffect == null) return;

        _isAnimating = true;
        _animationTime = 0;
        _oldPoints = oldPoints;
    }

    public void ClearBulgeEffect()
    {
        if (_shaderMaterial != null)
        {
            _tokenActive = false;
            _tokenProgress = 0f;
            SetShaderParameters(_shaderMaterial);
            GD.Print("[PipeVisuals Debug] Cleared bulge effect");
        }
    }

    public void UpdateTokenPosition(Vector2 position)
    {
        if (_visualPipe == null || _bulgeEffect == null)
        {
            GD.PrintErr("[PipeVisuals Debug] VisualPipe or BulgeEffect is null!");
            return;
        }

        // Calculate progress along the line
        if (_visualPipe.Points.Length >= 2)
        {
            Vector2 startPoint = _visualPipe.Points[0];
            Vector2 endPoint = _visualPipe.Points[^1];

            // Project token position onto the line from start to end
            Vector2 lineDir = (endPoint - startPoint).Normalized();
            Vector2 toToken = ToLocal(position) - startPoint;
            float projection = toToken.Dot(lineDir);
            float totalLength = (endPoint - startPoint).Length();

            // Calculate progress as ratio of projection to total length
            _tokenProgress = Mathf.Clamp(projection / totalLength, 0, 1);

            // Update shader parameters
            if (_shaderMaterial != null)
            {
                SetShaderParameters(_shaderMaterial);
            }
        }
    }

    public void StartTokenMovement()
    {
        if (_shaderMaterial != null)
        {
            _tokenActive = true;
            _tokenProgress = 0f;
            SetShaderParameters(_shaderMaterial);
            GD.Print("[PipeVisuals Debug] Started token movement");
        }
    }

    public void EndTokenMovement()
    {
        if (_shaderMaterial != null)
        {
            _tokenActive = false;
            SetShaderParameters(_shaderMaterial);
            GD.Print("[PipeVisuals Debug] Ended token movement");
        }
    }

    public Vector2 GetPositionAlongCurve(float t)
    {
        if (_visualPipe == null || _visualPipe.Points.Length < 2) return Vector2.Zero;
        return _visualPipe.Points[0].Lerp(_visualPipe.Points[1], t);
    }

    public override void _Process(double delta)
    {
        // Update shader time
        _shaderTime += (float)delta;

        if (_shaderMaterial != null && _tokenActive)
        {
            SetShaderParameters(_shaderMaterial);
        }

        if (!_isAnimating || _visualPipe == null || _bulgeEffect == null) return;

        _animationTime += (float)delta;
        var t = Mathf.Min(_animationTime / PipeConfig.Animation.ReconnectDuration, 1.0f);

        if (t >= 1.0f)
        {
            _isAnimating = false;
            return;
        }

        // Update points based on animation progress
        _visualPipe.ClearPoints();
        _bulgeEffect.ClearPoints();
        for (var i = 0; i < _oldPoints.Length; i++)
        {
            var point = _oldPoints[i];
            var targetPoint = i < _visualPipe.Points.Length ? _visualPipe.Points[i] : _visualPipe.Points[^1];
            var interpolatedPoint = point.Lerp(targetPoint, t);
            _visualPipe.AddPoint(interpolatedPoint);
            _bulgeEffect.AddPoint(interpolatedPoint);
        }
    }
}