using Godot;
using System;

namespace F.Framework.Core.SceneTree;

public partial class SceneTreePrinter : Node
{
    public override void _Ready()
    {
        // Wait a bit to ensure all nodes are initialized
        var timer = new Timer { OneShot = true, WaitTime = 0.5f };
        AddChild(timer);
        timer.Timeout += () =>
        {
            GD.Print("[SceneTreePrinter] Starting scene tree analysis...");
            PrintNodePaths(GetTree().Root);
            GD.Print("[SceneTreePrinter] Scene tree analysis complete.");
            timer.QueueFree();
        };
        timer.Start();
    }

    private void PrintNodePaths(Node node, string indent = "")
    {
        try
        {
            var path = node.IsInsideTree() ? node.GetPath().ToString() : $"(Not in tree) {node.Name} : {node.GetType().Name}";
            GD.Print($"{indent}[SceneTreePrinter] Node: {path}");

            // Print additional info for debugging
            if (node.IsInsideTree())
            {
                GD.Print($"{indent}  Type: {node.GetType().Name}");
                GD.Print($"{indent}  Parent: {node.GetParent()?.Name ?? "none"}");
                GD.Print($"{indent}  Children: {node.GetChildCount()}");
                if (node is Node2D node2D)
                {
                    GD.Print($"{indent}  Position: {node2D.GlobalPosition}");
                    GD.Print($"{indent}  Z-Index: {node2D.ZIndex}");
                }
            }

            foreach (Node child in node.GetChildren())
            {
                PrintNodePaths(child, indent + "  ");
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"{indent}[SceneTreePrinter] Error printing node {node.Name}: {e.Message}");
        }
    }
}