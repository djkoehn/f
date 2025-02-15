using Godot;
using F.Game.BlockLogic;
using F.Utils.Helpers;

namespace F.Utils
{
    public partial class InputHelper : Node
    {
        private BlockInteractionManager? _blockManager;
        private IDragService? _dragHelper;
        private ToolbarHelper? _toolbarHelper;
        private BaseBlock? _currentDraggedBlock;

        public override void _Ready()
        {
            _blockManager = GetNode<BlockInteractionManager>("/root/Main/GameManager/BlockInteractionManager");

            // Retrieve other helpers from the central HelperFunnel
            var hf = HelperFunnel.GetInstance();
            _toolbarHelper = hf.GetNodeOrNull<ToolbarHelper>("ToolbarHelper");
            _dragHelper = hf?.DragHelper;

            if (_dragHelper == null)
            {
                _dragHelper = F.Utils.Helpers.DragHelper.Instance;
                if (_dragHelper == null)
                    GD.PrintErr("[Debug InputHelper] DragHelper is STILL null after fallback!");
                else
                    GD.Print("[Debug InputHelper] Fallback: Using DragHelper.Instance.");
            }

        }

        public override void _Input(InputEvent @event)
        {
            if (_blockManager == null) return;
            
            if (@event is InputEventMouseButton mouseEvent)
            {
                if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
                {
                    HandleRightClick(mouseEvent);
                }
                else if (mouseEvent.ButtonIndex == MouseButton.Left)
                {
                    if (mouseEvent.Pressed)
                    {
                        HandleLeftClick(mouseEvent);
                    }
                    else // Released
                    {
                        HandleLeftRelease(mouseEvent);
                    }
                }
            }
            else if (@event is InputEventMouseMotion mouseMotion)
            {
                HandleMouseMotion(mouseMotion);
            }

            // Disable connection attempts while dragging.
            // If a block is in dragging state, do not attempt any connection.
            // The following code that would normally check for connections is disabled:
            /*
            if (block.IsDragging) {
                 // Connection attempt disabled
                 // GD.Print("Connection attempt is disabled while dragging.");
            }
            */
        }

        private void HandleRightClick(InputEventMouseButton mouseEvent)
        {
            var block = _blockManager!.GetBlockAtPosition(mouseEvent.GlobalPosition);
            if (block != null)
            {
                string blockName = ((IBlock)block).Name;
                if (string.IsNullOrEmpty(blockName))
                    blockName = block.GetName();

                // If the block is currently being dragged, first end the drag
                if (block.State == BlockState.Dragging)
                {
                    GD.Print("[Debug InputHelper] Block '" + blockName + "' is dragging; ending drag before returning to toolbar.");
                    _dragHelper?.EndDrag(block);
                    _currentDraggedBlock = null;
                }

                GD.Print("[Debug InputHelper] Right-click on block: '" + blockName + "'. Returning to toolbar.");
                _toolbarHelper?.ReturnBlockToToolbar(block);

                block.SetInToolbar(true);
                GetViewport().SetInputAsHandled();
            }
        }

        private void HandleLeftRelease(InputEventMouseButton mouseEvent)
        {
            if (_currentDraggedBlock == null) return;

            string blockName = ((IBlock)_currentDraggedBlock).Name;
            if (string.IsNullOrEmpty(blockName))
                blockName = _currentDraggedBlock.GetName();

            GD.Print($"[Debug InputHelper] Block {blockName} released at {mouseEvent.GlobalPosition}");
            
            // Try to connect the block at its current position
            Vector2 effectivePos = _currentDraggedBlock.GetTokenPosition();
            GD.Print($"[Debug InputHelper] Attempting to connect block at position: {effectivePos}");
            bool connected = ConnectionHelper.TryConnectBlock(this, _currentDraggedBlock, effectivePos);
            
            if (connected)
            {
                GD.Print($"[Debug InputHelper] Successfully connected block {blockName}");
            }
            else
            {
                GD.Print($"[Debug InputHelper] Failed to connect block {blockName}");
                _currentDraggedBlock.SetPlaced(true);
                GD.Print($"[Debug InputHelper] Block {blockName} reverted to Placed state");
            }

            _dragHelper?.EndDrag(_currentDraggedBlock);
            _currentDraggedBlock = null;
            GetViewport().SetInputAsHandled();
        }

        private void HandleLeftClick(InputEventMouseButton mouseEvent)
        {
            var block = _blockManager!.GetBlockAtPosition(mouseEvent.GlobalPosition);
            if (block == null)
            {
                GD.Print("[Debug InputHelper] No block found at position: " + mouseEvent.GlobalPosition);
                return;
            }

            string blockName = ((BaseBlock)block).Name;
            if (string.IsNullOrEmpty(blockName))
                blockName = block.GetName();

            // If the block's parent is a ToolbarBlockContainer, reparent it to the BlockLayer
            if (block.GetParent() is F.Game.Toolbar.ToolbarBlockContainer container)
            {
                GD.Print("[Debug InputHelper] Block '" + blockName + "' is in the toolbar. Attempting to reparent to BlockLayer before dragging.");

                // Locate BlockLayer using the direct path from BlockConfig
                var blockLayer = GetTree().Root.GetNodeOrNull<Node2D>(F.Config.BlockConfig.BlockLayerPath);
                if (blockLayer == null)
                {
                    // Fallback: try using the current scene
                    var currentScene = GetTree().CurrentScene;
                    if (currentScene != null)
                        blockLayer = currentScene.GetNodeOrNull<Node2D>("GameManager/BlockLayer");
                }

                if (blockLayer != null)
                {
                    // Record the block's global position to preserve its location after reparenting
                    Vector2 globalPos = block.GlobalPosition;
                    if (block.GetParent() is Control ctrl) {
                        globalPos = ctrl.GlobalPosition + block.Position;
                    }
                    container.RemoveChild(block);
                    blockLayer.AddChild(block);
                    block.GlobalPosition = globalPos;
                    GD.Print("[Debug InputHelper] Block reparented successfully to BlockLayer; new parent: " + block.GetParent()?.Name);
                }
                else
                {
                    GD.PrintErr("[Debug InputHelper] Could not locate BlockLayer; not reparenting block to avoid deletion.");
                    GetViewport().SetInputAsHandled();
                    return;
                }

                block.SetInToolbar(false);
            }

            // Start dragging the block
            GD.Print("[Debug InputHelper] Starting drag for block '" + blockName + "' at " + mouseEvent.GlobalPosition);
            _dragHelper?.StartDrag(block, mouseEvent.GlobalPosition);
            _currentDraggedBlock = block;
            GetViewport().SetInputAsHandled();
        }

        private void HandleMouseMotion(InputEventMouseMotion mouseEvent)
        {
            if (_currentDraggedBlock != null)
            {
                _dragHelper?.UpdateDrag(_currentDraggedBlock, mouseEvent.GlobalPosition);
                
                // Highlight pipe at current position if we're dragging a block
                Vector2 effectivePos = _currentDraggedBlock.GetTokenPosition();
                GD.Print($"[Debug InputHelper] Checking for pipe at token position: {effectivePos}");
                ConnectionHelper.HighlightPipeAtPosition(this, effectivePos);
            }
        }
    }
} 