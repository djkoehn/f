using F.Framework.Blocks;
using F.Framework.Connections;
using F.Framework.Logging;

namespace F.Game.Connections.Helpers;

public static class PipeRewiringHelper
{
    // Rewires an existing connection by inserting a new block between source and target blocks.
    public static bool RewirePipe(ConnectionPipe pipe, IBlock newBlock, ConnectionManager manager)
    {
        var originalSourceBlock = pipe.SourceBlock;
        var originalTargetBlock = pipe.TargetBlock;

        if (originalSourceBlock == null || originalTargetBlock == null)
        {
            Logger.Connection.Err("Cannot rewire pipe with null blocks");
            return false;
        }

        // Determine if we're inserting before or after the original source block
        var sourceBlock = originalSourceBlock;
        var targetBlock = originalTargetBlock;

        Logger.Connection.Print(
            $"Removing original pipe between {originalSourceBlock.GetType().Name} and {originalTargetBlock.GetType().Name}");
        manager.RemoveConnection(pipe);

        // Then remove the pipe from the scene
        pipe.RemovePipe();

        // Create Source -> NewBlock pipe
        Logger.Connection.Print($"Creating pipe: {sourceBlock.GetType().Name} -> {newBlock.GetType().Name}");
        var pipe1 = ConnectionFactory.CreatePipeForInsertion(sourceBlock, newBlock);
        if (pipe1 == null)
        {
            Logger.Connection.Err(
                $"Failed to create pipe from {sourceBlock.GetType().Name} to {newBlock.GetType().Name}");
            RestoreConnection(sourceBlock, targetBlock, manager);
            return false;
        }

        // Create NewBlock -> Target pipe
        Logger.Connection.Print($"Creating pipe: {newBlock.GetType().Name} -> {targetBlock.GetType().Name}");
        var pipe2 = ConnectionFactory.CreatePipeForInsertion(newBlock, targetBlock);
        if (pipe2 == null)
        {
            Logger.Connection.Err(
                $"Failed to create pipe from {newBlock.GetType().Name} to {targetBlock.GetType().Name}");
            pipe1.QueueFree();
            RestoreConnection(sourceBlock, targetBlock, manager);
            return false;
        }

        // Add both pipes to the manager
        manager.AddPipe(pipe1);
        manager.AddPipe(pipe2);

        // Set up the connections for all blocks
        manager.SetConnection(sourceBlock, pipe1);
        manager.SetConnection(newBlock, pipe1, false);
        manager.SetConnection(newBlock, pipe2);
        manager.SetConnection(targetBlock, pipe2, false);

        // Set the block's state to connected
        if (newBlock is BaseBlock connectedBlock)
        {
            connectedBlock.CompleteConnection();
            Logger.Connection.Print(
                $"Successfully connected block {connectedBlock.Name} between {sourceBlock.GetType().Name} and {targetBlock.GetType().Name}");
        }

        return true;
    }

    private static void RestoreConnection(IBlock sourceBlock, IBlock targetBlock, ConnectionManager manager)
    {
        Logger.Connection.Print(
            $"Restoring connection between {sourceBlock.GetType().Name} and {targetBlock.GetType().Name}");
        var restoredPipe = ConnectionFactory.CreatePipeForInsertion(sourceBlock, targetBlock);
        if (restoredPipe != null)
        {
            manager.AddPipe(restoredPipe);
            manager.SetConnection(sourceBlock, restoredPipe);
            manager.SetConnection(targetBlock, restoredPipe, false);
        }
    }
}