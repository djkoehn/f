using Godot;
using F.Game.BlockLogic;
using System.Collections.Generic;

namespace F.Utils.Helpers
{
    public partial class DragHelper : Node, IDragService
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
            GD.Print($"[Debug DragHelper] Entering StartDrag with block: {block.Name}, clickPosition: {clickPosition}");
            string blockName = block.Name;
            Vector2 offset;
            
            // Attempt to get the BlockLayer node
            var blockLayer = GetTree().Root.GetNodeOrNull<Node2D>("/root/Main/GameManager/BlockLayer");
            if (blockLayer != null)
            {
                // Store the block's global position before reparenting
                Vector2 initialGlobalPos = block.GlobalPosition;
                
                // Reparent block to BlockLayer if necessary
                if (block.GetParent() != blockLayer)
                {
                    block.GetParent()?.RemoveChild(block);
                    blockLayer.AddChild(block);
                    // Restore the global position so it remains unchanged
                    block.GlobalPosition = initialGlobalPos;
                    GD.Print($"[Debug DragHelper] Reparented block {blockName} to BlockLayer and preserved global position: {initialGlobalPos}.");
                }
                
                // Compute the offset in BlockLayer's coordinate space
                // block.Position is now the block's position relative to BlockLayer
                // blockLayer.ToLocal(clickPosition) converts the global click position to BlockLayer's local coordinates
                offset = block.Position - blockLayer.ToLocal(clickPosition);
            }
            else
            {
                // Fallback: compute offset in global coordinates
                offset = block.GlobalPosition - clickPosition;
            }
            
            GD.Print($"[Debug DragHelper] StartDrag called for block: {blockName}, computed offset: {offset}");
            
            if (_dragOffsets.ContainsKey(block))
                _dragOffsets[block] = offset;
            else
                _dragOffsets.Add(block, offset);
            
            block.SetDragging(true);
        }

        // Update drag: update block's global position using stored offset
        public void UpdateDrag(BaseBlock block, Vector2 position)
        {
            GD.Print($"[Debug DragHelper] Entering UpdateDrag with block: {block.Name}, position: {position}");
            string blockName = block.Name;

            // Try to get the BlockLayer node
            var blockLayer = GetTree().Root.GetNodeOrNull<Node2D>("/root/Main/GameManager/BlockLayer");
            if (blockLayer != null && block.GetParent() == blockLayer)
            {
                if (_dragOffsets.TryGetValue(block, out Vector2 offset))
                {
                    // Compute new local position wrt BlockLayer
                    Vector2 newLocalPos = blockLayer.ToLocal(position) + offset;
                    block.Position = newLocalPos; // sets the block's local position relative to BlockLayer
                    GD.Print($"[Debug DragHelper] UpdateDrag (BlockLayer): block {blockName} new local position: {newLocalPos}, global: {block.GlobalPosition}");
                }
                else
                {
                    Vector2 newLocalPos = blockLayer.ToLocal(position);
                    block.Position = newLocalPos;
                    GD.Print($"[Debug DragHelper] UpdateDrag (BlockLayer fallback): block {blockName} new local position: {newLocalPos}, global: {block.GlobalPosition}");
                }
            }
            else
            {
                // fallback if block isn't parented under BlockLayer
                if (_dragOffsets.TryGetValue(block, out Vector2 offset))
                {
                    Vector2 newPos = position + offset;
                    block.GlobalPosition = newPos;
                    GD.Print($"[Debug DragHelper] UpdateDrag (global fallback): block {blockName} new position: {newPos}");
                }
                else
                {
                    block.GlobalPosition = position;
                    GD.Print($"[Debug DragHelper] UpdateDrag (global fallback): no offset stored for block {blockName}, set position directly to {position}");
                }
            }
        }

        // End drag: remove stored offset and set block state to Placed
        public void EndDrag(BaseBlock block)
        {
            GD.Print($"[Debug DragHelper] Entering EndDrag with block: {block.Name}");
            string blockName = block.Name;
            GD.Print($"[Debug DragHelper] EndDrag called for block: {blockName}");
            if (_dragOffsets.ContainsKey(block))
                _dragOffsets.Remove(block);
            block.SetDragging(false);
        }
    }
} 