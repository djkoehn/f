using Godot;

namespace F;

public static class AnimConfig
{
    public static class Toolbar
    {
        public const float HoverThreshold = 0.8f;
        public const float AnimationDuration = 0.4f;
        public const float ContainerDelay = 0.05f;
        public const float OffscreenOffset = 256f;
        public const float ContainerOffset = -64f; // Adjusted to center blocks vertically
        public const float BlockScale = 1.0f; // Standard scale for blocks in toolbar
        public const float BlockSpacing = 5.0f; // Horizontal spacing between blocks in toolbar
        public const float DraggedBlockScale = 1.2f; // Scale for blocks when being dragged
    }

    public static class Pipe
    {
        public static readonly Color LineColor = new(0.7f, 0.7f, 1.0f, 0.8f);
        public static readonly Color HoverColor = new(1.0f, 0.7f, 0.2f, 0.8f);
        public const float LineWidth = 5.0f;
        public const float PaddingFactor = 1.5f; // How much to grow the rect beyond the connection points
        public const float HoverDistance = 100f; // Distance for hover detection
        
        // Add animation parameters
        public const float SpringDuration = 1.0f;
        public const float SpringStrength = 1.0f;
        public const int CurveResolution = 24;
    }

    public static class ZIndex
    {
        public const int Toolbar = 0;
        public const int Pipe = 1;
        public const int Block = 2;
        public const int DraggedBlock = 3;
    }
}
