namespace F.Game.Core;

public sealed class GameStateManager
{
    private readonly Inventory _inventory;

    public GameStateManager(Inventory inventory)
    {
        _inventory = inventory;
    }

    public void Initialize()
    {
        _inventory.InventoryReady += OnInventoryReady;
    }

    private void OnInventoryReady()
    {
        GD.Print("Initializing game state from inventory");

        // Additional game state initialization can go here
    }
}