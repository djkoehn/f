using System.Collections.Generic;
using Godot;
using F.Game.Connections;
using System.Linq;

namespace F.Game.Connections.Helpers
{
    public static class PipeSelector
    {
        public static ConnectionPipe? SelectPipe(IEnumerable<ConnectionPipe> pipes, Vector2 position, float hoverDistance)
        {
            ConnectionPipe? candidate = null;
            float minDist = float.MaxValue;

            GD.Print($"[PipeSelector Debug] Checking position {position} with hover distance {hoverDistance}");

            foreach (var pipe in pipes)
            {
                if (pipe.SourceBlock == null || pipe.TargetBlock == null)
                {
                    GD.Print($"[PipeSelector Debug] Skipping pipe with null blocks");
                    continue;
                }

                GD.Print($"[PipeSelector Debug] Checking pipe between {pipe.SourceBlock.GetType()} and {pipe.TargetBlock.GetType()}");
                
                var curvePoints = pipe.GetCurvePoints();
                if (!curvePoints.Any())
                {
                    GD.Print("[PipeSelector Debug] No curve points found for pipe");
                    continue;
                }

                foreach (var pt in curvePoints)
                {
                    float distance = position.DistanceTo(pt);
                    if (distance < minDist)
                    {
                        minDist = distance;
                        candidate = pipe;
                        GD.Print($"[PipeSelector Debug] New best distance: {distance} for pipe between {pipe.SourceBlock.GetType()} and {pipe.TargetBlock.GetType()}");
                    }
                }
            }

            if (minDist <= hoverDistance && candidate != null)
            {
                GD.Print($"[PipeSelector Debug] Selected pipe with distance {minDist}");
                return candidate;
            }

            GD.Print("[PipeSelector Debug] No pipe found within hover distance");
            return null;
        }
    }
} 