using F.Game.Connections;
using F.Game.Tokens;
using F.UI.Input;

// ADD THIS!

namespace F.Game.Core;

public partial class GameManager : Node2D
{
    private BlockInteractionManager? _blockInteractionManager;

    private BaseBlock? _draggedBlock;
    private BlockDragHandler? _dragHandler;
    private GameStateManager? _gameState;
    public static GameManager? Instance { get; private set; }
    public ConnectionManager? ConnectionManager { get; private set; }

    public TokenManager? TokenManager { get; private set; }

    public BlockManager? BlockManager { get; private set; }

    public override void _Ready()
    {
        Instance = this; // MAKE SURE THIS HAPPENS FIRST
        GD.Print("GameManager initialized as singleton");

        // Get required components
        ConnectionManager = GetNode<ConnectionManager>("BlockLayer");
        BlockManager = GetNode<BlockManager>("BlockManager");
        _blockInteractionManager = GetNode<BlockInteractionManager>("BlockInteractionManager");
        var tokenLayer = GetNode<Node2D>("TokenLayer");
        var inventory = GetNode<Inventory>("Inventory");
        _dragHandler = GetNode<BlockDragHandler>("/root/Main/InputManager/BlockDragHandler");

        if (ConnectionManager == null || inventory == null || tokenLayer == null ||
            BlockManager == null || _blockInteractionManager == null)
        {
            GD.PrintErr("Required components not found!");
            return;
        }

        // Initialize managers
        TokenManager = new TokenManager(ConnectionManager, tokenLayer);
        _gameState = new GameStateManager(inventory);

        // Initialize game state
        _gameState.Initialize();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }

    public override void _Process(double delta)
    {
        TokenManager?.Update();
    }

    public BaseBlock? CreateBlock(BlockMetadata metadata, Node2D parent)
    {
        return BlockManager?.CreateBlock(metadata, parent);
    }

    public BaseBlock? GetDraggedBlock()
    {
        return _draggedBlock;
    }

    public void HandleBlockDrag(BaseBlock block)
    {
        if (_blockInteractionManager == null) return;

        // USE NEW DRAG HANDLER!
        _dragHandler?.StartDragging(block);
        _blockInteractionManager.SetDraggedBlock(block);
    }

    public void HandleBlockDrop()
    {
        if (_blockInteractionManager == null) return;

        var block = _blockInteractionManager.GetDraggedBlock();
        if (block != null)
        {
            // USE NEW DRAG HANDLER!
            _dragHandler?.StopDragging(block);
            _blockInteractionManager.SetDraggedBlock(null);
        }
    }
}