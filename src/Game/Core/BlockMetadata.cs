namespace F.Game.Core;

public class BlockMetadata
{
    private BlockMetadata(string name, string id, string scenePath, string description, Func<int, int> operation)
    {
        Name = name;
        Id = id;
        // Ensure scene path starts with res://
        ScenePath = scenePath.StartsWith("res://") ? scenePath : $"res://{scenePath}";
        Description = description;
        Operation = operation;
    }

    public string Name { get; }
    public string Id { get; }
    public string ScenePath { get; }
    public string Description { get; }
    public Func<int, int> Operation { get; }
    public string Action => Id; // Use Id as the action identifier

    public static BlockMetadata? Create(string id)
    {
        return id switch
        {
            "add" => new BlockMetadata(
                "Add Block",
                "add",
                "res://scenes/Blocks/Add.tscn",
                "Adds a value to token",
                value => value + 1
            ),
            "multiply" => new BlockMetadata(
                "Multiply Block",
                "multiply",
                "res://scenes/Blocks/Multiply.tscn",
                "Multiplies token value",
                value => value * 2
            ),
            _ => null
        };
    }
}