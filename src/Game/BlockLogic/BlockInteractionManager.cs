using F.Game.Toolbar;
using F.Game.Core;
using Godot;
using F.Framework.Blocks;
using F.Framework.Core;

namespace F.Game.BlockLogic;

public partial class BlockInteractionManager : Node, IBlockInteractionManager
{
    public override void _Ready()
    {
        base._Ready();
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Process(double delta)
    {
        if (Services.Instance?.Connections == null) return;

        try
        {
            Services.Instance.Connections.ClearAllHighlights();
        }
        catch (NullReferenceException)
        {
            // Ignore null reference exceptions during initialization
            return;
        }
    }

    // Public method to get a block at a given position
    public BaseBlock? GetBlockAtPosition(Vector2 position)
    {
        BaseBlock? closestBlock = null;
        float closestDistance = 50.0f; // picking threshold in pixels
        foreach (Node node in GetTree().GetNodesInGroup("Blocks"))
        {
            if (node is BaseBlock block)
            {
                float distance = block.GlobalPosition.DistanceTo(position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestBlock = block;
                }
            }
        }
        return closestBlock;
    }
}