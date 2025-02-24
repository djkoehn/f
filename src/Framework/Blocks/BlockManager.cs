using F.Framework.Core;
using F.Framework.Core.Interfaces;
using F.Framework.Core.Services;
using F.Game.BlockLogic;
using F.Framework.Blocks.Interfaces;
using F.Framework.Logging;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;

namespace F.Framework.Blocks;

[Meta(typeof(IAutoNode))]
public partial class BlockManager : Node, IBlockInteractionManager
{
    private IBlockService? _blockService;
    private Vector2 _dragOffset;
    private BaseBlock? _hoveredBlock;

    public bool IsDragging => DraggedBlock != null;
    public BaseBlock? DraggedBlock { get; private set; }

    public BlockManager(IBlockService? blockService = null)
    {
        _blockService = blockService;
    }

    public override void _Ready()
    {
        if (_blockService == null)
        {
            _blockService = GetNode<BlockService>("../BlockService");
        }
        this.Notify((int)Node.NotificationReady);
    }

    public override void _Notification(int what)
    {
        this.Notify(what);
    }

    public void Initialize()
    {
        // Initialize block management
        ProcessMode = ProcessModeEnum.Always;
    }

    public BaseBlock? GetBlockAtPosition(Vector2 position)
    {
        return _blockService.GetBlockAtPosition(position);
    }

    public void StartDrag(BaseBlock block, Vector2 position)
    {
        _blockService.StartDrag(block, position);
    }

    public void UpdateDrag(Vector2 position)
    {
        _blockService.UpdateDrag(position);
    }

    public void EndDrag()
    {
        _blockService.EndDrag();
    }

    public void SetHoveredBlock(BaseBlock? block)
    {
        _blockService.SetHoveredBlock(block);
    }

    public BaseBlock? CreateBlock(BlockMetadata metadata, Node parent)
    {
        return _blockService.CreateBlock(metadata, parent);
    }

    public void ReturnBlockToToolbar(BaseBlock block)
    {
        _blockService.ReturnBlockToToolbar(block);
    }
}