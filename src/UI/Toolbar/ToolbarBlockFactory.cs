namespace F.UI.Toolbar;

/// <summary>
///     Responsible for creating and configuring blocks in the toolbar.
/// </summary>
public class ToolbarBlockFactory
{
    private readonly Container _blockContainer;
    private readonly GameManager _gameManager;

    public ToolbarBlockFactory(GameManager gameManager, Container blockContainer)
    {
        _gameManager = gameManager;
        _blockContainer = blockContainer;
    }

    /// <summary>
    ///     Creates a block in the toolbar from the given metadata.
    /// </summary>
    public BaseBlock? CreateToolbarBlock(BlockMetadata metadata)
    {
        GD.Print($"Creating toolbar block with metadata: {metadata.Id}");

        // Create a CenterContainer for UI layout
        var centerContainer = new CenterContainer
        {
            CustomMinimumSize = new Vector2(200, 256), // Fixed width for each block slot
            SizeFlagsHorizontal = Control.SizeFlags.Fill,
            SizeFlagsVertical = Control.SizeFlags.Fill
        };

        // Create a Node2D to hold the block
        var blockParent = new Node2D();

        // Create the block using GameManager
        var block = _gameManager.CreateBlock(metadata, blockParent);
        if (block == null)
        {
            GD.PrintErr($"Failed to create block: {metadata.Id}");
            blockParent.QueueFree();
            centerContainer.QueueFree();
            return null;
        }

        // Set up the hierarchy: CenterContainer -> Node2D -> Block
        centerContainer.AddChild(blockParent);

        // Center the block parent in the container
        blockParent.Position = new Vector2(100, 128); // Center in the 200x256 container

        // Add container to the block container using CallDeferred
        _blockContainer.CallDeferred(Node.MethodName.AddChild, centerContainer);

        // Configure block properties
        ConfigureBlock(block);

        GD.Print($"Successfully created toolbar block: {metadata.Id}");
        return block;
    }

    /// <summary>
    ///     Returns a block to the toolbar, handling all necessary state changes.
    /// </summary>
    public void ReturnBlockToToolbar(BaseBlock block)
    {
        var oldParent = block.GetParent();
        if (oldParent == _blockContainer) return;

        // Clear any existing dragged block reference in GameManager
        if (_gameManager.GetDraggedBlock() == block) _gameManager.HandleBlockDrop();

        // Reset block state
        block.SetDragging(false);
        block.SetPlaced(false);

        // Create a CenterContainer for UI layout
        var centerContainer = new CenterContainer
        {
            CustomMinimumSize = new Vector2(200, 256), // Fixed width for each block slot
            SizeFlagsHorizontal = Control.SizeFlags.Fill,
            SizeFlagsVertical = Control.SizeFlags.Fill
        };

        // Create a Node2D to hold the block
        var blockParent = new Node2D();

        // Remove block from old parent and add to new parent
        oldParent?.RemoveChild(block);
        blockParent.AddChild(block);
        centerContainer.AddChild(blockParent);

        // Center the block parent in the container
        blockParent.Position = new Vector2(100, 128); // Center in the 200x256 container

        // Add container to the block container using CallDeferred
        _blockContainer.CallDeferred(Node.MethodName.AddChild, centerContainer);

        // Configure block properties
        ConfigureBlock(block);
    }

    /// <summary>
    ///     Configures common block properties for toolbar blocks.
    /// </summary>
    private void ConfigureBlock(BaseBlock block)
    {
        block.SetProcessInput(true); // Enable input processing
        block.Scale = Vector2.One; // Set scale to 1
        block.ZIndex = 10; // Set initial z-index
    }
}