namespace F.Config.Connection;

public static class PipeConfig
{
    // Animation
    public static class Animation
    {
        public const float SpringDuration = 0.3f;
        public const float SpringStrength = 0.3f;
        public const float ReconnectDuration = 0.3f;
    }

    // Interaction
    public static class Interaction
    {
        public const float HoverDistance = 100f;      // Distance for initial hover detection
        public const float ConnectedDistance = 300f;  // Larger distance for connected blocks
    }

    // Visuals
    public static class Visual
    {
        public const int CurveResolution = 20;
        public static readonly Color LineColor = new(0.388f, 0.388f, 0.388f);
        public static readonly Color HoverColor = new(0.588f, 0.588f, 0.588f);
    }
}