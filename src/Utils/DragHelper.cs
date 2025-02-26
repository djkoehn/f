namespace F.Utils;

public partial class DragHelper : Node
{
    // Store the drag offset per block
    private readonly Dictionary<BaseBlock, Vector2> _dragOffsets = new();
    public static DragHelper Instance { get; private set; } = default!;

    public override void _Ready()
    {
        Instance = this;
        GD.Print("[Debug DragHelper] DragHelper initialized.");
    }

    // Initiate drag: compute and store offset, set block state to Dragging
    public void StartDrag(BaseBlock block, Vector2 clickPosition)
    {
        var blockName = ((IBlock)block).Name ?? "unknown";
        GD.Print($"[Debug DragHelper] Entering StartDrag with block: {blockName}, clickPosition: {clickPosition}");

        // Attempt to get the BlockLayerContent node
        var blockLayerContent = GetTree().Root
            .GetNodeOrNull<Node2D>(SceneNodeConfig.GameManager.BlockLayer + "/BlockLayerViewport/BlockLayerContent");
        if (blockLayerContent != null)
        {
            // Reparent block to BlockLayerContent if necessary
            if (block.GetParent() != blockLayerContent)
            {
                block.GetParent()?.RemoveChild(block);
                blockLayerContent.AddChild(block);

                // Only convert position if the block is coming from outside BlockLayerContent
                var blockLayer = GetTree().Root.GetNodeOrNull<Node2D>(SceneNodeConfig.GameManager.BlockLayer);
                if (blockLayer != null)
                {
                    // Convert global position to BlockLayer's local coordinates
                    var localPos = blockLayer.ToLocal(clickPosition);
                    // Convert BlockLayer's local coordinates to viewport coordinates
                    var viewportPos = localPos + new Vector2(960, 540); // Center of viewport (1920x1080)
                    block.GlobalPosition = viewportPos;
                }
                else
                {
                    block.GlobalPosition = clickPosition;
                }
            }
            else
            {
                // Block is already in BlockLayerContent, just use the click position directly
                block.GlobalPosition = clickPosition;
            }

            // Set proper Z-index for dragged block (relative)
            ZIndexConfig.SetZIndex(block, ZIndexConfig.Layers.DraggedBlock, true);
        }
        else
        {
            GD.PrintErr(
                $"[Debug DragHelper] BlockLayerContent not found for block {blockName}, using fallback positioning");
            // Fallback: position directly in global coordinates
            block.GlobalPosition = clickPosition;
            ZIndexConfig.SetZIndex(block, ZIndexConfig.Layers.DraggedBlock, true);
        }

        // Store zero offset for smooth dragging
        _dragOffsets[block] = Vector2.Zero;

        // Set block state to dragging
        block.SetDragging(true);
        GD.Print($"[Debug DragHelper] Block {blockName} is now in dragging state");
    }

    // Update drag: update block's global position using stored offset
    public void UpdateDrag(BaseBlock block, Vector2 position)
    {
        var blockName = ((IBlock)block).Name ?? "unknown";
        GD.Print($"[Debug DragHelper] Entering UpdateDrag with block: {blockName}, position: {position}");

        // Simply update position directly since we're using zero offset
        block.GlobalPosition = position;
        GD.Print($"[Debug DragHelper] Updated block {blockName} position to: {position}");
    }

    // End drag: remove stored offset and set block state to Placed
    public void EndDrag(BaseBlock block)
    {
        var blockName = ((IBlock)block).Name ?? "unknown";
        GD.Print($"[Debug DragHelper] Entering EndDrag with block: {blockName}");

        if (_dragOffsets.ContainsKey(block))
            _dragOffsets.Remove(block);

        // Reset Z-index to placed block level (relative)
        ZIndexConfig.SetZIndex(block, ZIndexConfig.Layers.PlacedBlock, true);
        GD.Print($"[Debug DragHelper] Block {blockName} drag ended, Z-index set to {block.ZIndex}");
    }
}