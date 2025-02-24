using F.Framework.Input;
using F.Framework.Logging;
using Godot;

namespace F.Framework.Core;

public partial class SceneInitializer : Node
{
    public override void _Ready()
    {
        InputActions.ConfigureInputActions();
        // Defer initialization to ensure GameManager is fully ready
        CallDeferred(nameof(InitializeZIndices));
    }

    private void InitializeZIndices()
    {
        // Get the main scene
        var main = GetNode<Node2D>("/root/Main");
        if (main == null)
        {
            Logger.Game.Err("Failed to get Main scene");
            return;
        }

        // Get blocks layer (absolute)
        var blockLayer = main.GetNode<Node2D>("BlockLayer");
        if (blockLayer == null) return;

        // Set z-indices for blocks layer components
        SetZIndex(blockLayer, ZIndexConfig.Layers.Block);

        // Get bounds (relative to viewport content)
        if (blockLayer.GetNode("Bounds/Background") is Node2D background)
            SetZIndex(background, ZIndexConfig.Layers.Background);

        if (blockLayer.GetNode("Bounds") is Node2D bounds)
            SetZIndex(bounds, ZIndexConfig.Layers.BoundsBorder);

        // Get input/output blocks (relative to viewport content)
        if (blockLayer.GetNode("Input") is Node2D input)
            SetZIndex(input, ZIndexConfig.Layers.PlacedBlock, true); // Force relative for blocks

        if (blockLayer.GetNode("Output") is Node2D output)
            SetZIndex(output, ZIndexConfig.Layers.PlacedBlock, true); // Force relative for blocks

        // Set toolbar (absolute)
        if (main.GetNode("Toolbar") is Node2D toolbar)
            SetZIndex(toolbar, ZIndexConfig.Layers.Toolbar);

        Logger.Game.Print("Scene initialization complete");
    }

    private void SetZIndex(Node2D node, int zIndex, bool forceRelative = false)
    {
        if (node == null) return;
        ZIndexConfig.SetZIndex(node, zIndex, forceRelative);
    }
}