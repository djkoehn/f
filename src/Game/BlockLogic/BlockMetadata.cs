using Newtonsoft.Json;
using Godot;

namespace F.Game.BlockLogic
{
    public class BlockMetadata
    {
        public string Id { get; set; } = "";
        public string ScenePath { get; set; } = "";
        public string ProcessTokenScript { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        private static Dictionary<string, BlockMetadata> _metadataCache = new Dictionary<string, BlockMetadata>();

        public static BlockMetadata? GetMetadata(string blockId)
        {
            if (_metadataCache.Count == 0)
            {
                // Load metadata from JSON file using Godot.FileAccess
                string json;
                if (FileAccess.FileExists("res://BlockMetadata.json"))
                {
                    using (var file = FileAccess.Open("res://BlockMetadata.json", FileAccess.ModeFlags.Read))
                    {
                        json = file.GetAsText();
                    }
                }
                else
                {
                    GD.PrintErr("BlockMetadata.json not found at res://BlockMetadata.json");
                    return null;
                }

                BlockMetadataFile? metadataFile = JsonConvert.DeserializeObject<BlockMetadataFile>(json);
                if (metadataFile?.Blocks != null)
                {
                    foreach (BlockMetadata m in metadataFile.Blocks)
                    {
                        _metadataCache[m.Id] = m;
                    }
                }
            }

            if (_metadataCache.TryGetValue(blockId, out BlockMetadata metadata))
            {
                return metadata;
            }
            else
            {
                return null;
            }
        }
    }

    class BlockMetadataFile
    {
        public BlockMetadata[]? Blocks { get; set; }
    }
}