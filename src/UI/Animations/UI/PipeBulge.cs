namespace F.UI.Animations;

public partial class PipeBulge : Node2D
{
    private const int MAX_CURVE_POINTS = 50;
    private Vector2 _endPoint;
    private ShaderMaterial? _material;
    private Line2D? _pipe;
    private float _progress;
    private float _speed = 1.0f;
    private Vector2 _startPoint;

    public override void _Ready()
    {
        base._Ready();
        _pipe = GetParent<Line2D>();
        if (_pipe == null)
        {
            GD.PrintErr("PipeVisuals node not found!");
            return;
        }

        InitializeShader();
        UpdatePoints();
    }

    private void InitializeShader()
    {
        var shader = GD.Load<Shader>("res://assets/shaders/ui/PipeBulge.gdshader");
        if (shader == null)
        {
            GD.PrintErr("Failed to load PipeBulge shader!");
            return;
        }

        _material = new ShaderMaterial
        {
            Shader = shader
        };

        // Set initial shader parameters
        _material.SetShaderParameter("bulge_amount", 1.5f);
        _material.SetShaderParameter("bulge_softness", 0.15f);
        _material.SetShaderParameter("token_position", Vector2.Zero);

        // Initialize curve points array with fixed size
        var emptyPoints = new Vector2[MAX_CURVE_POINTS];
        for (var i = 0; i < MAX_CURVE_POINTS; i++) emptyPoints[i] = Vector2.Zero;
        _material.SetShaderParameter("curve_points", emptyPoints);
        _material.SetShaderParameter("point_count", 0);

        if (_pipe != null) _pipe.Material = _material;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_pipe == null) return;

        _progress = (_progress + _speed * (float)delta) % 1.0f;
        Position = _startPoint.Lerp(_endPoint, _progress);

        // Update shader parameters
        if (_material != null) _material.SetShaderParameter("token_position", Position);
    }

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    public void UpdatePoints()
    {
        if (_pipe == null) return;

        if (_pipe.Points.Length >= 2)
        {
            _startPoint = _pipe.Points[0];
            _endPoint = _pipe.Points[^1];
        }

        if (_material != null)
        {
            // Update shader with current pipe points
            var points = new Vector2[MAX_CURVE_POINTS];
            var pointCount = Mathf.Min(_pipe.Points.Length, MAX_CURVE_POINTS);

            for (var i = 0; i < pointCount; i++) points[i] = _pipe.Points[i];

            _material.SetShaderParameter("curve_points", points);
            _material.SetShaderParameter("point_count", pointCount);
        }
    }

    public ShaderMaterial? GetShaderMaterial()
    {
        return _material;
    }
}