namespace F;

public static class SceneNodeConfig
{
    public static class Main
    {
        public const string Root = "/root/Main";
        public const string GameManager = "/root/Main/GameManager";
        public const string Background = "/root/Main/Background";
        public const string HelperFunnel = "/root/Main/HelperFunnel";
    }

    public static class GameManager
    {
        public const string BlockLayer = "/root/Main/GameManager/BlockLayer";
        public const string TokenLayer = "/root/Main/GameManager/TokenLayer";
        public const string Inventory = "/root/Main/GameManager/Inventory";
        public const string BlockInteractionManager = "/root/Main/GameManager/BlockInteractionManager";
        public const string Toolbar = "/root/Main/GameManager/Toolbar";
        public const string TokenManager = "/root/Main/GameManager/TokenManager";
    }

    public static class Toolbar
    {
        public const string ToolbarVisuals = "/root/Main/GameManager/Toolbar/ToolbarVisuals";
        public const string BlockContainer = "/root/Main/GameManager/Toolbar/BlockContainer";
    }

    public static class BlockLayer
    {
        public const string Input = "/root/Main/GameManager/BlockLayer/Input";
        public const string Output = "/root/Main/GameManager/BlockLayer/Output";
        public const string Bounds = "/root/Main/GameManager/BlockLayer/Bounds";
        public const string BlockLayerContent = "/root/Main/GameManager/BlockLayer/BlockLayerViewport/BlockLayerContent";
    }

    public static class HelperFunnel
    {
        public const string DragHelper = "/root/Main/HelperFunnel/DragHelper";
        public const string TweenHelper = "/root/Main/HelperFunnel/TweenHelper";
        public const string ToolbarHelper = "/root/Main/HelperFunnel/ToolbarHelper";
    }

    public static class Audio
    {
        public const string Manager = "/root/Main/AudioManager";
        public const string BlockSoundPlayer = "/root/Main/AudioManager/BlockSoundPlayer";
        public const string TokenSoundPlayer = "/root/Main/AudioManager/TokenSoundPlayer";
    }
} 