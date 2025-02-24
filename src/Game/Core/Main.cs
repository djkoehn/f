using F.Framework.Logging;

namespace F.Game.Core;

public partial class Main : Framework.Core.GameManager
{
    public override void _Ready()
    {
        base._Ready();
        Logger.Game.Print("Initialized");
    }
}