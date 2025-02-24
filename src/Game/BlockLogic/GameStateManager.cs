using F.Game.Core;
using F.Framework.Core;
using F.Framework.Core.SceneTree;

namespace F.Game.BlockLogic
{
    public class GameStateManager
    {
        private readonly IInventory _inventory;

        public GameStateManager(IInventory inventory)
        {
            _inventory = inventory;
        }

        public void Initialize()
        {
            // Minimal initialization logic for game state
            GD.Print("GameStateManager initialized.");
        }
    }
}