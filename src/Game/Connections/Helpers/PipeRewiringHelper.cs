using Godot;
using F.Game.BlockLogic;

namespace F.Game.Connections.Helpers
{
    public static class PipeRewiringHelper
    {
        // Rewires an existing connection by inserting a new block between source and target blocks.
        public static bool InsertBlockIntoPipe(IBlock newBlock, ConnectionPipe pipe, ConnectionFactory factory, ConnectionManager manager)
        {
            // First verify we have valid source and target blocks (Input -> Output)
            var sourceBlock = pipe.SourceBlock;
            var targetBlock = pipe.TargetBlock;
            
            if (sourceBlock == null || targetBlock == null)
            {
                GD.PrintErr($"[PipeRewiringHelper] Source or target block is null");
                return false;
            }
            
            // Verify we have an Input->Output pipe
            bool isInputToOutput = sourceBlock.GetType().Name == "Input" && targetBlock.GetType().Name == "Output";
            if (!isInputToOutput)
            {
                GD.PrintErr($"[PipeRewiringHelper] Invalid pipe configuration - Source: {sourceBlock.GetType().Name}, Target: {targetBlock.GetType().Name}");
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

            // Remove the existing Input -> Output pipe
            manager.RemovePipe(pipe);

            // Create Input -> NewBlock pipe
            var pipe1 = ConnectionFactory.CreatePipeForInsertion(sourceBlock, newBlock);
            if (pipe1 == null || pipe1.SourceBlock == null || pipe1.TargetBlock == null)
            {
                GD.PrintErr("[PipeRewiringHelper] Failed to create Input -> NewBlock pipe");
                return false;
            }

            // Create NewBlock -> Output pipe
            var pipe2 = ConnectionFactory.CreatePipeForInsertion(newBlock, targetBlock);
            if (pipe2 == null || pipe2.SourceBlock == null || pipe2.TargetBlock == null)
            {
                GD.PrintErr("[PipeRewiringHelper] Failed to create NewBlock -> Output pipe");
                return false;
            }

            // Add both pipes to the manager
            manager.AddPipe(pipe1);
            manager.AddPipe(pipe2);

            // Set up connections in the manager
            manager.SetConnection(sourceBlock, pipe1);  // Input -> pipe1
            manager.SetConnection(newBlock, pipe1);     // NewBlock -> pipe1 (input)
            manager.SetConnection(newBlock, pipe2);     // NewBlock -> pipe2 (output)
            manager.SetConnection(targetBlock, pipe2);  // Output -> pipe2
            
            // Mark the new block as connected
            if (newBlock is BaseBlock nb)
            {
                nb.CompleteConnection();
                GD.Print($"[PipeRewiringHelper] Successfully connected block {nb.Name} between Input and Output");
            }
            
            return true;
        }
    }
} 