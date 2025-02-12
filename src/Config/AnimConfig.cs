using Godot;

namespace F;

public static class AnimConfig
{
    public static class Block
    {
        public const float DragScale = 1.2f;
        public const float SnapDistance = 50f;
        public const float DraggedBlockScale = 1.2f;
    }

    public static class ZIndex
    {
        // Base layers
        public const int Background = -10;
        public const int Pipe = -1;
        public const int Block = 0;
        public const int Token = 1;
        
        // UI layers
        public const int Toolbar = -1;
        public const int DraggedBlock = 11;
        public const int Overlay = 12;
    }

    public static class Pipe
    {
        // Animation
        public const float SpringDuration = 0.3f;
        public const float SpringStrength = 0.3f;
        public const int CurveResolution = 20;
        
        // Interaction
        public const float HoverDistance = 30f;
        
        // Visuals
        public static readonly Color LineColor = new(0.388f, 0.388f, 0.388f);
        public static readonly Color HoverColor = new(0.588f, 0.588f, 0.588f);
    }

    public static class Token
    {
        public const float MoveDuration = 0.5f;
        public const float Scale = 0.5f;
    }

    public static class UI
    {
        public const float ToolbarHeight = 150f;
        public const float ToolbarAnimationDuration = 0.3f;
        public const float BlockSpacing = 20f;
    }

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
}
