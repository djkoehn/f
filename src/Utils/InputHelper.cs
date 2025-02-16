namespace F.Utils
{
    public partial class InputHelper : Node
    {
        private BlockInteractionManager? _blockManager;
        private DragHelper? _dragHelper;
        private ToolbarHelper? _toolbarHelper;
        private BaseBlock? _currentDraggedBlock;

        public override void _Ready()
        {
            _blockManager = GetNode<BlockInteractionManager>("/root/Main/GameManager/BlockInteractionManager");

            // Retrieve other helpers from the central HelperFunnel
            var hf = HelperFunnel.GetInstance();
            _toolbarHelper = hf.GetNodeOrNull<ToolbarHelper>("ToolbarHelper");
            _dragHelper = hf?.GetNodeOrNull<DragHelper>("DragHelper");

            if (_dragHelper == null)
            {
                _dragHelper = DragHelper.Instance;
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
            else if (@event is InputEventMouseMotion mouseMotion && _currentDraggedBlock != null)
            {
                HandleMouseMotion(mouseMotion);
            }
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
            // We don't need to handle releases anymore since we're using click-to-toggle
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

            // If we click on the currently dragged block, place it
            if (_currentDraggedBlock == block)
            {
                GD.Print($"[Debug InputHelper] Clicked dragged block {blockName}, placing it");
                PlaceBlock(block, mouseEvent.GlobalPosition);
                return;
            }

            // If the block is in the toolbar, handle it specially
            if (block.GetParent() is F.Game.Toolbar.ToolbarBlockContainer container)
            {
                GD.Print("[Debug InputHelper] Block '" + blockName + "' is in the toolbar. Moving to BlockLayer before dragging.");
                var blockLayer = GetTree().Root.GetNodeOrNull<Node2D>(BlockConfig.BlockLayerPath);
                if (blockLayer == null)
                {
                    var currentScene = GetTree().CurrentScene;
                    if (currentScene != null)
                        blockLayer = currentScene.GetNodeOrNull<Node2D>("GameManager/BlockLayer");
                }

                if (blockLayer != null)
                {
                    Vector2 globalPos = block.GlobalPosition;
                    if (block.GetParent() is Control ctrl) {
                        globalPos = ctrl.GlobalPosition + block.Position;
                    }
                    container.RemoveChild(block);
                    blockLayer.AddChild(block);
                    block.GlobalPosition = mouseEvent.GlobalPosition;
                    block.SetInToolbar(false);
                }
                else
                {
                    GD.PrintErr("[Debug InputHelper] Could not locate BlockLayer; not moving block.");
                    GetViewport().SetInputAsHandled();
                    return;
                }
            }

            // Start dragging the block
            GD.Print("[Debug InputHelper] Starting drag for block '" + blockName + "' at " + mouseEvent.GlobalPosition);
            _currentDraggedBlock = block;
            _dragHelper?.StartDrag(block, mouseEvent.GlobalPosition);
            
            GetViewport().SetInputAsHandled();
        }

        private void HandleMouseMotion(InputEventMouseMotion mouseEvent)
        {
            if (_currentDraggedBlock != null)
            {
                _dragHelper?.UpdateDrag(_currentDraggedBlock, mouseEvent.GlobalPosition);
                
                // Only highlight pipes if the block is not connected
                if (_currentDraggedBlock is BaseBlock baseBlock && !baseBlock.HasConnections())
                {
                    Vector2 effectivePos = _currentDraggedBlock.GetTokenPosition();
                    GD.Print($"[Debug InputHelper] Checking for pipe at token position: {effectivePos}");
                    ConnectionHelper.HighlightPipeAtPosition(this, effectivePos);
                }
                else
                {
                    // Clear any existing highlights if block is connected
                    ConnectionHelper.ClearPipeHighlights(this);
                }
            }
        }

        private void PlaceBlock(BaseBlock block, Vector2 position)
        {
            string blockName = ((IBlock)block).Name;
            if (string.IsNullOrEmpty(blockName))
                blockName = block.GetName();

            GD.Print($"[Debug InputHelper] Placing block {blockName} at {position}");
            
            // Try to connect the block at its current position
            Vector2 effectivePos = block.GetTokenPosition();
            GD.Print($"[Debug InputHelper] Attempting to connect block at position: {effectivePos}");
            bool connected = ConnectionHelper.TryConnectBlock(this, block, effectivePos);
            
            if (connected)
            {
                GD.Print($"[Debug InputHelper] Successfully connected block {blockName}");
                block.ZIndex = ZIndexConfig.Layers.PlacedBlock;
            }
            else
            {
                GD.Print($"[Debug InputHelper] Failed to connect block {blockName}");
                block.SetPlaced(true);
                block.ZIndex = ZIndexConfig.Layers.PlacedBlock;
                GD.Print($"[Debug InputHelper] Block {blockName} reverted to Placed state");
            }

            _dragHelper?.EndDrag(block);
            _currentDraggedBlock = null;
        }
    }
} 