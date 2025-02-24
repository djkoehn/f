using F.Framework.Core.Interfaces;

namespace F.Game.BlockLogic;

public class GameStateManager
{
    private readonly IInventory _inventory;

    public GameStateManager(IInventory inventory)
    {
        _inventory = inventory;
    }

    public void Initialize()
    {
        // Initialize game state
    }
}