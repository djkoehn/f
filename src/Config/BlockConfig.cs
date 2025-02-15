namespace F.Config
{
    public class BlockConfig
    {
        public string Name { get; set; } = "";
        public float? DefaultValue { get; set; } = null;

        // Added static property for the BlockLayer path
        public static string BlockLayerPath { get; } = "/root/Main/GameManager/BlockLayer";
    }
} 