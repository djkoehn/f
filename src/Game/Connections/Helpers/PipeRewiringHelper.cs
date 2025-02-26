namespace F.Game.Connections.Helpers;

public static class PipeRewiringHelper
{
    // Rewires an existing connection by inserting a new block between source and target blocks.
    public static bool InsertBlockIntoPipe(IBlock newBlock, ConnectionPipe pipe, ConnectionFactory factory,
        ConnectionManager manager)
    {
        // First verify we have valid source and target blocks
        var sourceBlock = pipe.SourceBlock;
        var targetBlock = pipe.TargetBlock;

        GD.Print(
            $"[PipeRewiringHelper] Attempting to insert {newBlock.GetType().Name} between {sourceBlock?.GetType().Name} and {targetBlock?.GetType().Name}");

        if (sourceBlock == null || targetBlock == null)
        {
            GD.PrintErr("[PipeRewiringHelper] Source or target block is null");
            return false;
        }

        // Only verify that we're not trying to insert the same block instance
        if (newBlock == sourceBlock || newBlock == targetBlock)
        {
            GD.PrintErr("[PipeRewiringHelper] Cannot insert a block into its own connection");
            return false;
        }

        // Get the sockets from the existing pipe
        var (fromSocket, toSocket) = pipe.GetSockets();
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

        GD.Print("[PipeRewiringHelper] All validations passed, proceeding with pipe rewiring");

        // Store the original pipe's blocks before removing it
        var originalSourceBlock = pipe.SourceBlock;
        var originalTargetBlock = pipe.TargetBlock;

        // First, remove the original pipe from the manager's collections
        GD.Print(
            $"[PipeRewiringHelper] Removing original pipe between {originalSourceBlock.GetType().Name} and {originalTargetBlock.GetType().Name}");
        manager.RemoveConnection(pipe);

        // Then remove the pipe from the scene
        pipe.RemovePipe();

        // Create Source -> NewBlock pipe
        GD.Print($"[PipeRewiringHelper] Creating pipe: {sourceBlock.GetType().Name} -> {newBlock.GetType().Name}");
        var pipe1 = ConnectionFactory.CreatePipeForInsertion(sourceBlock, newBlock);
        if (pipe1 == null)
        {
            GD.PrintErr("[PipeRewiringHelper] Failed to create Source -> NewBlock pipe");
            // Recreate the original connection
            GD.Print("[PipeRewiringHelper] Attempting to restore original connection");
            var originalPipe = ConnectionFactory.CreatePipeForInsertion(sourceBlock, targetBlock);
            if (originalPipe != null)
            {
                manager.AddChild(originalPipe);
                manager.SetConnection(sourceBlock, originalPipe);
                manager.SetConnection(targetBlock, originalPipe);
                GD.Print("[PipeRewiringHelper] Successfully restored original connection");
            }
            else
            {
                GD.PrintErr("[PipeRewiringHelper] Failed to restore original connection!");
            }

            return false;
        }

        // Create NewBlock -> Target pipe
        GD.Print($"[PipeRewiringHelper] Creating pipe: {newBlock.GetType().Name} -> {targetBlock.GetType().Name}");
        var pipe2 = ConnectionFactory.CreatePipeForInsertion(newBlock, targetBlock);
        if (pipe2 == null)
        {
            GD.PrintErr("[PipeRewiringHelper] Failed to create NewBlock -> Target pipe");
            // Clean up the first pipe and recreate the original connection
            GD.Print("[PipeRewiringHelper] Cleaning up first pipe and attempting to restore original connection");
            pipe1.RemovePipe();
            var originalPipe = ConnectionFactory.CreatePipeForInsertion(sourceBlock, targetBlock);
            if (originalPipe != null)
            {
                manager.AddChild(originalPipe);
                manager.SetConnection(sourceBlock, originalPipe);
                manager.SetConnection(targetBlock, originalPipe);
                GD.Print("[PipeRewiringHelper] Successfully restored original connection");
            }
            else
            {
                GD.PrintErr("[PipeRewiringHelper] Failed to restore original connection!");
            }

            return false;
        }

        // Add both pipes to the scene
        GD.Print("[PipeRewiringHelper] Both pipes created successfully, adding to scene");
        manager.AddChild(pipe1);
        manager.AddChild(pipe2);

        // Set up connections in the manager
        manager.SetConnection(sourceBlock, pipe1); // Source -> pipe1
        manager.SetConnection(newBlock, pipe1); // NewBlock -> pipe1 (input)
        manager.SetConnection(newBlock, pipe2); // NewBlock -> pipe2 (output)
        manager.SetConnection(targetBlock, pipe2); // Target -> pipe2

        // Mark the new block as connected
        if (newBlock is BaseBlock nb)
        {
            nb.CompleteConnection();
            GD.Print(
                $"[PipeRewiringHelper] Successfully connected block {nb.Name} between {sourceBlock.GetType().Name} and {targetBlock.GetType().Name}");
        }

        return true;
    }
}