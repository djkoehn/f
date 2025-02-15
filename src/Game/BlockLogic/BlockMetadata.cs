namespace F.Game.BlockLogic
{
    public class BlockMetadata
    {
        public string Id { get; set; } = "";
        public string ScenePath { get; set; } = "";

        public static BlockMetadata? Create(string blockType)
        {
            switch (blockType.ToLowerInvariant())
            {
                case "add":
                    return new BlockMetadata { Id = "add", ScenePath = "res://scenes/Blocks/Add.tscn" };
                case "input":
                    return new BlockMetadata { Id = "input", ScenePath = "res://scenes/Blocks/Input.tscn" };
                case "output":
                    return new BlockMetadata { Id = "output", ScenePath = "res://scenes/Blocks/Output.tscn" };
                default:
                    return null;
            }
        }
    }
} 