namespace F.Game.Connections.Helpers;

public static class PipeSelector
{
    public static ConnectionPipe? SelectPipe(IEnumerable<ConnectionPipe> pipes, Vector2 position, float hoverDistance)
    {
        ConnectionPipe? candidate = null;
        var minDist = float.MaxValue;

        GD.Print($"[PipeSelector Debug] Checking position {position} with hover distance {hoverDistance}");

        foreach (var pipe in pipes)
        {
            if (pipe.SourceBlock == null || pipe.TargetBlock == null)
            {
                GD.Print("[PipeSelector Debug] Skipping pipe with null blocks");
                continue;
            }

            var sourceName = pipe.SourceBlock.Name ?? "unknown";
            var targetName = pipe.TargetBlock.Name ?? "unknown";
            GD.Print($"[PipeSelector Debug] Checking pipe between {sourceName} and {targetName}");

            var curvePoints = pipe.GetCurvePoints();
            if (!curvePoints.Any())
            {
                GD.Print("[PipeSelector Debug] No curve points found for pipe");
                continue;
            }

            foreach (var pt in curvePoints)
            {
                var distance = position.DistanceTo(pt);
                if (distance < minDist)
                {
                    minDist = distance;
                    candidate = pipe;
                    GD.Print(
                        $"[PipeSelector Debug] New best distance: {distance} for pipe between {sourceName} and {targetName}");
                }
            }
        }

        if (minDist <= hoverDistance && candidate != null)
        {
            var sourceName = candidate.SourceBlock?.Name ?? "unknown";
            var targetName = candidate.TargetBlock?.Name ?? "unknown";
            GD.Print(
                $"[PipeSelector Debug] Selected pipe between {sourceName} and {targetName} with distance {minDist}");
            return candidate;
        }

        GD.Print("[PipeSelector Debug] No pipe found within hover distance");
        return null;
    }

    public static ConnectionPipe? GetPipeAtPosition(Vector2 position, List<ConnectionPipe> pipes)
    {
        return SelectPipe(pipes, position, PipeConfig.Interaction.HoverDistance);
    }
}