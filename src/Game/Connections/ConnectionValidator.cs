using Godot;
using F.Game.BlockLogic;

namespace F.Game.Connections;

public sealed class ConnectionValidator
{
    private readonly ColorRect _bounds;

    public ConnectionValidator(ColorRect bounds)
    {
        _bounds = bounds;
    }

    public bool CanConnect(IBlock from, IBlock to)
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

    // Added static method for validating insertion of a new block between two connected blocks
    public static bool ValidateInsertion(IBlock newBlock, IBlock source, IBlock target)
    {
        if (newBlock == source || newBlock == target) return false;
        if (newBlock.GetType() == source.GetType() || newBlock.GetType() == target.GetType()) return false;
        return true;
    }

    // New method for rewiring that relaxes the type check
    public static bool ValidateInsertionForPipeRewiring(IBlock newBlock, IBlock source, IBlock target)
    {
        if (newBlock == source || newBlock == target) return false;
        return true;
    }
}