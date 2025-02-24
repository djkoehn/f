using F.Framework.Logging;
using Godot;

namespace F.Framework.Logging;

public static class SceneTreePrinter
{
    public static void PrintSceneTree(Node root)
    {
        Logger.Game.Print("Starting scene tree analysis...");
        PrintNode(root, "", true);
        Logger.Game.Print("Scene tree analysis complete.");
    }

    private static void PrintNode(Node node, string indent, bool isRoot)
    {
        try
        {
            var path = isRoot
                ? "(Root)"
                : node.IsInsideTree()
                    ? node.GetPath().ToString()
                    : $"(Not in tree) {node.Name} : {node.GetType().Name}";

            Logger.Game.Print($"{indent}Node: {path}");

            // Print basic node info
            Logger.Game.Print($"{indent}  Type: {node.GetType().Name}");
            Logger.Game.Print($"{indent}  Parent: {node.GetParent()?.Name ?? "none"}");
            Logger.Game.Print($"{indent}  Children: {node.GetChildCount()}");

            // Print additional info for Node2D
            if (node is Node2D node2D)
            {
                Logger.Game.Print($"{indent}  Position: {node2D.GlobalPosition}");
                Logger.Game.Print($"{indent}  Z-Index: {node2D.ZIndex}");
            }

            // Recursively print children
            foreach (var child in node.GetChildren())
            {
                PrintNode(child, indent + "  ", false);
            }
        }
        catch (Exception e)
        {
            Logger.Game.Err($"{indent}Error printing node {node.Name}: {e.Message}");
        }
    }
}