using F.Framework.Input;

namespace F.Framework;

public partial class GameInitializer : Node
{
    public override void _Ready()
    {
        InputActions.ConfigureInputActions();
    }
}