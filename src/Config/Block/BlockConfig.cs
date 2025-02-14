namespace F.Config;

public class BlockConfig
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public float? DefaultValue { get; set; }
    public string ScenePath { get; set; } = "";
    public string Description { get; set; } = "";

    public static class Layout
    {
        public const float Size = 100f;
        public const float SocketSize = 20f;
        public const float SocketOffset = 40f;
    }

    public static class Visual
    {
        public const float ShadowBlur = 2.0f;
        public const float ShadowOpacity = 0.5f;
        public const float ShadowOffset = 10f;
    }

    public static class Interaction
    {
        public const float DragThreshold = 5f;
        public const float SnapThreshold = 30f;
        public const float DraggedBlockScale = 1.2f;
    }
}