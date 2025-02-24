using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Newtonsoft.Json;
using F.Framework.Logging;
using Godot;

namespace F.Framework.Core.SceneTree;

[Meta(typeof(IAutoNode))]
public partial class SceneTreeService : Node, IProvide<SceneTreeService>
{
    private static SceneTreeService? _instance;
    private SceneTreeConfig? _config;
    private readonly Node _root;
    private readonly Dictionary<string, PackedScene> _scenes = new();

    public static SceneTreeService Instance
    {
        get
        {
            if (_instance == null) throw new Exception("SceneTreeService not initialized");
            return _instance;
        }
    }

    SceneTreeService IProvide<SceneTreeService>.Value()
    {
        return this;
    }

    public SceneTreeService(Node root)
    {
        _root = root;
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
            Logger.Game.Err("Failed to load scene tree configuration");
            return;
        }

        Logger.Game.Print("Successfully loaded scene tree configuration");
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

    public void Initialize()
    {
        LoadScenes();
    }

    private void LoadScenes()
    {
        var dir = DirAccess.Open("res://scenes");
        if (dir == null)
        {
            Logger.Game.Err("Failed to load scene tree configuration");
            return;
        }

        LoadScenesInDirectory(dir, "res://scenes");
        Logger.Game.Print("Successfully loaded scene tree configuration");
    }

    private void LoadScenesInDirectory(DirAccess dir, string path)
    {
        dir.ListDirBegin();
        var fileName = dir.GetNext();

        while (!string.IsNullOrEmpty(fileName))
        {
            var fullPath = $"{path}/{fileName}";

            if (dir.CurrentIsDir())
            {
                if (fileName != "." && fileName != "..")
                {
                    var subDir = DirAccess.Open(fullPath);
                    if (subDir != null)
                    {
                        LoadScenesInDirectory(subDir, fullPath);
                    }
                }
            }
            else if (fileName.EndsWith(".tscn"))
            {
                var scene = GD.Load<PackedScene>(fullPath);
                if (scene != null)
                {
                    _scenes[fileName] = scene;
                    Logger.Game.Print($"Loaded scene: {fileName}");
                }
            }

            fileName = dir.GetNext();
        }

        dir.ListDirEnd();
    }

    public PackedScene? GetScene(string sceneName)
    {
        if (_scenes.TryGetValue(sceneName, out var scene))
        {
            return scene;
        }

        Logger.Game.Err($"Scene not found: {sceneName}");
        return null;
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