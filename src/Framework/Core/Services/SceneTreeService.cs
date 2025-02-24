using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using F.Framework.Core.Services.Interfaces;
using F.Framework.Logging;
using Newtonsoft.Json;

namespace F.Framework.Core.Services;

[Meta(typeof(IAutoNode))]
public partial class SceneTreeService : Node, ISceneTreeService, IProvide<ISceneTreeService>
{
    private static SceneTreeService? _instance;
    private SceneTreeConfig? _config;
    private ILogService? _log;

    public static SceneTreeService Instance
    {
        get
        {
            if (_instance == null) throw new Exception("SceneTreeService not initialized");
            return _instance;
        }
    }

    public SceneTreeService(ILogService? log = null)
    {
        _log = log;
    }

    ISceneTreeService IProvide<ISceneTreeService>.Value()
    {
        return this;
    }

    public override void _Ready()
    {
        if (_log == null)
        {
            _log = GetNode<LogService>("/root/Services/LogService");
        }
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
            _log.Error("Failed to load scene tree configuration");
            return;
        }

        _log.Info("Successfully loaded scene tree configuration");
    }

    public string GetPath(string nodePath)
    {
        if (_config == null) throw new Exception("Scene tree configuration not loaded");

        var parts = nodePath.Split('.');
        var current = _config.Root;
        var path = "/root/Main";

        for (var i = 1; i < parts.Length; i++)
        {
            if (current.Children == null || !current.Children.ContainsKey(parts[i]))
                throw new Exception($"Invalid node path: {nodePath}");

            current = current.Children[parts[i]];
            path += "/" + current.Name;
        }

        return path;
    }

    public override void _ExitTree()
    {
        if (_instance == this) _instance = null;
    }

    public override void _Notification(int what)
    {
        this.Notify(what);
    }
}

public class SceneTreeConfig
{
    [JsonProperty("root")] public SceneNodeConfig Root { get; set; } = new();
}

public class SceneNodeConfig
{
    [JsonProperty("type")] public string Type { get; set; } = "";

    [JsonProperty("name")] public string Name { get; set; } = "";

    [JsonProperty("children")] public Dictionary<string, SceneNodeConfig>? Children { get; set; }
}