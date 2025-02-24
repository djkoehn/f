using F.Framework.Blocks;
using F.Framework.Core;
using F.Framework.Core.SceneTree;
using F.Framework.Core.Services;

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
        }
    }

    public BaseBlock? GetBlockAtPosition(Vector2 position)
    {
        return Services.Instance?.Blocks.GetBlockAtPosition(position);
    }
}