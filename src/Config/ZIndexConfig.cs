namespace F;

public static class ZIndexConfig
{
    // Global setting for z-index behavior
    public const bool UseRelativeZIndex = false; // Default for non-block elements

    // Helper method to set z-index consistently
    // forceRelative should be true for blocks to ensure proper layering within their parent nodes
    public static void SetZIndex(Node2D node, int zIndex, bool forceRelative = false)
    {
        if (node == null) return;
        node.ZIndex = zIndex;
        node.ZAsRelative = forceRelative || UseRelativeZIndex;
    }

    public static class Layers
    {
        // Background layer (deepest)
        public const int Background = -10;
        public const int BoundsBackground = -9;
        public const int BoundsLens = 0;
        public const int BoundsBorder = 1;

        // Game elements (bottom to top)
        public const int Pipes = -2;
        public const int PlacedBlock = -1;
        public const int Block = -1;
        public const int Token = -2;
        public const int ProcessingToken = -2;

        // UI elements (top)
        public const int Toolbar = 15;
        public const int ToolbarBlock = 20;
        public const int DraggedBlock = 25;
    }
}