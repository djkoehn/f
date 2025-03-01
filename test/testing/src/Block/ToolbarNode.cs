namespace testing.Block;

using Godot;

public partial class ToolbarNode : Control
{
    private ColorRect _background;

    public override void _Ready()
    {
        _background = new ColorRect
        {
            Color = new Color(0.3f, 0.3f, 0.3f), // Dark grey
            AnchorsPreset = (int)Control.LayoutPreset.FullRect,
        };

        AddChild(_background);

        // Set toolbar to span bottom of screen
        CustomMinimumSize = new Vector2(1920, 200); // Width and height of toolbar
        AnchorBottom = 1.0f;
        AnchorRight = 1.0f;
        SetAnchorsPreset(LayoutPreset.BottomWide, true);
    }

    public Vector2 GetCenterPosition(Vector2 blockSize)
    {
        var rect = GetGlobalRect();
        return new Vector2(
            rect.Position.X + (rect.Size.X - blockSize.X) / 2,
            rect.Position.Y + (rect.Size.Y - blockSize.Y) / 2
        );
    }
}
