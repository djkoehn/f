using F.Game.Connections;
using F.Game.Tokens;
using F.Game.Core;
using F.Utils;
using F.Game.BlockLogic;
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
        TokenManager = GetNode<TokenManager>("TokenManager");

        if (blockLayer == null || inventory == null || tokenLayer == null ||
            _blockInteractionManager == null || TokenManager == null)
        {
            GD.PrintErr("Required components not found!");
            return;
        }

        // Initialize managers
        ConnectionManager = blockLayer;  // Store the reference
        _gameState = new GameStateManager(inventory);
        BlockFactory = new F.Game.Core.BlockFactory(this);

        // Initialize game state
        _gameState.Initialize();

        // Set metadata for input and output blocks
        var inputBlock = GetNode<BaseBlock>("BlockLayer/BlockLayerViewport/BlockLayerContent/Input");
        var outputBlock = GetNode<BaseBlock>("BlockLayer/BlockLayerViewport/BlockLayerContent/Output");

        if (inputBlock != null)
        {
            var inputMetadata = BlockMetadata.GetMetadata("input");
            if (inputMetadata != null)
            {
                inputBlock.Metadata = inputMetadata;
                GD.Print($"[GameManager Debug] Set input block metadata - SpawnOnSpace: {inputMetadata.SpawnOnSpace}");
            }
        }

        if (outputBlock != null)
        {
            var outputMetadata = BlockMetadata.GetMetadata("output");
            if (outputMetadata != null)
            {
                outputBlock.Metadata = outputMetadata;
                GD.Print($"[GameManager Debug] Set output block metadata - SpawnOnSpace: {outputMetadata.SpawnOnSpace}");
            }
        }
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }

    public BaseBlock? CreateBlock(F.Game.BlockLogic.BlockMetadata metadata, Node parent)
    {
        return BlockFactory?.CreateBlock(metadata, parent);
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