---
title: "GodotCollections Documentation"
description: "Godot-specific collection utilities and data structures"
category: "api"
version: "1.0.0"
---

# GodotCollections

GodotCollections provides specialized collection utilities and data structures optimized for Godot game development.

## Overview

GodotCollections enhances Godot development with:
- Godot-aware collection types
- Game-specific data structures
- Performance-optimized collections
- Memory-efficient implementations

## Installation

Add the NuGet package to your `.csproj` file:

```xml
<ItemGroup>
  <PackageReference Include="Chickensoft.GodotCollections" Version="4.0.0" />
</ItemGroup>
```

## Basic Usage

### 1. Scene Collection

```csharp
using Chickensoft.GodotCollections;

public class LevelManager : Node {
  private readonly SceneCollection<Level> _levels = new();
  
  public async Task LoadLevel(string levelPath) {
    var level = await _levels.LoadScene<Level>(levelPath);
    AddChild(level);
    level.Initialize();
  }
  
  public void UnloadLevel(Level level) {
    _levels.UnloadScene(level);
    level.QueueFree();
  }
}
```

### 2. Resource Collection

```csharp
public class AssetManager : Node {
  private readonly ResourceCollection _resources = new();
  
  public async Task<T> LoadResource<T>(string path) where T : Resource {
    return await _resources.Load<T>(path);
  }
  
  public void UnloadResource(string path) {
    _resources.Unload(path);
  }
}
```

## Advanced Features

### 1. Pooled Collections

```csharp
public class ParticleSystem {
  private readonly PooledList<Particle> _particles;
  
  public ParticleSystem(int capacity) {
    _particles = new PooledList<Particle>(capacity);
  }
  
  public void EmitParticle(Vector2 position) {
    if (_particles.TryAcquire(out var particle)) {
      particle.Position = position;
      particle.Reset();
    }
  }
  
  public void Update(float delta) {
    _particles.ForEach(particle => {
      particle.Update(delta);
      if (particle.IsDead) {
        _particles.Release(particle);
      }
    });
  }
}
```

### 2. Spatial Collections

```csharp
public class World : Node3D {
  private readonly SpatialGrid<Entity> _entities;
  
  public World(Vector3 size, float cellSize) {
    _entities = new SpatialGrid<Entity>(size, cellSize);
  }
  
  public void AddEntity(Entity entity) {
    _entities.Add(entity.GlobalPosition, entity);
    entity.PositionChanged += OnEntityPositionChanged;
  }
  
  private void OnEntityPositionChanged(Entity entity) {
    _entities.UpdatePosition(entity, entity.GlobalPosition);
  }
  
  public IEnumerable<Entity> GetNearbyEntities(Vector3 position, float radius) {
    return _entities.GetInSphere(position, radius);
  }
}
```

### 3. Scene Tree Collections

```csharp
public class UIManager : Control {
  private readonly NodeCollection<Control> _windows = new();
  
  public void RegisterWindow(Control window) {
    _windows.Add(window);
    window.TreeExiting += () => _windows.Remove(window);
  }
  
  public void HideAllWindows() {
    _windows.ForEach(window => window.Hide());
  }
  
  public T? FindWindow<T>() where T : Control {
    return _windows.FirstOrDefault(w => w is T) as T;
  }
}
```

## Collection Types

### 1. SceneCollection<T>

```csharp
public class GameManager : Node {
  private readonly SceneCollection<GameScene> _scenes = new();
  
  public async Task<T> LoadGameScene<T>(string path) where T : GameScene {
    // Automatically handles scene loading and caching
    var scene = await _scenes.LoadScene<T>(path);
    
    // Track scene state
    scene.TreeEntered += () => OnSceneLoaded(scene);
    scene.TreeExiting += () => OnSceneUnloaded(scene);
    
    return scene;
  }
  
  public void UnloadGameScene(GameScene scene) {
    // Automatically handles cleanup and resource management
    _scenes.UnloadScene(scene);
  }
}
```

### 2. ResourceCollection

```csharp
public class TextureManager {
  private readonly ResourceCollection _textures = new();
  
  public async Task<Texture2D> LoadTexture(string path) {
    // Automatically handles resource loading and caching
    return await _textures.Load<Texture2D>(path);
  }
  
  public void UnloadUnusedTextures() {
    // Automatically handles resource cleanup
    _textures.UnloadUnused();
  }
}
```

### 3. SpatialGrid<T>

```csharp
public class ChunkManager {
  private readonly SpatialGrid<Chunk> _chunks;
  
  public ChunkManager(Vector2 worldSize, float chunkSize) {
    _chunks = new SpatialGrid<Chunk>(worldSize, chunkSize);
  }
  
  public void LoadChunksAround(Vector2 position) {
    var nearbyChunks = _chunks.GetInRadius(position, 2f);
    foreach (var chunk in nearbyChunks) {
      chunk.Load();
    }
  }
  
  public void UnloadDistantChunks(Vector2 position) {
    var chunks = _chunks.GetAll()
      .Where(c => c.Position.DistanceTo(position) > 3f);
    
    foreach (var chunk in chunks) {
      chunk.Unload();
    }
  }
}
```

## Best Practices

1. **Resource Management**
   - Use SceneCollection for scene management
   - Implement proper cleanup in UnloadScene
   - Handle async loading appropriately

2. **Performance Optimization**
   - Use pooled collections for frequent allocations
   - Implement spatial partitioning for large worlds
   - Cache and reuse resources when possible

3. **Memory Management**
   - Unload unused resources
   - Clear collections when not needed
   - Use weak references for cached items

## Common Patterns

### 1. Scene Management

```csharp
public class SceneManager : Node {
  private readonly SceneCollection<GameScene> _scenes = new();
  private readonly Stack<GameScene> _sceneStack = new();
  
  public async Task PushScene(string path) {
    var scene = await _scenes.LoadScene<GameScene>(path);
    
    if (_sceneStack.Count > 0) {
      _sceneStack.Peek().Hide();
    }
    
    AddChild(scene);
    _sceneStack.Push(scene);
  }
  
  public void PopScene() {
    if (_sceneStack.Count > 0) {
      var scene = _sceneStack.Pop();
      _scenes.UnloadScene(scene);
      
      if (_sceneStack.Count > 0) {
        _sceneStack.Peek().Show();
      }
    }
  }
}
```

### 2. Resource Caching

```csharp
public class ResourceCache {
  private readonly ResourceCollection _resources = new();
  private readonly Dictionary<string, WeakReference<Resource>> _cache = new();
  
  public async Task<T> GetResource<T>(string path) where T : Resource {
    if (_cache.TryGetValue(path, out var weakRef)) {
      if (weakRef.TryGetTarget(out var resource) && resource is T typed) {
        return typed;
      }
      _cache.Remove(path);
    }
    
    var newResource = await _resources.Load<T>(path);
    _cache[path] = new WeakReference<Resource>(newResource);
    return newResource;
  }
}
```

## See Also

- [Collections](../collections/index.md)
- [AutoInject Integration](../auto-inject/index.md)
- [Testing Guide](./testing.md) 