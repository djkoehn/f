using F.Game.Toolbar;
using F.Game.Core;
using F.Game.BlockLogic;
using F.Game.Connections;

namespace F.Utils
{
    public partial class ToolbarHelper : Node
    {
        public void ReturnBlockToToolbar(BaseBlock block)
        {
            GD.Print("[Debug ToolbarHelper] Entering ReturnBlockToToolbar for block: " + (string.IsNullOrEmpty(block.Name) ? block.GetName() : block.Name));
            
            // Retrieve the GameManager from the scene tree
            var gameManager = GetTree().Root.GetNodeOrNull<GameManager>("/root/Main/GameManager");
            if (gameManager == null)
            {
                GD.PrintErr("[Debug ToolbarHelper] GameManager not found in ToolbarHelper.");
                return;
            }
            
            GD.Print("[Debug ToolbarHelper] GameManager found: " + gameManager.Name);
            // Retrieve the toolbar container node; assumed path is 'Toolbar/BlockContainer' under GameManager
            var toolbar = gameManager.GetNodeOrNull<Node>("Toolbar/BlockContainer");
            if (toolbar == null)
            {
                GD.PrintErr("[Debug ToolbarHelper] ToolbarBlockContainer not found in GameManager.");
                return;
            }

            // Get connected blocks before disconnecting
            IBlock? inputBlock = null;
            IBlock? outputBlock = null;
            
            if (gameManager.ConnectionManager != null)
            {
                // Get all current connections for the block
                var connections = gameManager.ConnectionManager.GetCurrentConnections(block);
                foreach (var pipe in connections)
                {
                    if (pipe.SourceBlock == block)
                    {
                        outputBlock = pipe.TargetBlock; // The block we were outputting to
                        GD.Print($"[Debug ToolbarHelper] Found output connection to: {outputBlock.GetType().Name}");
                    }
                    else if (pipe.TargetBlock == block)
                    {
                        inputBlock = pipe.SourceBlock; // The block that was inputting to us
                        GD.Print($"[Debug ToolbarHelper] Found input connection from: {inputBlock.GetType().Name}");
                    }
                }

                GD.Print("[Debug ToolbarHelper] Disconnecting block from ConnectionManager");
                gameManager.ConnectionManager.DisconnectBlock(block);

                // If we found both input and output blocks, reconnect them
                if (inputBlock != null && outputBlock != null)
                {
                    GD.Print($"[Debug ToolbarHelper] Attempting to reconnect {inputBlock.GetType().Name} to {outputBlock.GetType().Name}");
                    // First reset their connection states
                    if (inputBlock is BaseBlock inBlock) inBlock.ResetConnections();
                    if (outputBlock is BaseBlock outBlock) outBlock.ResetConnections();

                    // Create new connection using CreatePipeForInsertion to bypass connection checks
                    var newPipe = ConnectionFactory.CreatePipeForInsertion(inputBlock, outputBlock);
                    if (newPipe != null)
                    {
                        gameManager.ConnectionManager.AddChild(newPipe);
                        gameManager.ConnectionManager.SetConnection(inputBlock, newPipe);
                        gameManager.ConnectionManager.SetConnection(outputBlock, newPipe);
                        GD.Print($"[Debug ToolbarHelper] Successfully reconnected {inputBlock.GetType().Name} to {outputBlock.GetType().Name}");
                    }
                    else
                    {
                        GD.PrintErr("[Debug ToolbarHelper] Failed to create new connection between remaining blocks");
                    }
                }
            }
            
            ReturnBlockToToolbar(block, toolbar);
        }

        // Updated static method to accept any Node type that can be a parent
        public static void ReturnBlockToToolbar(BaseBlock block, Node container)
        {
            GD.Print("[Debug ToolbarHelper] (Static) Entering ReturnBlockToToolbar for block: " + (string.IsNullOrEmpty(block.Name) ? block.GetName() : block.Name));
            GD.Print($"[Debug ToolbarHelper] Block state before return: {block.State}");
            
            // First ensure all connections are properly disconnected through the GameManager
            var gameManager = block.GetTree().Root.GetNodeOrNull<GameManager>("/root/Main/GameManager");
            if (gameManager?.ConnectionManager != null)
            {
                // Get connected blocks before disconnecting
                IBlock? inputBlock = null;
                IBlock? outputBlock = null;
                var connections = gameManager.ConnectionManager.GetCurrentConnections(block);
                
                foreach (var pipe in connections)
                {
                    if (pipe.SourceBlock == block)
                    {
                        outputBlock = pipe.TargetBlock; // The block we were outputting to
                    }
                    else if (pipe.TargetBlock == block)
                    {
                        inputBlock = pipe.SourceBlock; // The block that was inputting to us
                    }
                }

                GD.Print("[Debug ToolbarHelper] (Static) Disconnecting block from ConnectionManager");
                gameManager.ConnectionManager.DisconnectBlock(block);

                // If we found both input and output blocks, reconnect them
                if (inputBlock != null && outputBlock != null)
                {
                    GD.Print($"[Debug ToolbarHelper] (Static) Attempting to reconnect {inputBlock.GetType().Name} to {outputBlock.GetType().Name}");
                    gameManager.ConnectionManager.ConnectBlocks(inputBlock, outputBlock);
                }
            }

            if (block.GetParent() != null)
            {
                GD.Print("[Debug ToolbarHelper] (Static) Removing block from parent: " + block.GetParent().Name);
                block.GetParent().RemoveChild(block);
            }

            // Reset all connection states before adding to toolbar
            GD.Print($"[Debug ToolbarHelper] Resetting connections for block: {block.Name}");
            block.ResetConnections();
            
            // Set state to InToolbar before adding to container to ensure proper state
            block.SetInToolbar(true);
            
            container.AddChild(block);
            block.Position = Vector2.Zero;
            block.Scale = Vector2.One;
            block.Rotation = 0;
            block.SetDragging(false);
            block.SetPlaced(false);
            
            // Set the correct Z-index for toolbar blocks
            block.ZIndex = ZIndexConfig.Layers.ToolbarBlock;
            block.ZAsRelative = false;
            GD.Print($"[Debug ToolbarHelper] Set Z-index to {block.ZIndex} for toolbar block");
            
            // Call UpdateBlockPositions if the container implements IBlockContainer
            if (container is IBlockContainer blockContainer)
            {
                blockContainer.UpdateBlockPositions();
            }

            // Ensure block is in group 'Blocks'
            if (!block.IsInGroup("Blocks"))
                block.AddToGroup("Blocks");

            string blockNameStatic = block.Name;
            if (string.IsNullOrEmpty(blockNameStatic))
                blockNameStatic = block.GetName();
            GD.Print($"[Debug ToolbarHelper] Block state after return: {block.State}");
            GD.Print("[Debug ToolbarHelper] (Static) Finished processing, block " + blockNameStatic + " returned to toolbar via static helper.");
        }
    }
} 