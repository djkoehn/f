using Godot;
using F.Game.Tokens;
using F.Framework.Core;
using F.Framework.Blocks;
using F.Framework.Connections;
using F.Framework.Core.SceneTree;
using F.Game.BlockLogic;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Chickensoft.GodotNodeInterfaces;

namespace F.Game.Core;

[Meta(typeof(IAutoNode))]
public partial class GameManager : Node2D, IGameManager, IProvide<GameManager>, IProvide<IGameManager>
{
	private BlockInteractionManager? _blockInteractionManager;
	private GameStateManager? _gameState;
	public static GameManager? Instance { get; private set; }
	public ConnectionManager? ConnectionManager { get; private set; }
	public TokenManager? TokenManagerImpl { get; private set; }
	private BlockInteractionManager? _blockInteractionManagerImpl;

	[Dependency]
	public SceneTreeService SceneTreeService => this.DependOn<SceneTreeService>();

	public IBlockLayer BlockLayer => GetNode<Node2D>("BlockLayer") as IBlockLayer ?? throw new System.Exception("BlockLayer not found");
	public ITokenLayer TokenLayer => GetNode<Node2D>("TokenLayer") as ITokenLayer ?? throw new System.Exception("TokenLayer not found");
	public IInventory Inventory => Services.Instance.Inventory;
	public F.Game.BlockLogic.IBlockInteractionManager BlockInteractionManager => _blockInteractionManager ?? throw new System.Exception("BlockInteractionManager not initialized");
	public IToolbar Toolbar => GetNode<Control>("Toolbar") as IToolbar ?? throw new System.Exception("Toolbar not found");
	public ITokenManager TokenManager => TokenManagerImpl ?? throw new System.Exception("TokenManager not initialized");
	public ColorRect Background => GetNode<ColorRect>("Background");

	public override void _Ready()
	{
		Instance = this;
		GD.Print("GameManager initialized as singleton");
		this.Provide();

		// Get required components
		ConnectionManager = GetNode<ConnectionManager>("ConnectionManager");
		var blockLayer = GetNode<BlockLayer>("BlockLayer");
		var tokenLayer = GetNode<Node2D>("TokenLayer");
		_blockInteractionManager = GetNode<BlockInteractionManager>("BlockInteractionManager");
		TokenManagerImpl = GetNode<TokenManager>("TokenManager");

		if (blockLayer == null || tokenLayer == null ||
			_blockInteractionManager == null || TokenManagerImpl == null || ConnectionManager == null)
		{
			GD.PrintErr("Required components not found!");
			return;
		}

		// Print node paths for debugging
		GD.Print($"[GameManager] BlockLayer path: {blockLayer.GetPath()}");
		GD.Print($"[GameManager] TokenLayer path: {tokenLayer.GetPath()}");
		GD.Print($"[GameManager] Toolbar path: {GetNode<Control>("Toolbar")?.GetPath()}");

		// Initialize managers
		_gameState = new GameStateManager(Services.Instance.Inventory);
		_blockInteractionManagerImpl = _blockInteractionManager;

		// Initialize game state
		_gameState.Initialize();

		// Set metadata for input and output blocks
		var inputBlock = GetNode<BaseBlock>("BlockLayer/Input");
		var outputBlock = GetNode<BaseBlock>("BlockLayer/Output");

		if (inputBlock != null)
		{
			inputBlock.Metadata = new BlockMetadata
			{
				Id = "input",
				Name = "Input",
				HasInputSocket = false,
				HasOutputSocket = true,
				IsStationary = true,
				SpawnOnSpace = false
			};
		}

		if (outputBlock != null)
		{
			outputBlock.Metadata = new BlockMetadata
			{
				Id = "output",
				Name = "Output",
				HasInputSocket = true,
				HasOutputSocket = false,
				IsStationary = true,
				SpawnOnSpace = false
			};
		}

		// Connect signals
		Services.Instance.Inventory.InventoryReady += OnInventoryReady;
	}

	private void OnInventoryReady()
	{
		GD.Print("[GameManager Debug] Inventory is ready");
	}

	public override void _Process(double delta)
	{
		// Process game logic here
	}

	public override void _ExitTree()
	{
		if (Instance == this)
			Instance = null;

		// Cleanup
		if (Services.Instance?.Inventory != null)
		{
			Services.Instance.Inventory.InventoryReady -= OnInventoryReady;
		}
	}

	public override void _Notification(int what) => this.Notify(what);

	GameManager IProvide<GameManager>.Value() => this;
	IGameManager IProvide<IGameManager>.Value() => this;
}
