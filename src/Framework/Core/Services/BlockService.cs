using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using F.Framework.Blocks;
using F.Framework.Blocks.Interfaces;
using F.Framework.Core.Interfaces;
using F.Framework.Logging;
using Godot;

namespace F.Framework.Core.Services;

[Meta(typeof(IAutoNode))]
public partial class BlockService : Node, IBlockService, IProvide<IBlockService>
{
    private ILogService? _log;
    private IBlockMetadata? _blockMetadata;
    private Vector2 _dragOffset;
    private BaseBlock? _hoveredBlock;
    private BaseBlock? _draggedBlock;

    public bool IsDragging => DraggedBlock != null;
    public BaseBlock? DraggedBlock => _draggedBlock;

    public BlockService(ILogService? log = null, IBlockMetadata? blockMetadata = null)
    {
        _log = log;
        _blockMetadata = blockMetadata;
    }

    public override void _Ready()
    {
        if (_log == null)
        {
            _log = GetNode<LogService>("/root/Services/LogService");
        }
        if (_blockMetadata == null)
        {
            _blockMetadata = GetNode<BlockMetadata>("/root/Services/BlockMetadata");
        }
        base._Ready();
        this.Provide();
        ProcessMode = ProcessModeEnum.Always;
    }

    public BaseBlock? GetBlockAtPosition(Vector2 position)
    {
        BaseBlock? closestBlock = null;
        var closestDistance = 50.0f; // picking threshold in pixels

        foreach (var node in GetTree().GetNodesInGroup("Blocks"))
            if (node is BaseBlock block)
            {
                var distance = block.GlobalPosition.DistanceTo(position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestBlock = block;
                }
            }

        return closestBlock;
    }

    public BaseBlock? CreateBlock(IBlockMetadata metadata, Node parent)
    {
        try
        {
            var scene = GD.Load<PackedScene>(metadata.Scene);
            if (scene == null)
            {
                Logger.Block.Err($"Failed to load scene at path: {metadata.Scene}");
                return null;
            }

            var block = scene.Instantiate<BaseBlock>();
            if (block == null)
            {
                Logger.Block.Err($"Failed to instantiate block from scene: {metadata.Scene}");
                return null;
            }

            parent.AddChild(block);
            block.Initialize(metadata);
            return block;
        }
        catch (Exception e)
        {
            Logger.Block.Err($"Error creating block: {e.Message}", e);
            return null;
        }
    }

    public void StartDrag(BaseBlock block, Vector2 position)
    {
        if (block.State == BlockState.InToolbar)
        {
            _draggedBlock = block;
            _dragOffset = block.GlobalPosition - position;
            // Send input to trigger state change, which will handle reparenting
            block.GetLogicMachine()?.Send(new Input.Interact());
            Logger.Block.Print($"Started dragging block {block.Name}");
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
        _draggedBlock.GetLogicMachine()?.Send(new Input.Interact());
        _draggedBlock = null;
    }

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

    public void ReturnBlockToToolbar(BaseBlock block)
    {
        if (block.GetLogicMachine() == null) return;
        block.GetLogicMachine().Send(new Input.ReturnBlock());
        Logger.Block.Print($"Returning block {block.Name} to toolbar");
    }

    public override void _Notification(int what)
    {
        this.Notify(what);
    }

    IBlockService IProvide<IBlockService>.Value()
    {
        return this;
    }
}