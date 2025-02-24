using F.Framework.Blocks;
using F.Framework.Connections;

namespace F.Game.Connections.Helpers
{
    public static class PipeRewiringHelper
    {
        // Rewires an existing connection by inserting a new block between source and target blocks.
        public static bool InsertBlockIntoPipe(IBlock newBlock, ConnectionPipe pipe, ConnectionFactory factory, ConnectionManager manager)
        {
            // First verify we have valid source and target blocks
            var sourceBlock = pipe.SourceBlock;
            var targetBlock = pipe.TargetBlock;

            GD.Print($"[PipeRewiringHelper] Attempting to insert {newBlock.GetType().Name} between {sourceBlock?.GetType().Name} and {targetBlock?.GetType().Name}");

            if (sourceBlock == null || targetBlock == null)
            {
                GD.PrintErr($"[PipeRewiringHelper] Source or target block is null");
                return false;
            }

            // Only verify that we're not trying to insert the same block instance
            if (newBlock == sourceBlock || newBlock == targetBlock)
            {
                GD.PrintErr($"[PipeRewiringHelper] Cannot insert a block into its own connection");
                return false;
            }

            // Get the sockets from the existing pipe
            (Node2D? fromSocket, Node2D? toSocket) = pipe.GetSockets();
            if (fromSocket == null || toSocket == null)
            {
                GD.PrintErr("Failed to get sockets from existing pipe");
                return false;
            }

            // Get the new block's sockets
            var newInput = newBlock.GetInputSocket() as Node2D;
            var newOutput = newBlock.GetOutputSocket() as Node2D;
            if (newInput == null || newOutput == null)
            {
                GD.PrintErr("New block is missing input or output socket");
                return false;
            }

            GD.Print($"[PipeRewiringHelper] All validations passed, proceeding with pipe rewiring");

            // Store the original pipe's blocks before removing it
            var originalSourceBlock = pipe.SourceBlock;
            var originalTargetBlock = pipe.TargetBlock;

            // First, remove the original pipe from the manager's collections
            GD.Print($"[PipeRewiringHelper] Removing original pipe between {originalSourceBlock.GetType().Name} and {originalTargetBlock.GetType().Name}");
            manager.RemoveConnection(pipe);

            // Then remove the pipe from the scene
            pipe.RemovePipe();

            // Create Source -> NewBlock pipe
            GD.Print($"[PipeRewiringHelper] Creating pipe: {sourceBlock.GetType().Name} -> {newBlock.GetType().Name}");
            var pipe1 = ConnectionFactory.CreatePipeForInsertion(sourceBlock, newBlock);
            if (pipe1 == null)
            {
                GD.PrintErr($"[PipeRewiringHelper] Failed to create pipe from {sourceBlock.GetType().Name} to {newBlock.GetType().Name}");
                RestoreConnection(sourceBlock, targetBlock, manager);
                return false;
            }

            // Create NewBlock -> Target pipe
            GD.Print($"[PipeRewiringHelper] Creating pipe: {newBlock.GetType().Name} -> {targetBlock.GetType().Name}");
            var pipe2 = ConnectionFactory.CreatePipeForInsertion(newBlock, targetBlock);
            if (pipe2 == null)
            {
                GD.PrintErr($"[PipeRewiringHelper] Failed to create pipe from {newBlock.GetType().Name} to {targetBlock.GetType().Name}");
                pipe1.QueueFree();
                RestoreConnection(sourceBlock, targetBlock, manager);
                return false;
            }

            // Add both pipes to the manager
            manager.AddPipe(pipe1);
            manager.AddPipe(pipe2);

            // Set up the connections for all blocks
            manager.SetConnection(sourceBlock, pipe1, true);
            manager.SetConnection(newBlock, pipe1, false);
            manager.SetConnection(newBlock, pipe2, true);
            manager.SetConnection(targetBlock, pipe2, false);

            // Set the block's state to connected
            if (newBlock is BaseBlock connectedBlock)
            {
                connectedBlock.CompleteConnection();
                GD.Print($"[PipeRewiringHelper] Successfully connected block {connectedBlock.Name} between {sourceBlock.GetType().Name} and {targetBlock.GetType().Name}");
            }

            return true;
        }

        private static void RestoreConnection(IBlock sourceBlock, IBlock targetBlock, ConnectionManager manager)
        {
            GD.Print($"[PipeRewiringHelper] Restoring connection between {sourceBlock.GetType().Name} and {targetBlock.GetType().Name}");
            var restoredPipe = ConnectionFactory.CreatePipeForInsertion(sourceBlock, targetBlock);
            if (restoredPipe != null)
            {
                manager.AddPipe(restoredPipe);
                manager.SetConnection(sourceBlock, restoredPipe, true);
                manager.SetConnection(targetBlock, restoredPipe, false);
            }
        }
    }
}