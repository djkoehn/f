using F.Game.BlockLogic;
using F.Game.Connections;
using F.Game.Tokens;

namespace F.Game.Core;

public partial class SceneInitializer : Node
{
    public override void _Ready()
    {
        // Defer initialization to ensure GameManager is fully ready
        CallDeferred(nameof(InitializeZIndices));
    }

    private void InitializeZIndices()
    {
        var main = GetTree().Root.GetNode<Node2D>("Main");
        if (main == null) return;

        // Set background
        if (main.GetNode("Background") is Node2D background)
            SetZIndex(background, ZIndexConfig.Layers.Background);

        // Get GameManager and its components
        var gameManager = main.GetNode<Node2D>("GameManager");
        if (gameManager == null)
        {
            GD.PrintErr("[SceneInitializer] Failed to get GameManager");
            return;
        }

        var blockLayer = gameManager.GetNode<Node2D>("BlockLayer");
        var connectionManager = blockLayer as ConnectionManager; // BlockLayer has ConnectionManager script
        var tokenManager = gameManager.GetNode<TokenManager>("TokenManager");

        if (blockLayer == null || connectionManager == null)
        {
            GD.PrintErr($"[SceneInitializer] Failed to get required components - BlockLayer: {blockLayer != null}, ConnectionManager: {connectionManager != null}");
            return;
        }

        SetZIndex(blockLayer, ZIndexConfig.Layers.Pipes);

        // Declare variables at the start of the block
        BaseBlock? input = null;
        BaseBlock? output = null;

        // Set input/output blocks and their metadata
        if (blockLayer.GetNode("Input") is BaseBlock inputBlock)
        {
            input = inputBlock;
            SetZIndex(input, ZIndexConfig.Layers.PlacedBlock);
            var inputMetadata = BlockMetadata.GetMetadata("input");
            if (inputMetadata != null)
            {
                input.Metadata = inputMetadata;
                GD.Print($"[SceneInitializer] Set Input block metadata - SpawnOnSpace: {inputMetadata.SpawnOnSpace}");
                GD.Print($"[SceneInitializer] Input block connection state - HasOutputConnection: {input.HasOutputConnection()}, HasInputConnection: {input.HasInputConnection()}");
            }
        }
        if (blockLayer.GetNode("Output") is BaseBlock outputBlock)
        {
            output = outputBlock;
            SetZIndex(output, ZIndexConfig.Layers.PlacedBlock);
            var outputMetadata = BlockMetadata.GetMetadata("output");
            if (outputMetadata != null)
            {
                output.Metadata = outputMetadata;
                GD.Print($"[SceneInitializer] Set Output block metadata - HasInputSocket: {outputMetadata.HasInputSocket}");
                GD.Print($"[SceneInitializer] Output block connection state - HasOutputConnection: {output.HasOutputConnection()}, HasInputConnection: {output.HasInputConnection()}");
            }
        }

        // Create initial connection between Input and Output blocks
        if (input != null && output != null)
        {
            GD.Print("[SceneInitializer] Creating initial connection between Input and Output blocks");
            if (connectionManager != null)
            {
                bool connected = connectionManager.ConnectBlocks(input, output);
                GD.Print($"[SceneInitializer] Initial connection result: {connected}");
                if (connected)
                {
                    GD.Print($"[SceneInitializer] Connection state after connecting - Input(HasOutput: {input.HasOutputConnection()}), Output(HasInput: {output.HasInputConnection()})");
                }
            }
        }

        // Set toolbar
        if (gameManager.GetNode("Toolbar") is Node2D toolbar)
            SetZIndex(toolbar, ZIndexConfig.Layers.Toolbar);
    }

    private void SetZIndex(Node2D node, int zIndex)
    {
        node.ZIndex = zIndex;
        node.ZAsRelative = false;
        GD.Print($"Set {node.Name} Z-index to {zIndex}");
    }
}