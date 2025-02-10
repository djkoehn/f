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
    
    public static BlockMetadata CreateAdd()
    {
        return new BlockMetadata(
            "Add Block",
            "add",
            "res://scenes/Blocks/Add.tscn",
            "Adds a value to token",
            1
        );
    }
}
