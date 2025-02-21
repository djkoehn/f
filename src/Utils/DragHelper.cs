using F.Game.BlockLogic;
using F.Game.Core;

namespace F.Utils
{
    public partial class DragHelper : Node
    {
        public static DragHelper Instance { get; private set; } = default!;

        // Store the drag offset per block
        private Dictionary<BaseBlock, Vector2> _dragOffsets = new Dictionary<BaseBlock, Vector2>();

        public override void _Ready()
        {
            Instance = this;
            GD.Print("[Debug DragHelper] DragHelper initialized.");
        }

        // Initiate drag: compute and store offset, set block state to Dragging
        public void StartDrag(BaseBlock block, Vector2 clickPosition)
        {
            string blockName = ((IBlock)block).Name ?? "unknown";
            GD.Print($"[Debug DragHelper] Entering StartDrag with block: {blockName}, clickPosition: {clickPosition}");
            
            // Attempt to get the BlockLayer node
            var blockLayer = GetTree().Root.GetNodeOrNull<Node2D>("/root/Main/GameManager/BlockLayer");
            if (blockLayer != null)
            {
                // Reparent block to BlockLayer if necessary
                if (block.GetParent() != blockLayer)
                {
                    block.GetParent()?.RemoveChild(block);
                    blockLayer.AddChild(block);
                }

                // Set proper Z-index for dragged block
                block.ZIndex = ZIndexConfig.Layers.DraggedBlock;
                block.ZAsRelative = false;  // Make Z-index absolute
                GD.Print($"[Debug DragHelper] Set Z-index to {block.ZIndex} for dragged block {blockName}");

                // Position the block at the click position immediately
                block.GlobalPosition = clickPosition;
                GD.Print($"[Debug DragHelper] Positioned block {blockName} at click position: {clickPosition}");
                
                // Store zero offset for smooth dragging
                _dragOffsets[block] = Vector2.Zero;
            }
            else
            {
                GD.PrintErr($"[Debug DragHelper] BlockLayer not found for block {blockName}, using fallback positioning");
                // Fallback: position directly in global coordinates
                block.GlobalPosition = clickPosition;
                block.ZIndex = ZIndexConfig.Layers.DraggedBlock;
                block.ZAsRelative = false;
                _dragOffsets[block] = Vector2.Zero;
            }
            
            // Set block state to dragging
            block.SetDragging(true);
            GD.Print($"[Debug DragHelper] Block {blockName} is now in dragging state");
        }

        // Update drag: update block's global position using stored offset
        public void UpdateDrag(BaseBlock block, Vector2 position)
        {
            string blockName = ((IBlock)block).Name ?? "unknown";
            GD.Print($"[Debug DragHelper] Entering UpdateDrag with block: {blockName}, position: {position}");
            
            // Simply update position directly since we're using zero offset
            block.GlobalPosition = position;
            GD.Print($"[Debug DragHelper] Updated block {blockName} position to: {position}");
        }

        // End drag: remove stored offset and set block state to Placed
        public void EndDrag(BaseBlock block)
        {
            string blockName = ((IBlock)block).Name ?? "unknown";
            GD.Print($"[Debug DragHelper] Entering EndDrag with block: {blockName}");
            
            if (_dragOffsets.ContainsKey(block))
                _dragOffsets.Remove(block);
            
            // Reset Z-index to placed block level
            block.ZIndex = ZIndexConfig.Layers.PlacedBlock;
            GD.Print($"[Debug DragHelper] Block {blockName} drag ended, Z-index set to {block.ZIndex}");
        }
    }
} 