using F.Game.Toolbar;
using F.Game.Core;

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
            var toolbar = gameManager.GetNodeOrNull<ToolbarBlockContainer>("Toolbar/BlockContainer");
            if (toolbar == null)
            {
                GD.PrintErr("[Debug ToolbarHelper] ToolbarBlockContainer not found in GameManager.");
                return;
            }
            
            GD.Print("[Debug ToolbarHelper] ToolbarBlockContainer found: " + toolbar.Name);
            // Remove block from its current parent if any
            if (block.GetParent() != null)
            {
                GD.Print("[Debug ToolbarHelper] Removing block from parent: " + block.GetParent().Name);
                block.GetParent().RemoveChild(block);
            }
            else { GD.Print("[Debug ToolbarHelper] Block had no parent."); }
            
            // Add the block to the toolbar using the AddBlock method so that it gets repositioned
            GD.Print("[Debug ToolbarHelper] Adding block to toolbar container using toolbar.AddBlock.");
            toolbar.AddBlock(block);
            
            // Reset block properties
            block.Position = Vector2.Zero;
            block.Scale = Vector2.One;
            block.Rotation = 0;
            block.SetDragging(false);
            block.SetPlaced(false);

            // Ensure block is in group 'Blocks'
            if (!block.IsInGroup("Blocks"))
                block.AddToGroup("Blocks");

            string blockName = block.Name;
            if (string.IsNullOrEmpty(blockName))
                blockName = block.GetName();
            GD.Print("[Debug ToolbarHelper] Finished processing, block " + blockName + " returned to toolbar.");
        }

        // New static method for reparenting a block using a given ToolbarBlockContainer
        public static void ReturnBlockToToolbar(BaseBlock block, F.Game.Toolbar.ToolbarBlockContainer container)
        {
            GD.Print("[Debug ToolbarHelper] (Static) Entering ReturnBlockToToolbar for block: " + (string.IsNullOrEmpty(block.Name) ? block.GetName() : block.Name));
            
            if (block.GetParent() != null)
            {
                GD.Print("[Debug ToolbarHelper] (Static) Removing block from parent: " + block.GetParent().Name);
                block.GetParent().RemoveChild(block);
            }
            container.AddChild(block);
            block.Position = Vector2.Zero;
            block.Scale = Vector2.One;
            block.Rotation = 0;
            block.SetDragging(false);
            block.SetPlaced(false);
            container.UpdateBlockPositions();

            // Ensure block is in group 'Blocks'
            if (!block.IsInGroup("Blocks"))
                block.AddToGroup("Blocks");

            string blockNameStatic = block.Name;
            if (string.IsNullOrEmpty(blockNameStatic))
                blockNameStatic = block.GetName();
            GD.Print("[Debug ToolbarHelper] (Static) Finished processing, block " + blockNameStatic + " returned to toolbar via static helper.");
        }
    }
} 