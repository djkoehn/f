using F.Framework.Blocks;

namespace F.UI.Animations.UI;

public class ToolbarContainerAnimation
{
    private readonly BaseBlock _block;

    private ToolbarContainerAnimation(BaseBlock block)
    {
        _block = block;
    }

    public static ToolbarContainerAnimation Create(BaseBlock block)
    {
        return new ToolbarContainerAnimation(block);
    }
}