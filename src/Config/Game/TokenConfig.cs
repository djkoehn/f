namespace F.Config.Game;

public static class TokenConfig
{
    public static class Movement
    {
        public const float MoveSpeed = 300f; // Units per second
    }

    public static class Visual
    {
        public const float GlowWidth = 0.3f; // Shader glow width
        public const float GlowIntensity = 1.2f; // Shader glow intensity
    }

    public static class Animation
    {
        public const float MovementDuration = 0.5f;
    }
}