using Godot;

namespace F.Blocks;

public partial class Add : BaseBlock
{
    // Process for 0.3 seconds
    protected override float ProcessingDuration => 0.3f;

    protected override void OnProcessingComplete(Token token)
    {
        token.Value += 1;
        GD.Print($"Add block modified token value to: {token.Value}");
        base.OnProcessingComplete(token);
    }
}