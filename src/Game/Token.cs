using Godot;

namespace F;

public partial class Token : Node2D
{
    [Export]
    public float Value
    {
        get => _value;
        set
        {
            _value = value;
            UpdateValueLabel();
        }
    }
    
    private float _value = 1;
    private Label _valueLabel = null!;
    private bool _isDragging = false;
    private Vector2 _dragOffset;
    
    public override void _Ready()
    {
        _valueLabel = GetNode<Label>("ValueLabel");
        UpdateValueLabel();
    }
    
    private void UpdateValueLabel()
    {
        if (_valueLabel != null)
        {
            _valueLabel.Text = _value.ToString("0.#");
        }
    }
    
    public void MoveTo(Vector2 position, bool animate = true)
    {
        if (animate)
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "position", position, 0.3f)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.Out);
        }
        else
        {
            Position = position;
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (mouseEvent.Pressed)
                {
                    var mousePos = GetGlobalMousePosition();
                    var tokenPos = GlobalPosition;
                    
                    if (mousePos.DistanceTo(tokenPos) < 32)
                    {
                        _isDragging = true;
                        _dragOffset = tokenPos - mousePos;
                    }
                }
                else
                {
                    _isDragging = false;
                }
            }
        }
    }
    
    public override void _Process(double delta)
    {
        if (_isDragging)
        {
            GlobalPosition = GetGlobalMousePosition() + _dragOffset;
        }
    }
}
