using F.Game.Connections;
using F.Game.Core;

namespace F.Utils
{
    public static class ConnectionHelper
    {
        // Now a static method that requires a Node context to obtain the GameManager
        public static bool TryConnectBlock(Node context, IBlock block, Vector2 position)
        {
            var gameManager = context.GetTree().Root.GetNode<GameManager>($"/root/Main/GameManager");
            if (gameManager?.ConnectionManager == null)
            {
                GD.PrintErr("ConnectionManager not found in GameManager.");
                return false;
            }
            bool connected = gameManager.ConnectionManager.HandleBlockConnection(block, position);
            if (connected && block is BaseBlock baseBlock)
            {
                baseBlock.CompleteConnection();
                GD.Print($"[ConnectionHelper Debug] Forced block {baseBlock.Name} to Connected state. Connection operation returned: {connected}");
            }
            return connected;
        }

        // Static method for highlighting a pipe at a given position using a Node context
        public static void HighlightPipeAtPosition(Node context, Vector2 position)
        {
            GD.Print($"[ConnectionHelper] HighlightPipeAtPosition called with position: {position}");
            var gameManager = context.GetTree().Root.GetNode<GameManager>("/root/Main/GameManager");
            if (gameManager == null)
            {
                GD.PrintErr($"[ConnectionHelper] GameManager not found in scene tree using context: {context.Name}");
                return;
            }
            else
            {
                GD.Print($"[ConnectionHelper] GameManager found: {gameManager.Name}");
            }
            if (gameManager.ConnectionManager == null)
            {
                GD.PrintErr($"[ConnectionHelper] ConnectionManager is null in GameManager: {gameManager.Name}");
                return;
            }
            var pipe = gameManager.ConnectionManager.GetPipeAtPosition(position);
            if (pipe != null)
            {
                GD.Print($"[ConnectionHelper] Pipe found at position {position}. Highlighting pipe.");
                pipe.SetInsertionHighlight(true);
            }
            else
            {
                GD.Print($"[ConnectionHelper] No pipe found at position: {position}");
            }
        }

        // Static helper for cubic Bezier interpolation remains unchanged
        public static Vector2 CubicBezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
        {
            float oneMinusT = 1 - t;
            return a * (oneMinusT * oneMinusT * oneMinusT) +
                   b * (3 * t * oneMinusT * oneMinusT) +
                   c * (3 * t * t * oneMinusT) +
                   d * (t * t * t);
        }

        public static bool HasOutputConnection(IBlock block)
        {
            // Check if the Output block has a connection
            var parent = block as Node;
            if (parent == null)
            {
                GD.PrintErr("IBlock is not a Node, cannot get parent.");
                return false;
            }
            var gameManager = parent.GetTree().Root.GetNode<GameManager>($"/root/Main/GameManager");
            if (gameManager?.ConnectionManager == null)
            {
                GD.PrintErr("ConnectionManager not found in GameManager.");
                return false;
            }
            return gameManager.ConnectionManager.IsBlockConnected(block);
        }

        public static bool HasInputConnection(IBlock block)
        {
            // Check if the Input block has a connection
            var parent = block as Node;
            if (parent == null)
            {
                GD.PrintErr("IBlock is not a Node, cannot get parent.");
                return false;
            }
            var gameManager = parent.GetTree().Root.GetNode<GameManager>($"/root/Main/GameManager");
            if (gameManager?.ConnectionManager == null)
            {
                GD.PrintErr("ConnectionManager not found in GameManager.");
                return false;
            }
            return gameManager.ConnectionManager.IsBlockConnected(block);
        }

    }
} 