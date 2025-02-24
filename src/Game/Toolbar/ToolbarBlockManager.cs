using F.Framework.Core;
using F.Framework.Blocks;
using F.Framework.Connections;
using Godot;

namespace F.Game.Toolbar;

public partial class ToolbarBlockManager : Node
{
    [Signal]
    public delegate void BlockPositionsUpdatedEventHandler(float width);

    private readonly Dictionary<string, BaseBlock> _blocks = new();
    private HBoxContainer? _blockContainer;

    public override void _Ready()
    {
        base._Ready();
        _blockContainer = GetParent().GetNode<HBoxContainer>("BlockContainer");

        if (_blockContainer == null)
            GD.PrintErr("Required nodes not found in ToolbarBlockManager!");
    }

    public void AddBlock(string blockType)
    {
        if (_blockContainer == null) return;

        GD.Print("Adding block of type: " + blockType);

        var metadata = BlockMetadata.GetMetadata(blockType);
        if (metadata == null) return;

        // Let BlockManager create the block
        var block = Services.Instance.Blocks.CreateBlock(metadata, _blockContainer);
        if (block == null)
        {
            GD.PrintErr($"Failed to create block: {blockType}");
            return;
        }

        // Connect to our local handler
        const string fixedBlockClickedSignal = "block_clicked";
        block.Connect(fixedBlockClickedSignal, new Callable(this, nameof(OnBlockClicked)));

        _blocks[blockType] = block;
        UpdateBlockPositions();
    }

    private void OnBlockClicked(BaseBlock block)
    {
        // Only handle blocks in InToolbar state
        if (block.State != BlockState.InToolbar) return;

        // Remove from blocks dictionary since it's leaving the toolbar
        var key = _blocks.FirstOrDefault(x => x.Value == block).Key;
        if (key != null) _blocks.Remove(key);

        // Let BlockManager handle the interaction
        Services.Instance.Blocks.StartDrag(block, block.GlobalPosition);

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

        // Let BlockManager handle the state change
        Services.Instance.Blocks.ReturnBlockToToolbar(block);

        // Add to our dictionary for tracking
        var metadata = block.Metadata;
        if (metadata != null)
        {
            _blocks[metadata.Id] = block;
        }

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