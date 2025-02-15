using Godot;
using F.Game.Core;

namespace F.Game.BlockLogic
{
    public class GameStateManager
    {
        private F.Game.Core.Inventory _inventory;

        public GameStateManager(F.Game.Core.Inventory inventory)
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