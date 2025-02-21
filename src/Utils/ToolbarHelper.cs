using Godot;
using F.Game.BlockLogic;
using F.Game.Connections;

namespace F.Utils
{
    // ToolbarHelper is now an instance class derived from Node
    public partial class ToolbarHelper : Node
    {
        public void ReturnBlockToToolbar(BaseBlock block, Node container)
        {
            if (block == null)
            {
                GD.PrintErr("ReturnBlockToToolbar: block is null.");
                return;
            }
            if (block.GetTree() == null)
            {
                GD.PrintErr("ReturnBlockToToolbar: block is not in the scene tree.");
                return;
            }

            if (container != null)
            {
                string blockName = block.Name ?? "unknown";
                GD.Print($"[ToolbarHelper] Returning block {blockName} to toolbar");
                
                // First, handle any existing connections
                var connectionManager = block.GetNode<ConnectionManager>("/root/Main/GameManager/BlockLayer");
                if (connectionManager != null)
                {
                    // Get all current connections before we disconnect
                    var connections = connectionManager.GetCurrentConnections(block);
                    
                    // Find the blocks that were connected to this block
                    IBlock? sourceBlock = null;
                    IBlock? targetBlock = null;
                    
                    foreach (var pipe in connections)
                    {
                        if (pipe.SourceBlock == block)
                        {
                            // If this block is the source, then its target is our target
                            targetBlock = pipe.TargetBlock;
                        }
                        if (pipe.TargetBlock == block)
                        {
                            // If this block is the target, then its source is our source
                            sourceBlock = pipe.SourceBlock;
                        }
                    }

                    // Store the blocks we found
                    var savedSourceBlock = sourceBlock;
                    var savedTargetBlock = targetBlock;

                    // Disconnect only the block being returned to toolbar
                    connectionManager.DisconnectBlock(block);

                    // If we found both a source and target block, reconnect them
                    if (savedSourceBlock != null && savedTargetBlock != null)
                    {
                        GD.Print($"[ToolbarHelper] Reconnecting {savedSourceBlock.Name ?? "unknown"} to {savedTargetBlock.Name ?? "unknown"}");
                        // Create new connection between the previously connected blocks
                        var newPipe = ConnectionFactory.CreatePipeForInsertion(savedSourceBlock, savedTargetBlock);
                        if (newPipe != null)
                        {
                            // Add the pipe to both lists and the scene
                            connectionManager.AddPipe(newPipe);
                            
                            // Set up the connections for both blocks, maintaining source/target relationship
                            connectionManager.SetConnection(savedSourceBlock, newPipe, true); // true = is source
                            connectionManager.SetConnection(savedTargetBlock, newPipe, false); // false = is target
                            
                            // Manually trigger a connection complete state for both blocks
                            if (savedSourceBlock is BaseBlock sb) sb.CompleteConnection();
                            if (savedTargetBlock is BaseBlock tb) tb.CompleteConnection();
                        }
                    }
                }
                
                // Reset the block's connection state
                block.ResetConnections();
                
                if (block.GetParent() != null)
                {
                    block.GetParent().RemoveChild(block);
                }
                container.AddChild(block);
                block.ZIndex = ZIndexConfig.Layers.ToolbarBlock;

                // If container is a ToolbarBlockContainer, update positions
                if (container is F.Game.Toolbar.ToolbarBlockContainer toolbarContainer)
                {
                    toolbarContainer.UpdateBlockPositions();
                    toolbarContainer.UpdateContainerSize();
                }

                GD.Print($"[ToolbarHelper] Block {blockName} returned to toolbar successfully");
            }
            else
            {
                GD.PrintErr("ReturnBlockToToolbar: container is null.");
            }
        }
    }
}