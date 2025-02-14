namespace F.Game.Connections;

public sealed class ConnectionValidator
{
    private readonly ColorRect _bounds;

    public ConnectionValidator(ColorRect bounds)
    {
        _bounds = bounds;
    }

    public bool CanConnect(BaseBlock from, BaseBlock to)
    {
        if (from == to) return false;
        if (from.GetType() == to.GetType()) return false;

        // Check if the connection would be within bounds
        var fromPos = from.GlobalPosition;
        var toPos = to.GlobalPosition;

        return IsWithinBounds(fromPos) && IsWithinBounds(toPos);
    }

    public bool IsWithinBounds(Vector2 point)
    {
        if (_bounds == null) return true;

        var boundsPos = _bounds.GlobalPosition;
        var boundsSize = _bounds.Size;

        return point.X >= boundsPos.X &&
               point.X <= boundsPos.X + boundsSize.X &&
               point.Y >= boundsPos.Y &&
               point.Y <= boundsPos.Y + boundsSize.Y;
    }
}