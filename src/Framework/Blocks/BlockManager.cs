using Godot;
using F.Framework.Core;

namespace F.Framework.Blocks;

public partial class BlockManager : Node
{
    private BaseBlock? _draggedBlock;
    private BaseBlock? _hoveredBlock;
    private Vector2 _dragOffset;

    public void Initialize()
    {
        // Initialize block management
    }

    public BaseBlock? GetBlockAtPosition(Vector2 position)
    {
        BaseBlock? closestBlock = null;
        float closestDistance = 50.0f; // picking threshold in pixels

        foreach (Node node in GetTree().GetNodesInGroup("Blocks"))
        {
            if (node is BaseBlock block)
            {
                float distance = block.GlobalPosition.DistanceTo(position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestBlock = block;
                }
            }
        }
        return closestBlock;
    }

    public void StartDrag(BaseBlock block, Vector2 position)
    {
        if (block.State == BlockState.InToolbar)
        {
            _draggedBlock = block;
            _dragOffset = block.GlobalPosition - position;
            // Send input to trigger state change, which will handle reparenting
            block.GetLogicMachine()?.Send(new BlockLogicInput.Interact());
            GD.Print($"[BlockManager Debug] Sent drag input to block {block.Name}");
        }
    }

    public void UpdateDrag(Vector2 position)
    {
        if (_draggedBlock == null) return;
        _draggedBlock.GlobalPosition = position + _dragOffset;
    }

    public void EndDrag()
    {
        if (_draggedBlock == null) return;
        // Send input to trigger state change
        _draggedBlock.GetLogicMachine()?.Send(new BlockLogicInput.Interact());
        _draggedBlock = null;
    }

    public bool IsDragging => _draggedBlock != null;
    public BaseBlock? DraggedBlock => _draggedBlock;

    public void SetHoveredBlock(BaseBlock? block)
    {
        if (_hoveredBlock == block) return;

        if (_hoveredBlock != null)
        {
            // Clear previous hover state
        }

        _hoveredBlock = block;

        if (_hoveredBlock != null)
        {
            // Set new hover state
        }
    }

    public BaseBlock? CreateBlock(BlockMetadata metadata, Node parent)
    {
        if (string.IsNullOrEmpty(metadata.ScenePath))
        {
            GD.PrintErr($"Block {metadata.Id} has no scene path");
            return null;
        }

        var scene = GD.Load<PackedScene>(metadata.ScenePath);
        if (scene == null)
        {
            GD.PrintErr($"Failed to load block scene: {metadata.ScenePath}");
            return null;
        }

        var block = scene.Instantiate<BaseBlock>();
        if (block == null)
        {
            GD.PrintErr($"Failed to instantiate block: {metadata.Id}");
            return null;
        }

        block.Name = metadata.Id + GetUniqueBlockId();
        block.Metadata = metadata;
        block.SetProcessInput(true);

        // Add to parent
        parent.AddChild(block);
        GD.Print($"[BlockManager] Created block {block.Name} in {parent.Name}");

        return block;
    }

    private int _blockIdCounter = 0;
    private int GetUniqueBlockId() => _blockIdCounter++;

    public void ReturnBlockToToolbar(BaseBlock block)
    {
        if (block.GetLogicMachine() == null) return;
        block.GetLogicMachine().Send(new BlockLogicInput.ReturnBlock());
        GD.Print($"[BlockManager Debug] Sent return to toolbar input to block {block.Name}");
    }
}