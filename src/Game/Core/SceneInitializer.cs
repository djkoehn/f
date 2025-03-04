namespace F.Game.Core;

public partial class SceneInitializer : Node
{
    public override void _Ready()
    {
        InitializeZIndices();
    }

    private void InitializeZIndices()
    {
        var main = GetTree().Root.GetNode<Node2D>("Main");
        if (main == null) return;

        // Set background
        if (main.GetNode("Background") is Node2D background)
            SetZIndex(background, ZIndexConfig.Layers.Background);

        // Set connection layer and its children
        var BlockLayer = main.GetNode<Node2D>("GameManager/BlockLayer");
        if (BlockLayer != null) // FIX: Check for null
        {
            SetZIndex(BlockLayer, ZIndexConfig.Layers.Pipes);

            // Set input/output blocks
            if (BlockLayer.GetNode("Input") is Node2D input)
                SetZIndex(input, ZIndexConfig.Layers.PlacedBlock);
            if (BlockLayer.GetNode("Output") is Node2D output)
                SetZIndex(output, ZIndexConfig.Layers.PlacedBlock);
        }

        // Set toolbar
        if (main.GetNode("GameManager/Toolbar") is Node2D toolbar)
            SetZIndex(toolbar, ZIndexConfig.Layers.Toolbar);
    }

    private void SetZIndex(Node2D node, int zIndex)
    {
        node.ZIndex = zIndex;
        node.ZAsRelative = false;
        GD.Print($"Set {node.Name} Z-index to {zIndex}");
    }
}