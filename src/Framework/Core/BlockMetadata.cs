using System.Text.Json;
using F.Framework.Logging;
using Godot;

namespace F.Framework.Core;

public class BlockMetadata : IBlockMetadata
{
    private static readonly Dictionary<string, BlockMetadata> _metadataCache = new();

    public string Id { get; private set; }
    public string Scene { get; private set; }
    public string SpawnHotkey { get; private set; }
    public bool HasInput { get; private set; }
    public bool HasOutput { get; private set; }
    public bool IsToolbarBlock { get; private set; }

    private BlockMetadata(
        string id,
        string scene,
        string spawnHotkey,
        bool hasInput,
        bool hasOutput,
        bool isToolbarBlock)
    {
        Id = id;
        Scene = scene;
        SpawnHotkey = spawnHotkey;
        HasInput = hasInput;
        HasOutput = hasOutput;
        IsToolbarBlock = isToolbarBlock;
    }

    public static BlockMetadata? GetMetadata(string id)
    {
        if (_metadataCache.TryGetValue(id, out var metadata))
        {
            Logger.Block.Print($"Retrieved metadata for block {id} from cache");
            return metadata;
        }

        var jsonPath = $"res://metadata/blocks/{id}.json";
        Logger.Block.Print($"Loading metadata for block {id} from {jsonPath}");

        var jsonContent = FileAccess.GetFileAsString(jsonPath);
        if (string.IsNullOrEmpty(jsonContent))
        {
            Logger.Block.Err($"Failed to load metadata file for block {id}");
            return null;
        }

        Logger.Block.Print($"Loaded JSON content for block {id} (length: {jsonContent.Length})");

        try
        {
            var metadata = LoadMetadataFromJson(id, jsonContent);
            if (metadata != null)
            {
                _metadataCache[id] = metadata;
                Logger.Block.Print($"Cached metadata for block {id}");
            }
            return metadata;
        }
        catch (JsonException e)
        {
            Logger.Block.Err($"Failed to parse metadata JSON for block {id}: {e.Message}");
            return null;
        }
    }

    private static BlockMetadata? LoadMetadataFromJson(string id, string jsonContent)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            var jsonData = JsonSerializer.Deserialize<JsonElement>(jsonContent, options);

            var scene = jsonData.GetProperty("scene").GetString() ?? "";
            var spawnHotkey = jsonData.GetProperty("spawnHotkey").GetString() ?? "";
            var hasInput = jsonData.GetProperty("hasInput").GetBoolean();
            var hasOutput = jsonData.GetProperty("hasOutput").GetBoolean();
            var isToolbarBlock = jsonData.GetProperty("isToolbarBlock").GetBoolean();

            var metadata = new BlockMetadata(
                id,
                scene,
                spawnHotkey,
                hasInput,
                hasOutput,
                isToolbarBlock
            );

            Logger.Block.Print($"Successfully parsed metadata for block {id}");
            return metadata;
        }
        catch (KeyNotFoundException e)
        {
            Logger.Block.Err($"Missing required field in metadata JSON for block {id}: {e.Message}");
            return null;
        }
    }
}

file class BlockMetadataFile
{
    public BlockMetadata[]? Blocks { get; set; }
}