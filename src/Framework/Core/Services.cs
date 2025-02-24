using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using F.Framework.Blocks;
using F.Framework.Blocks.Interfaces;
using F.Framework.Connections;
using F.Framework.Connections.Interfaces;
using F.Framework.Core.Interfaces;
using F.Framework.Core.Services;
using F.Framework.Core.Services.Interfaces;
using F.Framework.Input;
using F.Framework.Input.Interfaces;
using F.Framework.Logging;
using F.Framework.Tokens;
using F.Framework.Tokens.Interfaces;
using F.Game.Core;
using F.Game.Tokens;

namespace F.Framework.Core.Services;

/// <summary>
///     Global services that are autoloaded by Godot.
///     Access these through Services.Instance.
/// </summary>
[Meta(typeof(IAutoNode))]
public partial class Services : Node, IProvide<Services>
{
    private const float RETRY_INTERVAL = 0.1f;
    private const int MAX_RETRIES = 10;
    private int _initRetryCount;
    private bool _isInitialized;
    private ILogService? _log;

    public static Services? Instance { get; private set; }

    public required IGameManager Game { get; set; }
    public required IInputManager Input { get; set; }
    public required F.Framework.Connections.Interfaces.IConnectionManager Connections { get; set; }
    public required F.Framework.Blocks.Interfaces.IBlockService Blocks { get; set; }
    public required F.Framework.Tokens.Interfaces.ITokenManager Tokens { get; set; }
    public required IInventory Inventory { get; set; }
    public required IBlockMetadata BlockMetadata { get; set; }
    public required F.Framework.Core.Services.Interfaces.ISceneTreeService SceneTree { get; set; }

    public Services()
    {
        // Default constructor
    }

    public Services(ILogService? log = null)
    {
        _log = log;
    }

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;

            // Create required services
            if (_log == null)
            {
                _log = new LogService();
                AddChild(_log as Node);
            }

            var inventory = new Inventory();
            AddChild(inventory);
            Inventory = inventory;

            var blockMetadata = new BlockMetadata();
            AddChild(blockMetadata);
            BlockMetadata = blockMetadata;

            var sceneTreeService = new SceneTreeService(_log);
            AddChild(sceneTreeService);
            SceneTree = sceneTreeService;

            // Initialize services
            InitializeLocalServices();
            CallDeferred(nameof(InitializeExternalServices));
        }
        else
        {
            QueueFree();
        }
    }

    private void InitializeLocalServices()
    {
        try
        {
            // Initialize BlockService
            var blockService = new BlockService(_log, BlockMetadata);
            AddChild(blockService);
            Blocks = blockService;

            _log?.Info("Successfully initialized local services");
        }
        catch (Exception e)
        {
            _log?.Error("Failed to initialize local services", e);
        }
    }

    private void InitializeExternalServices()
    {
        if (_isInitialized) return;

        try
        {
            // Get the main scene first
            var mainScene = GetNode<GameManager>("/root/Main");
            if (mainScene == null)
            {
                RetryInitialization();
                return;
            }

            Game = mainScene;

            // Create and add required managers in the correct order
            var connectionManager = new ConnectionManager();
            AddChild(connectionManager);
            Connections = connectionManager;

            var inputManager = new InputManager();
            AddChild(inputManager);
            Input = inputManager;

            var tokenManager = new TokenManager();
            AddChild(tokenManager);
            Tokens = tokenManager;

            _isInitialized = true;
            _log?.Info("Successfully initialized all services");
        }
        catch (Exception e)
        {
            _log?.Error("Failed to initialize external services", e);
            RetryInitialization();
        }
    }

    private void RetryInitialization()
    {
        _initRetryCount++;
        if (_initRetryCount >= MAX_RETRIES)
        {
            _log.Error($"Failed to initialize after {MAX_RETRIES} attempts");
            return;
        }

        _log.Info($"Services not ready, retry {_initRetryCount}/{MAX_RETRIES} in {RETRY_INTERVAL}s");
        var timer = new Timer
        {
            OneShot = true,
            WaitTime = RETRY_INTERVAL
        };
        AddChild(timer);
        timer.Timeout += () =>
        {
            timer.QueueFree();
            InitializeExternalServices();
        };
        timer.Start();
    }

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }

    public override void _Notification(int what)
    {
        this.Notify(what);
    }

    Services IProvide<Services>.Value()
    {
        return this;
    }
}