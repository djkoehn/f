---
title: "AutoInject Documentation"
description: "Build-time dependency injection for Godot C# projects"
category: "api"
version: "1.0.0"
---

# AutoInject

AutoInject provides build-time dependency injection for Godot C# projects, enabling automatic node binding and lifecycle management.

## Overview

AutoInject simplifies dependency management in Godot projects by:
- Automatically binding nodes at build time
- Managing component lifecycles
- Providing clean dependency injection patterns
- Reducing boilerplate code

## Installation

Add the NuGet package to your `.csproj` file:

```xml
<ItemGroup>
  <PackageReference Include="Chickensoft.AutoInject" Version="4.0.0" />
</ItemGroup>
```

## Basic Usage

### 1. Node Binding

```csharp
using Chickensoft.AutoInject;
using Godot;

[AutoNode]
public partial class Player : Node3D {
  // Automatically bound child nodes
  [Node("AnimationPlayer")]
  private AnimationPlayer _animator = null!;
  
  [Node("Sprite")]
  private Sprite3D _sprite = null!;
  
  // Called when all nodes are bound
  public override void _Ready() {
    _animator.Play("idle");
  }
}
```

### 2. Dependency Injection

```csharp
public interface IPlayerService {
  void Jump();
  void Move(Vector3 direction);
}

[AutoNode]
public partial class Player : Node3D {
  private readonly IPlayerService _playerService;
  
  public Player(IPlayerService playerService) {
    _playerService = playerService;
  }
  
  public override void _Input(InputEvent @event) {
    if (@event.IsActionPressed("jump")) {
      _playerService.Jump();
    }
  }
}
```

## Advanced Features

### 1. Lifecycle Management

```csharp
[AutoNode]
public partial class GameManager : Node {
  private readonly IGameState _gameState;
  private readonly ILogger _logger;
  
  public GameManager(IGameState gameState, ILogger logger) {
    _gameState = gameState;
    _logger = logger;
  }
  
  // Called when node and dependencies are ready
  public override void OnResolved() {
    _logger.Info("Game manager initialized");
    _gameState.StartGame();
  }
  
  // Called before node is destroyed
  public override void OnUnresolving() {
    _gameState.SaveGame();
    _logger.Info("Game manager shutting down");
  }
}
```

### 2. Scene Tree Integration

```csharp
[AutoNode]
public partial class UIManager : Control {
  [Node("MainMenu")]
  private Control _mainMenu = null!;
  
  [Node("PauseMenu")]
  private Control _pauseMenu = null!;
  
  [Node("HUD/HealthBar")]
  private ProgressBar _healthBar = null!;
  
  // Nodes are guaranteed to be bound before OnResolved is called
  public override void OnResolved() {
    _mainMenu.Show();
    _pauseMenu.Hide();
    _healthBar.Value = 100;
  }
}
```

### 3. Custom Node Resolution

```csharp
[AutoNode]
public partial class CustomComponent : Node {
  [Node]
  private Node _customNode = null!;
  
  // Custom node resolution logic
  protected override void OnResolveNode(
    string path,
    Node node,
    NodeAttribute attribute
  ) {
    if (node == _customNode) {
      // Custom initialization
      InitializeCustomNode(node);
    }
  }
  
  private void InitializeCustomNode(Node node) {
    // Custom initialization logic
  }
}
```

## Best Practices

1. **Node Organization**
   - Keep node paths short and descriptive
   - Group related nodes under common parents
   - Use consistent naming conventions

2. **Dependency Management**
   - Use interfaces for better testability
   - Keep dependencies focused and minimal
   - Document required dependencies

3. **Lifecycle Handling**
   - Clean up resources in OnUnresolving
   - Initialize in OnResolved
   - Handle dependencies properly

## Common Patterns

### 1. Service Locator

```csharp
public interface IServiceProvider {
  T Get<T>() where T : class;
}

[AutoNode]
public partial class ServiceLocator : Node {
  private readonly Dictionary<Type, object> _services = new();
  
  public void Register<T>(T service) where T : class {
    _services[typeof(T)] = service;
  }
  
  public T Get<T>() where T : class {
    return (_services[typeof(T)] as T)!;
  }
}
```

### 2. Factory Pattern

```csharp
[AutoNode]
public partial class EnemyFactory : Node {
  private readonly PackedScene _enemyScene;
  private readonly IServiceProvider _services;
  
  public EnemyFactory(IServiceProvider services) {
    _services = services;
    _enemyScene = GD.Load<PackedScene>("res://enemy.tscn");
  }
  
  public Enemy CreateEnemy() {
    var enemy = _enemyScene.Instantiate<Enemy>();
    // AutoInject will handle dependency injection
    return enemy;
  }
}
```

## Troubleshooting

1. **Node Not Found**
   - Verify node path is correct
   - Check if node exists in scene
   - Ensure node type matches

2. **Dependency Resolution**
   - Check if all dependencies are registered
   - Verify dependency interfaces match
   - Look for circular dependencies

3. **Build Issues**
   - Clean and rebuild project
   - Check source generator output
   - Verify attribute usage

## See Also

- [GodotNodeInterfaces](../godot-node-interfaces/index.md)
- [LogicBlocks Integration](../logic-blocks/index.md)
- [Testing Guide](./testing.md) 