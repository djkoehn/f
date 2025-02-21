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
        public bool HasInputSocket { get; set; } = true;
        public bool HasOutputSocket { get; set; } = true;
        public bool IsStationary { get; set; } = false;
        public bool SpawnOnSpace { get; set; } = false;
        public bool DisplayValue { get; set; } = false;

        private static Dictionary<string, BlockMetadata> _metadataCache = new Dictionary<string, BlockMetadata>();

        public static BlockMetadata? GetMetadata(string blockId)
        {
            if (_metadataCache.Count == 0)
            {
                GD.Print($"[BlockMetadata Debug] Loading metadata for first time, looking for blockId: {blockId}");
                // Load metadata from JSON file using Godot.FileAccess
                string json;
                if (FileAccess.FileExists("res://BlockMetadata.json"))
                {
                    using (var file = FileAccess.Open("res://BlockMetadata.json", FileAccess.ModeFlags.Read))
                    {
                        json = file.GetAsText();
                        GD.Print($"[BlockMetadata Debug] JSON content length: {json.Length}");
                        GD.Print($"[BlockMetadata Debug] First 200 chars of JSON: {json.Substring(0, Math.Min(200, json.Length))}");
                    }
                }
                else
                {
                    GD.PrintErr("[BlockMetadata Debug] BlockMetadata.json not found at res://BlockMetadata.json");
                    return null;
                }

                try
                {
                    BlockMetadataFile? metadataFile = JsonConvert.DeserializeObject<BlockMetadataFile>(json);
                    GD.Print($"[BlockMetadata Debug] Deserialization result: {(metadataFile != null ? "success" : "null")}");
                    GD.Print($"[BlockMetadata Debug] Number of blocks: {metadataFile?.Blocks?.Length ?? 0}");
                    
                    if (metadataFile?.Blocks != null)
                    {
                        foreach (BlockMetadata m in metadataFile.Blocks)
                        {
                            _metadataCache[m.Id.ToLower()] = m;  // Store with lowercase ID
                            GD.Print($"[BlockMetadata Debug] Cached block - Id: {m.Id.ToLower()}, Name: {m.Name}, SpawnOnSpace: {m.SpawnOnSpace}");
                        }
                    }
                    else
                    {
                        GD.PrintErr("[BlockMetadata Debug] Blocks array is null after deserialization");
                    }
                }
                catch (Exception e)
                {
                    GD.PrintErr($"[BlockMetadata Debug] Error deserializing metadata: {e.Message}");
                    GD.PrintErr($"[BlockMetadata Debug] Stack trace: {e.StackTrace}");
                    return null;
                }
            }

            string lookupId = blockId.ToLower();  // Convert lookup ID to lowercase
            GD.Print($"[BlockMetadata Debug] Looking up metadata for blockId: {lookupId}");
            GD.Print($"[BlockMetadata Debug] Available cache keys: {string.Join(", ", _metadataCache.Keys)}");

            if (_metadataCache.TryGetValue(lookupId, out BlockMetadata metadata))
            {
                GD.Print($"[BlockMetadata Debug] Retrieved metadata for {lookupId} - Name: {metadata.Name}, SpawnOnSpace: {metadata.SpawnOnSpace}");
                return metadata;
            }
            else
            {
                GD.PrintErr($"[BlockMetadata Debug] No metadata found for blockId: {lookupId}");
                return null;
            }
        }
    }

    class BlockMetadataFile
    {
        public BlockMetadata[]? Blocks { get; set; }
    }
}