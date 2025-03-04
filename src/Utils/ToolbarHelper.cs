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
                // First, handle any existing connections
                var connectionManager = block.GetNode<ConnectionManager>("/root/Main/GameManager/BlockLayer");
                if (connectionManager != null)
                {
                    // Get all current connections before we disconnect
                    var connections = connectionManager.GetCurrentConnections(block);
                    if (connections.Count > 0)
                    {
                        // Find the source and target blocks that need to be reconnected
                        IBlock? sourceBlock = null;
                        IBlock? targetBlock = null;
                        
                        foreach (var pipe in connections)
                        {
                            if (pipe.SourceBlock == block && pipe.TargetBlock != null)
                            {
                                targetBlock = pipe.TargetBlock;
                            }
                            else if (pipe.TargetBlock == block && pipe.SourceBlock != null)
                            {
                                sourceBlock = pipe.SourceBlock;
                            }
                        }

                        // Disconnect the block first
                        connectionManager.DisconnectBlock(block);

                        // If we found both blocks, reconnect them
                        if (sourceBlock != null && targetBlock != null)
                        {
                            connectionManager.ConnectBlocks(sourceBlock, targetBlock);
                        }
                    }
                    else
                    {
                        // If no connections, just disconnect
                        connectionManager.DisconnectBlock(block);
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

                GD.Print($"ReturnBlockToToolbar: Block {block.Name} returned to toolbar.");
            }
            else
            {
                GD.PrintErr("ReturnBlockToToolbar: container is null.");
            }
        }
    }
}