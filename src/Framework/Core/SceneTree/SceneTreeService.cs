using Godot;
using Newtonsoft.Json;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;

namespace F.Framework.Core.SceneTree;

[Meta(typeof(IAutoNode))]
public partial class SceneTreeService : Node, IProvide<SceneTreeService>
{
    private static SceneTreeService? _instance;
    private SceneTreeConfig? _config;

    public static SceneTreeService Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new System.Exception("SceneTreeService not initialized");
            }
            return _instance;
        }
    }

    public override void _Ready()
    {
        _instance = this;
        LoadConfig();
        this.Provide();
    }

    private void LoadConfig()
    {
        var json = FileAccess.GetFileAsString("res://src/Config/SceneTree.json");
        _config = JsonConvert.DeserializeObject<SceneTreeConfig>(json);

        if (_config == null)
        {
            GD.PrintErr("Failed to load scene tree configuration");
            return;
        }

        GD.Print("[SceneTreeService] Successfully loaded scene tree configuration");
    }

    public string GetPath(string nodePath)
    {
        if (_config == null)
        {
            throw new System.Exception("Scene tree configuration not loaded");
        }

        var parts = nodePath.Split('.');
        var current = _config.Root;
        var path = "/root/Main";

        for (int i = 1; i < parts.Length; i++)
        {
            if (current.Children == null || !current.Children.ContainsKey(parts[i]))
            {
                throw new System.Exception($"Invalid node path: {nodePath}");
            }

            current = current.Children[parts[i]];
            path += "/" + current.Name;
        }

        return path;
    }

    public override void _ExitTree()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    public override void _Notification(int what) => this.Notify(what);

    SceneTreeService IProvide<SceneTreeService>.Value() => this;
}

public class SceneTreeConfig
{
    [JsonProperty("root")]
    public SceneNodeConfig Root { get; set; } = new();
}

public class SceneNodeConfig
{
    [JsonProperty("type")]
    public string Type { get; set; } = "";

    [JsonProperty("name")]
    public string Name { get; set; } = "";

    [JsonProperty("children")]
    public Dictionary<string, SceneNodeConfig>? Children { get; set; }
}