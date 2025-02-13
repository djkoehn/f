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
        public const int Background = -3;
        public const int PlacedBlock = 0;    // Blocks in ConnectionLayer that aren't being dragged
        public const int Block = 1;          // Blocks in toolbar
        public const int Toolbar = 2;        // Toolbar itself
        public const int DraggedBlock = 3;   // Any block being dragged
    }

    public static class Pipe
    {
        // Animation
        public const float SpringDuration = 0.3f;
        public const float SpringStrength = 0.3f;
        public const int CurveResolution = 20;
        
        // Interaction
        public const float HoverDistance = 100f;
        
        // Visuals
        public static readonly Color LineColor = new(0.388f, 0.388f, 0.388f);
        public static readonly Color HoverColor = new(0.588f, 0.588f, 0.588f);
    }

    public static class Toolbar
    {
        public const float HoverThreshold = 0.8f;
        public const float AnimationDuration = 0.4f;
        public const float ContainerDelay = 0.05f;
        public const float OffscreenOffset = 256f;
        public const float ContainerOffset = -64f; // Adjusted to center blocks vertically
        public const float BlockSpacing = 10f;
        public const float ReturnAnimationDuration = 0.5f;
        public const float DraggedBlockScale = 1.2f; // Scale for blocks when being dragged
    }
}
