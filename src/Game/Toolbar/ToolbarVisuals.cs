using F.UI.Animations;
using ToolbarHoverAnimation = F.UI.Animations.UI.ToolbarHoverAnimation;
using Godot;
using F.Framework.Core.SceneTree;

namespace F.Game.Toolbar
{
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
            GD.Print("ToolbarVisuals: Block positions updated.");
        }

        public void StartHoverAnimation(bool show)
        {
            // Get the parent node (the toolbar) and delegate the hover animation to it
            var parentControl = GetParent<Control>();
            if (parentControl != null)
            {
                ToolbarHoverAnimation.Create(parentControl, show);
            }
            else
            {
                GD.PrintErr("ToolbarVisuals: Parent control not found for hover animation.");
            }
        }
    }
}