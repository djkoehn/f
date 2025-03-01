using F.Game.Core;

namespace F.Game.BlockLogic;

public class GameStateManager
{
    private Inventory _inventory;

    public GameStateManager(Inventory inventory)
    {
        _inventory = inventory;
    }

    public void Initialize()
    {
        // Minimal initialization logic for game state
        GD.Print("GameStateManager initialized.");
    }
}