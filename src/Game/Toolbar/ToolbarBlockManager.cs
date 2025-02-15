using GMFG = F.Game.Core.GameManager;
using DragService = F.Utils.IDragService;
using BaseBlockFG = F.Game.BlockLogic.BaseBlock;
using F.Utils;
using F.Utils.Helpers;

namespace F.Game.Toolbar;

public partial class ToolbarBlockManager : Node
{
    [Signal]
    public delegate void BlockPositionsUpdatedEventHandler(float width);

    private readonly Dictionary<string, BaseBlock> _blocks = new();
    private HBoxContainer? _blockContainer;

    private GMFG? _gameManager;
    private DragService? _dragHelper;

    public override void _Ready()
    {
        base._Ready();
        _gameManager = GetNode<GMFG>("/root/Main/GameManager");
        _blockContainer = GetParent().GetNode<HBoxContainer>("BlockContainer");
        var hf = HelperFunnel.GetInstance();
        _dragHelper = hf?.DragHelper as DragService;

        if (_gameManager == null || _blockContainer == null)
            GD.PrintErr("Required nodes not found in ToolbarBlockManager!");
    }

    public void AddBlock(string blockType)
    {
        if (_gameManager == null || _blockContainer == null) return;

        GD.Print("Adding block of type: " + blockType);

        var metadata = BlockMetadata.Create(blockType);
        if (metadata == null) return;

        // Load and instantiate the block scene
        var scene = GD.Load<PackedScene>(metadata.ScenePath);
        if (scene == null)
        {
            GD.PrintErr($"Failed to load block scene: {metadata.ScenePath}");
            return;
        }

        var block = scene.Instantiate<BaseBlock>();
        if (block == null)
        {
            GD.PrintErr($"Failed to instantiate block: {blockType}");
            return;
        }

        _blockContainer.AddChild(block);
        block.SetProcessInput(true);
        block.Scale = Vector2.One;
        block.ZIndex = 10;

        // Connect to our local handler
        const string fixedBlockClickedSignal = "block_clicked";
        block.Connect(fixedBlockClickedSignal, new Callable(this, nameof(OnBlockClicked)));

        _blocks[blockType] = block;
        UpdateBlockPositions();
    }

    private void OnBlockClicked(BaseBlock block)
    {
        if (_gameManager == null) return;

        // Get the BlockLayer
        var BlockLayer = _gameManager.GetNode<Node2D>("BlockLayer");
        if (BlockLayer == null)
        {
            GD.PrintErr("BlockLayer not found!");
            return;
        }

        // Get block's current global position before reparenting
        var globalPos = block.GlobalPosition;

        // Remove from blocks dictionary and toolbar
        var key = _blocks.FirstOrDefault(x => x.Value == block).Key;
        if (key != null) _blocks.Remove(key);
        block.GetParent()?.RemoveChild(block);

        // Move to BlockLayer
        BlockLayer.AddChild(block);
        block.GlobalPosition = globalPos;

        // Start dragging
        _dragHelper?.StartDrag(block, globalPos);

        UpdateBlockPositions();
    }

    public void ClearBlocks()
    {
        if (_blockContainer == null) return;
        foreach (var child in _blockContainer.GetChildren()) child.QueueFree();
        _blocks.Clear();
        UpdateBlockPositions();
    }

    public void ReturnBlockToToolbar(BaseBlock block)
    {
        if (_blockContainer == null) return;
        block.GetParent()?.RemoveChild(block);
        _blockContainer.AddChild(block);
        block.SetDragging(false);
        block.SetPlaced(false);
        UpdateBlockPositions();
    }

    public void RemoveBlock(BaseBlock block)
    {
        if (_blockContainer == null) return;
        _blockContainer.RemoveChild(block);
        var key = _blocks.FirstOrDefault(x => x.Value == block).Key;
        if (key != null) _blocks.Remove(key);
        UpdateBlockPositions();
    }

    private void UpdateBlockPositions()
    {
        if (_blockContainer == null) return;
        float totalWidth = _blockContainer.GetChildren().Count * (100 + 40); // Block width + separation from scene
        EmitSignal(SignalName.BlockPositionsUpdated, totalWidth);
    }
}