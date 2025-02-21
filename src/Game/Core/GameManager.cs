using F.Game.Connections;
using F.Game.Tokens;
using F.Game.Core;
using F.Utils;
using InventoryType = F.Game.Core.Inventory;

namespace F.Game.Core;

public partial class GameManager : Node2D
{
    private BlockInteractionManager? _blockInteractionManager;
    private GameStateManager? _gameState;
    public static GameManager? Instance { get; private set; }
    public ConnectionManager? ConnectionManager { get; private set; }
    public TokenManager? TokenManager { get; private set; }
    public F.Game.Core.BlockFactory? BlockFactory { get; private set; }

    public override void _Ready()
    {
        Instance = this; // MAKE SURE THIS HAPPENS FIRST
        GD.Print("GameManager initialized as singleton");

        // Get required components
        var blockLayer = GetNode<ConnectionManager>("BlockLayer");
        var tokenLayer = GetNode<Node2D>("TokenLayer");
        var inventory = GetNode<InventoryType>("Inventory");
        _blockInteractionManager = GetNode<BlockInteractionManager>("BlockInteractionManager");

        if (blockLayer == null || inventory == null || tokenLayer == null ||
            _blockInteractionManager == null)
        {
            GD.PrintErr("Required components not found!");
            return;
        }

        // Initialize managers
        ConnectionManager = blockLayer;  // Store the reference
        TokenManager = new TokenManager(ConnectionManager, tokenLayer);
        AddChild(TokenManager); // Add TokenManager to the scene tree
        TokenManager.Name = "TokenManager"; // Set the name so it can be found by path
        GD.Print("[GameManager] TokenManager initialized and added to scene tree");

        _gameState = new GameStateManager(inventory);
        BlockFactory = new F.Game.Core.BlockFactory(this);

        // Initialize game state
        _gameState.Initialize();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }

    public BaseBlock? CreateBlock(F.Game.BlockLogic.BlockMetadata metadata, Node parent)
    {
        if (string.IsNullOrEmpty(metadata.ScenePath))
        {
            GD.PrintErr($"ScenePath is empty for block {metadata.Id}");
            return null;
        }
        var blockScene = GD.Load<PackedScene>(metadata.ScenePath);
        if (blockScene == null)
        {
            GD.PrintErr($"Failed to load block scene at path: {metadata.ScenePath}");
            return null;
        }
        var block = blockScene.Instantiate<BaseBlock>();
        if (block == null)
        {
            GD.PrintErr($"Failed to instantiate block scene at path: {metadata.ScenePath}");
            return null;
        }
        // Remove from existing parent if necessary
        if (block.GetParent() != null)
        {
            block.GetParent().RemoveChild(block);
        }
        parent.AddChild(block);
        block.Initialize(new BlockConfig { Name = metadata.Id });
        return block;
    }

    // --- New Block Factory Methods ---

    public BaseBlock? CreateToolbarBlock(F.Game.BlockLogic.BlockMetadata metadata)
    {
        // Get the toolbar container node relative to GameManager
        var toolbarContainer = GetNodeOrNull<Container>("Toolbar/BlockContainer");
        if (toolbarContainer == null)
        {
            GD.PrintErr("Toolbar container not found.");
            return null;
        }
        // Create the block using existing CreateBlock method
        var block = CreateBlock(metadata, toolbarContainer);
        if (block != null)
        {
            ConfigureBlock(block);
        }
        return block;
    }

    private void ConfigureBlock(BaseBlock block)
    {
        block.Position = Vector2.Zero;
        block.Scale = Vector2.One;
        block.Rotation = 0;
        block.SetDragging(false);
        block.SetPlaced(false);
    }

    // --- End New Block Factory Methods ---
}