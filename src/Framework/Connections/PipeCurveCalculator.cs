namespace F.Framework.Connections;

public static class PipeCurveCalculator
{
    public static List<Vector2> CalculateCurvePoints(Vector2 start, Vector2 end)
    {
        var points = new List<Vector2>();
        // Using a configurable resolution if available, else default to 20
        var resolution = 20;
        // Optionally: resolution = PipeConfig.Visual.CurveResolution; if defined in your config

        // Calculate control points; these are basic and can be enhanced as needed
        var controlOffset = (end - start).Length() * 0.5f;
        var control1 = start + new Vector2(controlOffset, 0);
        var control2 = end - new Vector2(controlOffset, 0);

        for (var i = 0; i <= resolution; i++)
        {
            var t = (float)i / resolution;
            var pt = CubicBezier(start, control1, control2, end, t);
            points.Add(pt);
        }

        return points;
    }

    private static Vector2 CubicBezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
    {
        var oneMinusT = 1 - t;
        return a * (oneMinusT * oneMinusT * oneMinusT) +
               b * (3 * t * oneMinusT * oneMinusT) +
               c * (3 * t * t * oneMinusT) +
               d * (t * t * t);
    }
}