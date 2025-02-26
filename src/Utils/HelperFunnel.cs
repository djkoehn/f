namespace F.Utils;

// HelperFunnel acts as a central container for various helper instances (i.e. a basic DI container).
public partial class HelperFunnel : Node
{
    // Expose helpers as public properties.
    public DragHelper? DragHelper { get; private set; }

    public TweenHelper? TweenHelper { get; private set; }
    // public ConnectionHelper ConnectionHelper { get; } = new ConnectionHelper();
    // Removed ToolbarHelper property because ToolbarHelper is now a static class.
    // public ToolbarHelper? ToolbarHelper { get; private set; } = null;
    // If needed, we can add more helpers here. TweenHelper is static so it is not needed as an instance.
    // e.g. public SomeOtherHelper OtherHelper { get; private set; }

    public override void _Ready()
    {
        // Retrieve helpers using safe casting
        DragHelper = GetNode("DragHelper") as DragHelper;
        if (DragHelper == null) GD.PrintErr("DragHelper not found or not of correct type in HelperFunnel");

        var tweenNode = GetNode("TweenHelper");
        try
        {
            TweenHelper = (TweenHelper)tweenNode;
        }
        catch (InvalidCastException)
        {
            GD.PrintErr(
                "TweenHelper not found or not of correct type in HelperFunnel. Got type: " + tweenNode.GetType());
            TweenHelper = null;
        }

        // ConnectionHelper = GetNode("ConnectionHelper") as ConnectionHelper;
        // if (ConnectionHelper == null) GD.PrintErr("ConnectionHelper not found or not of correct type in HelperFunnel");

        // Removed ToolbarHelper initialization.

        // Initialize additional helpers as needed
        // OtherHelper = GetNode<SomeOtherHelper>("SomeOtherHelper");

        GD.Print("HelperFunnel initialized with all helpers.");
    }

    // This method allows easy retrieval of HelperFunnel from anywhere in the scene
    public static HelperFunnel? GetInstance()
    {
        var tree = Engine.GetMainLoop() as SceneTree;
        if (tree == null)
        {
            GD.PrintErr("Failed to get SceneTree!");
            return null;
        }

        var root = tree.Root;
        if (root == null)
        {
            GD.PrintErr("Failed to get scene root!");
            return null;
        }

        // Look for HelperFunnel as a direct child of Main
        var helperFunnel = root.GetNodeOrNull<HelperFunnel>(SceneNodeConfig.Main.HelperFunnel);
        if (helperFunnel == null)
        {
            GD.PrintErr("Failed to find HelperFunnel! Path: /root/Main/HelperFunnel");
            // Try to print the actual scene tree for debugging
            GD.Print("Scene tree structure:");
            PrintSceneTree(root, 0);
            return null;
        }

        GD.Print("[Debug HelperFunnel] Successfully found HelperFunnel instance");
        return helperFunnel;
    }

    // Helper method to print the scene tree structure for debugging
    private static void PrintSceneTree(Node node, int depth)
    {
        var indent = new string(' ', depth * 2);
        GD.Print($"{indent}- {node.Name} ({node.GetType()})");
        foreach (var child in node.GetChildren())
            if (child is Node childNode)
                PrintSceneTree(childNode, depth + 1);
    }
}