using Godot;
using F.Framework.Input;
using F.Framework.Connections;
using F.Framework.Blocks;
using F.Game.Core;
using F.Game.Tokens;
using F.Framework.Core.SceneTree;

namespace F.Framework.Core;

/// <summary>
/// Global services that are autoloaded by Godot.
/// Access these through Services.Instance.
/// </summary>
public partial class Services : Node
{
    public static Services? Instance { get; private set; }

    public required IGameManager Game { get; set; }
    public required InputManager Input { get; set; }
    public required ConnectionManager Connections { get; set; }
    public required BlockManager Blocks { get; set; }
    public required TokenManager Tokens { get; set; }
    public required IInventory Inventory { get; set; }
    public required BlockMetadata BlockMetadata { get; set; }

    private bool _isInitialized;
    private const float RETRY_INTERVAL = 0.1f;
    private const int MAX_RETRIES = 10;
    private int _initRetryCount;

    public override void _EnterTree()
    {
        if (Instance == null)
        {
            Instance = this;
            // Initialize local services immediately
            InitializeLocalServices();
            // Defer external services initialization to ensure scene tree is ready
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
            // Initialize services that are children of this node
            Inventory = GetNode<Inventory>("Inventory");
            BlockMetadata = GetNode<BlockMetadata>("BlockMetadata");
            Blocks = new BlockManager();
            AddChild(Blocks);

            GD.Print("[Services] Successfully initialized local services");
        }
        catch (Exception e)
        {
            GD.PrintErr("[Services] Failed to initialize local services: " + e.Message);
            GD.PrintErr("[Services] Stack trace: " + e.StackTrace);
        }
    }

    private void InitializeExternalServices()
    {
        if (_isInitialized) return;

        try
        {
            // Get references from scene tree
            var mainScene = GetNode<GameManager>("/root/Main");
            if (mainScene == null)
            {
                RetryInitialization();
                return;
            }

            Game = mainScene;
            Input = mainScene.GetNode<InputManager>("InputManager");
            Connections = mainScene.GetNode<ConnectionManager>("ConnectionManager");
            Tokens = mainScene.GetNode<TokenManager>("TokenManager");

            _isInitialized = true;
            GD.Print("[Services] Successfully initialized all services");
        }
        catch (Exception e)
        {
            GD.PrintErr("[Services] Failed to initialize external services: " + e.Message);
            GD.PrintErr("[Services] Stack trace: " + e.StackTrace);
            RetryInitialization();
        }
    }

    private void RetryInitialization()
    {
        _initRetryCount++;
        if (_initRetryCount >= MAX_RETRIES)
        {
            GD.PrintErr($"[Services] Failed to initialize after {MAX_RETRIES} attempts");
            return;
        }

        GD.Print($"[Services] Services not ready, retry {_initRetryCount}/{MAX_RETRIES} in {RETRY_INTERVAL}s");
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
        if (Instance == this)
        {
            Instance = null;
        }
    }
}