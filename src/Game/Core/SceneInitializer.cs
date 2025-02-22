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
        var gameManager = GetNode<GameManager>(SceneNodeConfig.Main.GameManager);
        if (gameManager == null) return;

        // Get blocks layer (absolute)
        var blockLayer = gameManager.GetNode<Node2D>("BlockLayer");
        if (blockLayer == null) return;

        // Get viewport content
        var viewportContent = blockLayer.GetNode<Node2D>("BlockLayerViewport/BlockLayerContent");
        if (viewportContent == null) return;

        // Set z-indices for blocks layer components
        SetZIndex(blockLayer, ZIndexConfig.Layers.Block);
        SetZIndex(viewportContent, ZIndexConfig.Layers.Block);

        // Get bounds (relative to viewport content)
        if (viewportContent.GetNode("Bounds/Background") is Node2D background)
            SetZIndex(background, ZIndexConfig.Layers.Background);

        if (viewportContent.GetNode("Bounds") is Node2D bounds)
            SetZIndex(bounds, ZIndexConfig.Layers.BoundsBorder);

        // Get input/output blocks (relative to viewport content)
        if (viewportContent.GetNode("Input") is Node2D input)
            SetZIndex(input, ZIndexConfig.Layers.PlacedBlock, true); // Force relative for blocks

        if (viewportContent.GetNode("Output") is Node2D output)
            SetZIndex(output, ZIndexConfig.Layers.PlacedBlock, true); // Force relative for blocks

        // Set toolbar (absolute)
        if (gameManager.GetNode("Toolbar") is Node2D toolbar)
            SetZIndex(toolbar, ZIndexConfig.Layers.Toolbar);
    }

    private void SetZIndex(Node2D node, int zIndex, bool forceRelative = false)
    {
        if (node == null) return;
        ZIndexConfig.SetZIndex(node, zIndex, forceRelative);
    }
}