using Godot;
using F.UI;

namespace F.Blocks;

public partial class Input : BaseBlock
{
    private float _value = 1.0f;
    private new Vector2 _originalPosition;

    public void SetValue(float value)
    {
        _value = value;
    }
    
    public float GetValue()
    {
        return _value;
    }

    public override void _Ready()
    {
        base._Ready();
        _originalPosition = Position;
        GD.Print($"Input block initialized: {this.Name}");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("spawn_token"))
        {
            var tween = CreateTween();
            // Move right by 10 pixels
            tween.TweenProperty(this, "position", _originalPosition + new Vector2(50, 0), 0.1f);
            // Move back to original position
            tween.TweenProperty(this, "position", _originalPosition, 0.2f);
            
            GameManager.Instance?.TokenManager?.SpawnToken(this);
        }
    }
}