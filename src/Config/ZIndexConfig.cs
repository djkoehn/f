namespace F;

public static class ZIndexConfig
{
    public static class Layers
    {
        public const int Background = -10; // MAKE DEEPER
        public const int Pipes = -5; // PIPES BETWEEN BACKGROUND AND BLOCKS
        public const int PlacedBlock = 0; // BLOCKS ON GAME LAYER
        public const int Block = 0; // REGULAR BLOCKS
        public const int DraggedBlock = 25; // BLOCKS BEING DRAGGED
        public const int Toolbar = 15; // TOOLBAR ON TOP
        public const int ToolbarBlock = 20; // BLOCKS IN TOOLBAR
    }
}