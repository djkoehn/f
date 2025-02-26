namespace F.Game.Core;

public partial class BlockLayer : Node2D, IBlockContainer
{
    public void UpdateBlockPositions()
    {
        // In BlockLayer, blocks can be positioned anywhere, so we don't need to
        // enforce any specific positioning logic. Each block maintains its own position.
    }

    public Vector2 GetNextBlockPosition()
    {
        // In BlockLayer, blocks can be positioned anywhere, so we return the center
        // of the viewport as a reasonable default position
        var viewport = GetViewport();
        if (viewport != null)
        {
            var rect = viewport.GetVisibleRect();
            return rect.GetCenter();
        }

        return Vector2.Zero;
    }
}