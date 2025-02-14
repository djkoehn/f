using F.Config.Visual;
using F.Config;

// ADD THIS!

// ADD THIS!

namespace F.Game.Core;

public sealed partial class BlockManager : Node
{
    private BaseBlock? _draggedBlock;
    private Vector2 _dragOffset;
    private GameManager? _gameManager;

    public bool IsDragging => _draggedBlock != null;

    public override void _Ready()
    {
        base._Ready();
        // Wait one frame for GameManager to initialize
        CallDeferred("DeferredReady");
    }

    private void DeferredReady()
    {
        _gameManager = GameManager.Instance;
        if (_gameManager == null)
        {
            GD.PrintErr("GameManager singleton not found! Did you forget to add GameManager to the scene?");
            return;
        }

        GD.Print("BlockManager connected to GameManager");
    }

    public void StartDragging(BaseBlock block, Vector2 clickPosition)
    {
        _draggedBlock = block;
        _dragOffset = block.GlobalPosition - clickPosition;
    }

    public void StopDragging()
    {
        _draggedBlock = null;
    }

    public override void _Process(double delta)
    {
        if (_draggedBlock != null)
        {
            var mousePos = GetViewport().GetMousePosition();
            _draggedBlock.GlobalPosition = mousePos;
        }
    }

    public void HandleBlockDrag(BaseBlock block)
    {
        if (_draggedBlock != null && _draggedBlock != block) return;
        _draggedBlock = block;
        block.SetDragging(true);
        block.OnDragStart(GetViewport().GetMousePosition());
    }

    public void HandleBlockDrop(BaseBlock block, Vector2 position)
    {
        if (_draggedBlock != block) return;

        // Update block position
        block.GlobalPosition = position;
        block.SetDragging(false);
        block.OnDragEnd();
        block.SetPlaced(true);

        // Clear dragged block reference
        _draggedBlock = null;

        // Play sound effect
        // AudioManager.Instance?.PlayBlockPlace();
    }

    public BaseBlock? GetDraggedBlock()
    {
        return _draggedBlock;
    }

    public BaseBlock? CreateBlock(BlockMetadata metadata, Node2D parent)
    {
        try
        {
            GD.Print($"Attempting to create block {metadata.Id} with scene path: {metadata.ScenePath}");

            // Convert scene path to project-relative path if needed
            var scenePath = metadata.ScenePath;
            if (!scenePath.StartsWith("res://")) scenePath = $"res://{scenePath}";

            // Load the block scene
            var blockScene = GD.Load<PackedScene>(scenePath);
            if (blockScene == null)
            {
                GD.PrintErr($"Failed to load block scene at path: {scenePath}");
                return null;
            }

            GD.Print($"Successfully loaded scene for block {metadata.Id}");

            // Instance the block
            var block = blockScene.Instantiate<BaseBlock>();
            GD.Print($"Created block of type: {block.GetType()} with ID: {metadata.Id}");
            if (block == null)
            {
                GD.PrintErr($"Failed to instantiate block: {metadata.Id}");
                return null;
            }

            GD.Print($"Successfully instantiated block {metadata.Id}");

            // Initialize the block
            var config = new BlockConfig
            {
                Name = metadata.Id,
                Type = metadata.Id
            };

            try
            {
                block.Initialize(config);
                GD.Print($"Successfully initialized block {metadata.Id}");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to initialize block {metadata.Id}: {e.Message}");
                block.QueueFree();
                return null;
            }

            // Add to parent
            parent.AddChild(block);

            // Set proper Z-index based on parent
            block.ZIndex = parent.GetType().ToString().Contains("Toolbar")
                ? ZIndexConfig.Layers.ToolbarBlock
                : ZIndexConfig.Layers.Block;
            block.ZAsRelative = false;

            GD.Print($"Successfully created block {metadata.Id}");
            return block;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error creating block {metadata.Id}: {e.Message}\nStack trace: {e.StackTrace}");
            return null;
        }
    }
}