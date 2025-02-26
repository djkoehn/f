using ToolbarHoverAnimation = F.UI.Animations.UI.ToolbarHoverAnimation;

namespace F.Game.Toolbar;

public partial class ToolbarVisuals : Control
{
    public void UpdateBlockPositions()
    {
        GD.Print("ToolbarVisuals: Block positions updated.");
    }

    public void StartHoverAnimation(bool show)
    {
        // Get the parent node (the toolbar) and delegate the hover animation to it
        var parentControl = GetParent<Control>();
        if (parentControl != null)
            ToolbarHoverAnimation.Create(parentControl, show);
        else
            GD.PrintErr("ToolbarVisuals: Parent control not found for hover animation.");
    }
}