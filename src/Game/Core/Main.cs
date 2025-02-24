using Godot;
using F.Framework.Core;

namespace F.Game.Core;

public partial class Main : GameManager
{
    public override void _Ready()
    {
        base._Ready();
        GD.Print("[Main] Initialized");
    }
}