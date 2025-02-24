---
title: "Serialization.Godot Documentation"
description: "Godot-specific serialization utilities for C#"
category: "api"
version: "1.0.0"
---

# Serialization.Godot

Chickensoft.Serialization.Godot provides specialized serialization support for Godot types and resources.

## Overview

The Serialization.Godot library offers:
- Godot type serialization
- Resource serialization
- Scene data persistence
- Type-safe conversions
- Performance optimizations

## Installation

Add the NuGet package to your `.csproj` file:

```xml
<ItemGroup>
  <PackageReference Include="Chickensoft.Serialization.Godot" Version="4.0.0" />
</ItemGroup>
```

## Basic Usage

### 1. Godot Type Serialization

```csharp
using Chickensoft.Serialization.Godot;

public class GodotSerializer {
  private readonly GodotDataSerializer _serializer = new();
  
  public void SaveTransform(Transform3D transform) {
    var data = _serializer.Serialize(transform);
    File.WriteAllBytes("transform.dat", data);
  }
  
  public Transform3D LoadTransform() {
    var data = File.ReadAllBytes("transform.dat");
    return _serializer.Deserialize<Transform3D>(data);
  }
}
```

### 2. Resource Serialization

```csharp
public class ResourceManager {
  private readonly GodotResourceSerializer _serializer = new();
  
  public async Task SaveResource(Resource resource, string path) {
    var data = _serializer.Serialize(resource);
    await File.WriteAllBytesAsync(path, data);
  }
  
  public async Task<T> LoadResource<T>(string path) where T : Resource {
    var data = await File.ReadAllBytesAsync(path);
    return _serializer.Deserialize<T>(data);
  }
}
```

## Advanced Features

### 1. Scene Data Persistence

```csharp
public class SceneSerializer {
  private readonly GodotSceneSerializer _serializer = new();
  
  public void SaveSceneState(Node scene, string path) {
    var state = new SceneState {
      NodePath = scene.GetPath(),
      Properties = GetSerializableProperties(scene),
      Children = GetChildStates(scene)
    };
    
    var data = _serializer.Serialize(state);
    File.WriteAllBytes(path, data);
  }
  
  public void RestoreSceneState(Node scene, string path) {
    var data = File.ReadAllBytes(path);
    var state = _serializer.Deserialize<SceneState>(data);
    
    ApplyProperties(scene, state.Properties);
    RestoreChildStates(scene, state.Children);
  }
}
```

### 2. Custom Type Converters

```csharp
public class Vector3Converter : IGodotTypeConverter<Vector3> {
  public byte[] Serialize(Vector3 value) {
    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream);
    
    writer.Write(value.X);
    writer.Write(value.Y);
    writer.Write(value.Z);
    
    return stream.ToArray();
  }
  
  public Vector3 Deserialize(byte[] data) {
    using var stream = new MemoryStream(data);
    using var reader = new BinaryReader(stream);
    
    return new Vector3(
      reader.ReadSingle(),
      reader.ReadSingle(),
      reader.ReadSingle()
    );
  }
}
```

### 3. Resource References

```csharp
public class ResourceReferenceManager {
  private readonly GodotResourceSerializer _serializer = new();
  private readonly Dictionary<string, WeakReference<Resource>> _cache = new();
  
  public async Task SaveWithReferences(
    Resource resource, 
    string path,
    bool saveReferences = true
  ) {
    var references = new Dictionary<string, Resource>();
    
    if (saveReferences) {
      CollectReferences(resource, references);
      foreach (var (refPath, refResource) in references) {
        await SaveReference(refResource, refPath);
      }
    }
    
    var data = _serializer.SerializeWithReferences(resource, references);
    await File.WriteAllBytesAsync(path, data);
  }
  
  public async Task<T> LoadWithReferences<T>(
    string path
  ) where T : Resource {
    var data = await File.ReadAllBytesAsync(path);
    var references = await LoadReferences(path);
    
    return _serializer.DeserializeWithReferences<T>(data, references);
  }
}
```

## Best Practices

1. **Resource Management**
   - Cache frequently used resources
   - Handle resource dependencies
   - Clean up unused resources

2. **Performance**
   - Use appropriate serialization formats
   - Implement caching strategies
   - Handle large resources efficiently

3. **Type Safety**
   - Use type converters
   - Validate resource types
   - Handle missing resources

## Common Patterns

### 1. Scene Snapshot

```csharp
public class SceneSnapshot {
  private readonly GodotSceneSerializer _serializer = new();
  
  public byte[] CaptureState(Node root) {
    var snapshot = new SceneSnapshot {
      Nodes = CaptureNodes(root),
      Signals = CaptureSignals(root),
      Resources = CaptureResources(root)
    };
    
    return _serializer.Serialize(snapshot);
  }
  
  public void RestoreState(Node root, byte[] state) {
    var snapshot = _serializer.Deserialize<SceneSnapshot>(state);
    
    RestoreNodes(root, snapshot.Nodes);
    RestoreSignals(root, snapshot.Signals);
    RestoreResources(root, snapshot.Resources);
  }
}
```

### 2. Resource Pool

```csharp
public class ResourcePool {
  private readonly GodotResourceSerializer _serializer = new();
  private readonly Dictionary<string, Resource> _loadedResources = new();
  
  public async Task<T> GetResource<T>(string path) where T : Resource {
    if (_loadedResources.TryGetValue(path, out var resource)) {
      return resource as T;
    }
    
    var data = await File.ReadAllBytesAsync(path);
    var loadedResource = _serializer.Deserialize<T>(data);
    _loadedResources[path] = loadedResource;
    
    return loadedResource;
  }
  
  public void UnloadResource(string path) {
    if (_loadedResources.Remove(path, out var resource)) {
      resource.Dispose();
    }
  }
}
```

### 3. Scene Prefab System

```csharp
public class PrefabSystem {
  private readonly GodotSceneSerializer _serializer = new();
  private readonly Dictionary<string, PackedScene> _prefabs = new();
  
  public async Task<PackedScene> CreatePrefab(Node source, string path) {
    var scene = new PackedScene();
    scene.Pack(source);
    
    var data = _serializer.Serialize(scene);
    await File.WriteAllBytesAsync(path, data);
    
    _prefabs[path] = scene;
    return scene;
  }
  
  public async Task<Node> InstantiatePrefab(string path) {
    if (!_prefabs.TryGetValue(path, out var scene)) {
      var data = await File.ReadAllBytesAsync(path);
      scene = _serializer.Deserialize<PackedScene>(data);
      _prefabs[path] = scene;
    }
    
    return scene.Instantiate();
  }
}
```

## See Also

- [Serialization](../serialization/index.md)
- [SaveFileBuilder Integration](../save-file-builder/index.md)
- [Best Practices Guide](../guides/best-practices.md) 