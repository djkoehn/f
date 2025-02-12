using Godot;

namespace F.Blocks;

public partial class Input : BaseBlock
{
    private float _value = 1.0f;
    
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
        GD.Print($"Input block initialized: {this.Name}");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("spawn_token"))
        {
            GameManager.Instance?.TokenManager?.SpawnToken(this);
        }
    }
}