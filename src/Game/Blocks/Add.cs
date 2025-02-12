using Godot;

namespace F.Blocks;

public partial class Add : BaseBlock
{
    public override void ProcessToken(Token token)
    {
        GD.Print($"Add block processing token with value {token.Value}");
        token.Value++;
        GD.Print($"Token value after processing: {token.Value}");

        // Finish processing after incrementing the token value
        FinishProcessing(token);
    }
}