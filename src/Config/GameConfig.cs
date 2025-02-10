using Godot;

namespace F;

public static class GameConfig
{
    // UI Text Sizes
    public const int VALUE_LABEL_SIZE = 70;
    public const int TYPE_LABEL_SIZE = 30;
    
    // Block Dimensions
    public const float BLOCK_SIZE = 128f;  // Base block size
    public const float SOCKET_RADIUS = 5f;  // Socket circle radius
    public const float SOCKET_OFFSET = BLOCK_SIZE / 4f;  // Distance of socket from block center
    
    // Block Assets
    public const string DEFAULT_BLOCK_SPRITE = "res://assets/images/untitled.png";
}
