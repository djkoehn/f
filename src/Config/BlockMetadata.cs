using Godot;

namespace F;

public class BlockMetadata
{
    public string Name { get; }
    public string Id { get; }
    public string ScenePath { get; }
    public string Description { get; }
    public int Rarity { get; }
    public System.Func<float, float> Action { get; }
    
    private BlockMetadata(string name, string id, string scenePath, string description, int rarity, System.Func<float, float> action)
    {
        Name = name;
        Id = id;
        ScenePath = scenePath;
        Description = description;
        Rarity = rarity;
        Action = action;
    }
    
    public static BlockMetadata? Create(string id)
    {
        return id switch
        {
            "add" => new BlockMetadata(
                "Add Block",
                "add",
                "res://scenes/Blocks/Add.tscn",
                "Adds a value to token",
                1,
                (value) => value + 1  // Add 1 to the token value
            ),
            "multiply" => new BlockMetadata(
                "Multiply Block",
                "multiply",
                "res://scenes/Blocks/Multiply.tscn",
                "Multiplies token value",
                1,
                (value) => value * 2  // Multiply token value by 2
            ),
            // Add more block types here as needed
            _ => null
        };
    }
}
