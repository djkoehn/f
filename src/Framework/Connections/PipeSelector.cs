namespace F.Framework.Connections;

public static class PipeSelector
{
    public static ConnectionPipe? GetPipeAtPosition(Vector2 position, IEnumerable<ConnectionPipe> pipes)
    {
        ConnectionPipe? candidate = null;
        float minDist = float.MaxValue;

        // GD.Print($"[PipeSelector Debug] Checking position {position} with hover distance {PipeConfig.Interaction.HoverDistance}");

        foreach (var pipe in pipes)
        {
            if (pipe.SourceBlock == null || pipe.TargetBlock == null)
            {
                // GD.Print($"[PipeSelector Debug] Skipping pipe with null blocks");
                continue;
            }

            string sourceName = pipe.SourceBlock.Name ?? "unknown";
            string targetName = pipe.TargetBlock.Name ?? "unknown";
            // GD.Print($"[PipeSelector Debug] Checking pipe between {sourceName} and {targetName}");

            var curvePoints = pipe.GetCurvePoints();
            if (!curvePoints.Any())
            {
                // GD.Print("[PipeSelector Debug] No curve points found for pipe");
                continue;
            }

            foreach (var pt in curvePoints)
            {
                float distance = position.DistanceTo(pt);
                if (distance < minDist)
                {
                    minDist = distance;
                    candidate = pipe;
                    // GD.Print($"[PipeSelector Debug] New best distance: {distance} for pipe between {sourceName} and {targetName}");
                }
            }
        }

        if (minDist <= PipeConfig.Interaction.HoverDistance && candidate != null)
        {
            string sourceName = candidate.SourceBlock?.Name ?? "unknown";
            string targetName = candidate.TargetBlock?.Name ?? "unknown";
            // GD.Print($"[PipeSelector Debug] Selected pipe between {sourceName} and {targetName}");
            return candidate;
        }

        // GD.Print("[PipeSelector Debug] No pipe found within hover distance");
        return null;
    }
}