namespace F;

public static class ToolbarConfig
{
    public static class Block
    {
        public const float Width = 96f; // EXACT BLOCK SIZE!
        public const float Height = 96f;
        public const float Spacing = 64f; // EXACT SPACING!
        public const float YPosition = 128f; // EXACT Y POSITION!
    }

    public static class Animation
    {
        public const float HoverThreshold = 0.8f;
        public const float Duration = 0.3f;
        public const float ShowY = 600f; // Toolbar visible position
        public const float HideY = 1080f; // Toolbar hidden position
        public const float ContainerDelay = 0.05f;
        public const float ReturnDuration = 0.5f;
    }

    public static class Layout
    {
        public const float OffscreenOffset = 256f;
        public const float ContainerOffset = -64f; // Adjusted to center blocks vertically
        public const float BlockSpacing = 10f;
    }

    public static class Visual
    {
        public const float DraggedBlockScale = 1.2f;
    }
}