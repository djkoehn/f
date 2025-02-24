using F.Framework.Core.SceneTree;
using ToolbarHoverAnimation = F.UI.Animations.UI.ToolbarHoverAnimation;
using F.Framework.Core.Interfaces;
using F.Framework.Logging;

namespace F.Game.Toolbar;

public partial class ToolbarVisuals : Control, IToolbarVisuals
{
    private ColorRect? _background;

    public override void _Ready()
    {
        base._Ready();
        _background = GetNode<ColorRect>("Background");
    }

    public void UpdateBlockPositions()
    {
        // Update block positions in the toolbar
        // This is called whenever blocks are added/removed
        Logger.UI.Print("Block positions updated.");
    }

    public void StartHoverAnimation(bool show)
    {
        var parent = GetParent<Control>();
        if (parent == null)
        {
            Logger.UI.Err("Parent control not found for hover animation.");
            return;
        }

        // Start the hover animation
        var animation = new ToolbarHoverAnimation(parent, show);
        AddChild(animation);
    }
}