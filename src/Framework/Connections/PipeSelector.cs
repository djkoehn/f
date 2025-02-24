using F.Framework.Blocks;
using F.Framework.Logging;
using Godot;

namespace F.Framework.Connections;

public static class PipeSelector
{
    public static ConnectionPipe? FindClosestPipe(Vector2 position, IEnumerable<ConnectionPipe> pipes)
    {
        Logger.Connection.Print($"Checking position {position} with hover distance {PipeConfig.Interaction.HoverDistance}");

        ConnectionPipe? closestPipe = null;
        float closestDistance = float.MaxValue;

        foreach (var pipe in pipes)
        {
            if (pipe.FromSocket == null || pipe.ToSocket == null)
            {
                Logger.Connection.Print("Skipping pipe with null blocks");
                continue;
            }

            var sourceName = pipe.FromSocket.Name;
            var targetName = pipe.ToSocket.Name;
            Logger.Connection.Print($"Checking pipe between {sourceName} and {targetName}");

            var curvePoints = pipe.GetCurvePoints();
            if (curvePoints == null || curvePoints.Length == 0)
            {
                Logger.Connection.Print("No curve points found for pipe");
                continue;
            }

            var distance = float.MaxValue;
            foreach (var point in curvePoints)
            {
                distance = Mathf.Min(distance, position.DistanceTo(point));
            }

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPipe = pipe;
                Logger.Connection.Print($"New best distance: {distance} for pipe between {sourceName} and {targetName}");
            }
        }

        if (closestPipe != null && closestDistance <= PipeConfig.Interaction.HoverDistance)
        {
            var sourceName = closestPipe.FromSocket?.Name ?? "unknown";
            var targetName = closestPipe.ToSocket?.Name ?? "unknown";
            Logger.Connection.Print($"Selected pipe between {sourceName} and {targetName}");
            return closestPipe;
        }

        Logger.Connection.Print("No pipe found within hover distance");
        return null;
    }
}