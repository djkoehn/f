using Godot;

namespace F;

public partial class Add : BaseBlock
{
    public override void _Ready()
    {
        base._Ready();
        // Initialization for Add block
    }

    public override void ProcessToken(Token token)
    {
        // Add-specific token processing logic
        token.Value += 1; // Example: Add 1 to the token value
    }
}
