using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using F.Framework.Blocks;
using F.Framework.Blocks.Interfaces;
using F.Framework.Connections;
using F.Framework.Core.Interfaces;
using F.Framework.Core.SceneTree;
using F.Framework.Input;
using F.Framework.Logging;
using F.Game.BlockLogic;
using F.Game.Core;
using F.Game.Tokens;
using F.Framework.Tokens.Interfaces;
using IBlockInteractionManager = F.Game.BlockLogic.IBlockInteractionManager;

namespace F.Framework.Core;

[Meta(typeof(IAutoNode))]
public partial class GameManager : Node2D, IGameManager, IProvide<GameManager>, IProvide<IGameManager>
{
	private IBlockInteractionManager? _blockInteractionManager;
	private ITokenManager? _tokenManager;
	private GameStateManager? _gameState;
	public static GameManager? Instance { get; private set; }
	public ConnectionManager? ConnectionManager { get; private set; }
	public TokenManager? TokenManagerImpl { get; private set; }

	[Dependency] public SceneTreeService SceneTreeService => DependentExtensions.DependOn<SceneTreeService>(this);

	public IBlockLayer BlockLayer =>
		GetNode<Node2D>("BlockLayer") as IBlockLayer ?? throw new Exception("BlockLayer not found");

	public ITokenLayer TokenLayer =>
		GetNode<Node2D>("TokenLayer") as ITokenLayer ?? throw new Exception("TokenLayer not found");

	public IInventory Inventory =>
		GetNode<Node>("/root/Services/Inventory") as IInventory ?? throw new Exception("Inventory not found");

	public IBlockInteractionManager BlockInteractionManager => _blockInteractionManager ??=
		GetNode<Node>("BlockInteractionManager") as IBlockInteractionManager ?? throw new Exception(
			"BlockInteractionManager not found");

	public IToolbar Toolbar =>
		GetNode<Node>("Toolbar") as IToolbar ?? throw new Exception("Toolbar not found");

	public ITokenManager TokenManager => _tokenManager ??=
		GetNode<Node>("TokenManager") as ITokenManager ?? throw new Exception("TokenManager not found");

	public ColorRect Background =>
		GetNode<ColorRect>("Background") ?? throw new Exception("Background not found");

	public override void _Ready()
	{
		Instance = this;
		ProcessMode = ProcessModeEnum.Always;
		Logger.Game.Print("GameManager initialized as singleton");
		this.Provide();

		try
		{
			// Initialize Input and Output blocks first
			var inputBlock = GetNode<BaseBlock>("BlockLayer/Input");
			var outputBlock = GetNode<BaseBlock>("BlockLayer/Output");

			if (inputBlock != null && outputBlock != null)
			{
				// Set metadata for input and output blocks
				inputBlock.Metadata = BlockMetadata.GetMetadata("input");
				outputBlock.Metadata = BlockMetadata.GetMetadata("output");

				Logger.Game.Print($"Metadata initialized for {inputBlock.Name} Input");
				Logger.Game.Print($"Metadata initialized for {outputBlock.Name} Output");
			}
			else
			{
				Logger.Game.Err("Failed to get Input or Output blocks!");
			}

			var blockLayer = BlockLayer;
			var tokenLayer = TokenLayer;
			var toolbar = Toolbar;
			var inventory = Inventory;

			Logger.Game.Print($"BlockLayer path: {blockLayer.GetPath()}");
			Logger.Game.Print($"TokenLayer path: {tokenLayer.GetPath()}");
			Logger.Game.Print($"Toolbar path: {toolbar.GetPath()}");

			// Initialize managers
			_gameState = new GameStateManager(inventory);

			// Initialize game state
			_gameState.Initialize();

			// Get required components
			ConnectionManager = GetNode<ConnectionManager>("ConnectionManager");
			TokenManagerImpl = GetNode<TokenManager>("TokenManager");

			if (ConnectionManager == null || TokenManagerImpl == null)
			{
				Logger.Game.Err("Required components not found!");
				return;
			}

			// Connect signals
			inventory.InventoryReady += OnInventoryReady;
		}
		catch (Exception e)
		{
			Logger.Game.Err($"Error during initialization: {e.Message}");
			Logger.Game.Err($"Stack trace: {e.StackTrace}");
		}
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
		if (Inventory != null) Inventory.InventoryReady -= OnInventoryReady;
	}

	public override void _Notification(int what)
	{
		this.Notify(what);
	}

	GameManager IProvide<GameManager>.Value()
	{
		return this;
	}

	IGameManager IProvide<IGameManager>.Value()
	{
		return this;
	}

	private void OnInventoryReady()
	{
		Logger.Game.Print("Inventory is ready");
	}

	public void Initialize()
	{
		// Initialize game state
		_gameState?.Initialize();
	}

	public void HandleBlockPlaced(BaseBlock block)
	{
		Logger.Game.Print($"Block {block.Name} placed at {block.Position}");
		// Add any specific handling for block placement
	}

	public void HandleBlockRemoved(BaseBlock block)
	{
		Logger.Game.Print($"Block {block.Name} removed from {block.Position}");
		// Add any specific handling for block removal
	}

	public void HandleBlockDragStarted(BaseBlock block)
	{
		Logger.Game.Print($"Block {block.Name} drag started");
		// Add any specific handling for drag start
	}

	public void HandleBlockDragEnded(BaseBlock block)
	{
		Logger.Game.Print($"Block {block.Name} drag ended");
		// Add any specific handling for drag end
	}
}
