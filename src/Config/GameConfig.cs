using Godot;

namespace F;

public static class GameConfig
{
    // UI Text Sizes
    public const int VALUE_LABEL_SIZE = 70;
    public const int TYPE_LABEL_SIZE = 30;
    
    // Block Dimensions
    public const float BLOCK_SIZE = 200f;  // Base block size
    public const float SOCKET_RADIUS = 5f;  // Socket circle radius
    public const float SOCKET_OFFSET = BLOCK_SIZE / 4f;  // Distance of socket from block center

    // Token Configuration
    public const float TOKEN_MOVE_SPEED = 300f;  // Units per second
    public const float TOKEN_GLOW_WIDTH = 0.3f;  // Shader glow width
    public const float TOKEN_GLOW_INTENSITY = 1.2f;  // Shader glow intensity
}
