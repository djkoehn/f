using Godot;

namespace F;

public class BlockMetadata
{
    public string Name { get; }
    public string Id { get; }
    public string ScenePath { get; }
    public string Description { get; }
    public int Rarity { get; }
    
    private BlockMetadata(string name, string id, string scenePath, string description, int rarity)
    {
        Name = name;
        Id = id;
        ScenePath = scenePath;
        Description = description;
        Rarity = rarity;
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
                1
            ),
            // Add more block types here as needed:
            // "multiply" => new BlockMetadata("Multiply Block", "multiply", "res://scenes/Blocks/Multiply.tscn", "Multiplies token value", 1),
            // "divide" => new BlockMetadata("Divide Block", "divide", "res://scenes/Blocks/Divide.tscn", "Divides token value", 1),
            _ => null
        };
    }
}
