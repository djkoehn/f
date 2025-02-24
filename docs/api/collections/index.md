---
title: "Collections Documentation"
description: "Extended collection utilities for C# development"
category: "api"
version: "1.0.0"
---

# Collections

Chickensoft.Collections provides a set of extended collection utilities and data structures to enhance C# development.

## Overview

The Collections library offers:
- Reactive collections with change notifications
- Thread-safe collection operations
- Extended LINQ-style operations
- Specialized data structures

## Installation

Add the NuGet package to your `.csproj` file:

```xml
<ItemGroup>
  <PackageReference Include="Chickensoft.Collections" Version="4.0.0" />
</ItemGroup>
```

## Basic Usage

### 1. Observable Collections

```csharp
using Chickensoft.Collections;

public class InventorySystem {
  private readonly ObservableList<Item> _items = new();
  
  public IReadOnlyObservableList<Item> Items => _items;
  
  public void AddItem(Item item) {
    _items.Add(item);
  }
  
  public void RemoveItem(Item item) {
    _items.Remove(item);
  }
}

// Subscribe to changes
inventorySystem.Items.OnChanged += (sender, args) => {
  switch (args.Action) {
    case ListChangeAction.Add:
      Console.WriteLine($"Added: {args.NewItems[0]}");
      break;
    case ListChangeAction.Remove:
      Console.WriteLine($"Removed: {args.OldItems[0]}");
      break;
  }
};
```

### 2. Thread-Safe Collections

```csharp
public class GameState {
  private readonly ThreadSafeList<Player> _players = new();
  
  public void AddPlayer(Player player) {
    _players.Add(player);
  }
  
  public void UpdatePlayers() {
    _players.ForEach(player => {
      player.Update();
    });
  }
  
  public Player? FindPlayer(string id) {
    return _players.FirstOrDefault(p => p.Id == id);
  }
}
```

## Advanced Features

### 1. Reactive Properties

```csharp
public class Character {
  private readonly ReactiveProperty<int> _health = new(100);
  private readonly ReactiveProperty<Vector2> _position = new();
  
  public IObservable<int> Health => _health;
  public IObservable<Vector2> Position => _position;
  
  public void TakeDamage(int amount) {
    _health.Value = Math.Max(0, _health.Value - amount);
  }
  
  public void Move(Vector2 direction) {
    _position.Value += direction;
  }
}

// Subscribe to changes
character.Health.Subscribe(health => {
  UpdateHealthBar(health);
});
```

### 2. Collection Operations

```csharp
public class EntityManager {
  private readonly ObservableList<Entity> _entities = new();
  
  public IEnumerable<Entity> GetActiveEntities() {
    return _entities.Where(e => e.IsActive)
      .OrderBy(e => e.Priority);
  }
  
  public void UpdateEntities() {
    _entities.ForEachParallel(entity => {
      entity.Update();
    });
  }
  
  public void BatchAddEntities(IEnumerable<Entity> entities) {
    _entities.AddRange(entities);
  }
}
```

### 3. Specialized Collections

```csharp
public class SpatialPartitioning {
  private readonly Grid2D<Entity> _grid;
  
  public SpatialPartitioning(int width, int height, float cellSize) {
    _grid = new Grid2D<Entity>(width, height, cellSize);
  }
  
  public void AddEntity(Entity entity) {
    _grid.Add(entity.Position, entity);
  }
  
  public IEnumerable<Entity> GetNearby(Vector2 position, float radius) {
    return _grid.GetInRadius(position, radius);
  }
}
```

## Collection Types

### 1. ObservableList<T>

```csharp
public class UISystem {
  private readonly ObservableList<UIElement> _elements = new();
  
  public UISystem() {
    _elements.OnChanged += HandleElementsChanged;
  }
  
  private void HandleElementsChanged(
    object sender,
    ListChangeEventArgs<UIElement> args
  ) {
    switch (args.Action) {
      case ListChangeAction.Add:
        foreach (var element in args.NewItems) {
          InitializeElement(element);
        }
        break;
      case ListChangeAction.Remove:
        foreach (var element in args.OldItems) {
          CleanupElement(element);
        }
        break;
    }
  }
}
```

### 2. ThreadSafeList<T>

```csharp
public class NetworkManager {
  private readonly ThreadSafeList<Connection> _connections = new();
  
  public void ProcessMessages() {
    _connections.ForEachParallel(connection => {
      while (connection.HasPendingMessages) {
        var message = connection.ReadMessage();
        ProcessMessage(message);
      }
    });
  }
  
  public void AddConnection(Connection connection) {
    _connections.Add(connection);
  }
  
  public void RemoveConnection(Connection connection) {
    _connections.Remove(connection);
  }
}
```

### 3. Grid2D<T>

```csharp
public class WorldGrid {
  private readonly Grid2D<Tile> _tiles;
  
  public WorldGrid(int width, int height) {
    _tiles = new Grid2D<Tile>(width, height, 1f);
  }
  
  public void SetTile(int x, int y, Tile tile) {
    _tiles[x, y] = tile;
  }
  
  public Tile GetTile(int x, int y) {
    return _tiles[x, y];
  }
  
  public IEnumerable<Tile> GetNeighbors(int x, int y) {
    return _tiles.GetNeighbors(new Vector2(x, y));
  }
}
```

## Best Practices

1. **Collection Selection**
   - Use ObservableList for UI and reactive scenarios
   - Use ThreadSafeList for concurrent access
   - Choose specialized collections for specific needs

2. **Performance Considerations**
   - Minimize collection changes in tight loops
   - Use batch operations when possible
   - Consider memory usage with large collections

3. **Event Handling**
   - Properly unsubscribe from events
   - Handle collection changes efficiently
   - Avoid recursive modifications

## Common Patterns

### 1. Object Pooling

```csharp
public class ObjectPool<T> where T : class, new() {
  private readonly ThreadSafeList<T> _available = new();
  private readonly ThreadSafeList<T> _inUse = new();
  
  public T Acquire() {
    if (_available.Count > 0) {
      var item = _available[0];
      _available.RemoveAt(0);
      _inUse.Add(item);
      return item;
    }
    
    var newItem = new T();
    _inUse.Add(newItem);
    return newItem;
  }
  
  public void Release(T item) {
    if (_inUse.Remove(item)) {
      _available.Add(item);
    }
  }
}
```

### 2. Event Aggregation

```csharp
public class EventAggregator {
  private readonly Dictionary<Type, ObservableList<object>> _handlers = new();
  
  public void Subscribe<T>(Action<T> handler) {
    var type = typeof(T);
    if (!_handlers.ContainsKey(type)) {
      _handlers[type] = new ObservableList<object>();
    }
    _handlers[type].Add(handler);
  }
  
  public void Publish<T>(T @event) {
    var type = typeof(T);
    if (_handlers.TryGetValue(type, out var handlers)) {
      foreach (var handler in handlers.OfType<Action<T>>()) {
        handler(@event);
      }
    }
  }
}
```

## See Also

- [GoDotCollections](../godot-collections/index.md)
- [LogicBlocks Integration](../logic-blocks/index.md)
- [Testing Guide](./testing.md) 