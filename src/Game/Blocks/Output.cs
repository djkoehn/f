using Godot;

namespace F.Blocks;

public partial class Output : BaseBlock
{
    [Signal]
    public delegate void TokenProcessedEventHandler(float value);
    
    public override void _Ready()
    {
        base._Ready();
        GD.Print($"Output block initialized: {this.Name}");
    }

    public void ProcessValue(float value)
    {
        EmitSignal(SignalName.TokenProcessed, value);
    }
}