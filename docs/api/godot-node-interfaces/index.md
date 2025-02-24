---
title: "GodotNodeInterfaces Documentation"
description: "Node interfaces and adapters for Godot C# testing and abstraction"
category: "api"
version: "1.0.0"
---

# GodotNodeInterfaces

GodotNodeInterfaces provides interfaces and adapters to facilitate testing and interaction with Godot nodes and scenes.

## Overview

GodotNodeInterfaces helps you:
- Create testable Godot nodes and scenes
- Mock node behavior in tests
- Provide clean abstractions for node interactions
- Simplify unit testing of Godot code

## Installation

Add the NuGet package to your `.csproj` file:

```xml
<ItemGroup>
  <PackageReference Include="Chickensoft.GodotNodeInterfaces" Version="4.0.0" />
</ItemGroup>
```

## Basic Usage

### 1. Node Interfaces

```csharp
// Define an interface for your node
public interface IPlayerNode : INode3D {
  void Jump();
  void Move(Vector3 direction);
  float Health { get; set; }
}

// Implement the interface in your node
public partial class Player : Node3D, IPlayerNode {
  private float _health = 100f;
  
  public float Health {
    get => _health;
    set {
      _health = value;
      EmitSignal(SignalName.HealthChanged, _health);
    }
  }
  
  public void Jump() {
    // Implementation
  }
  
  public void Move(Vector3 direction) {
    // Implementation
  }
}
```

### 2. Testing with Mocks

```csharp
public class MockPlayer : IPlayerNode {
  public float Health { get; set; } = 100f;
  public bool JumpCalled { get; private set; }
  public Vector3 LastMoveDirection { get; private set; }
  
  public void Jump() {
    JumpCalled = true;
  }
  
  public void Move(Vector3 direction) {
    LastMoveDirection = direction;
  }
  
  // Implement other INode3D members...
}

public class PlayerTests : TestClass {
  private MockPlayer _player;
  private PlayerController _controller;
  
  public override void BeforeEach() {
    _player = new MockPlayer();
    _controller = new PlayerController(_player);
  }
  
  [Test]
  public void JumpsOnCommand() {
    _controller.HandleJumpInput();
    Assert.That(_player.JumpCalled, Is.True);
  }
}
```

## Advanced Features

### 1. Signal Handling

```csharp
public interface IPlayerNode : INode3D {
  ISignal<float> HealthChanged { get; }
  ISignal<Vector3> PositionChanged { get; }
}

public partial class Player : Node3D, IPlayerNode {
  [Signal]
  public delegate void HealthChangedEventHandler(float health);
  
  [Signal]
  public delegate void PositionChangedEventHandler(Vector3 position);
  
  public ISignal<float> HealthChanged => 
    this.GetSignal<float>(SignalName.HealthChanged);
  
  public ISignal<Vector3> PositionChanged =>
    this.GetSignal<Vector3>(SignalName.PositionChanged);
}
```

### 2. Node Property Abstraction

```csharp
public interface IUINode : IControl {
  INodeProperty<bool> Visible { get; }
  INodeProperty<Vector2> Size { get; }
  INodeProperty<string> Text { get; }
}

public partial class UIElement : Control, IUINode {
  public INodeProperty<bool> Visible => 
    this.GetProperty<bool>(nameof(Visible));
  
  public INodeProperty<Vector2> Size =>
    this.GetProperty<Vector2>(nameof(Size));
  
  public INodeProperty<string> Text =>
    this.GetProperty<string>(nameof(Text));
}
```

### 3. Scene Tree Navigation

```csharp
public interface ISceneTree : INode {
  INode GetRoot();
  INode GetNodeOrNull(string path);
  T GetNodeOrNull<T>(string path) where T : class, INode;
}

public class GameWorld {
  private readonly ISceneTree _tree;
  
  public GameWorld(ISceneTree tree) {
    _tree = tree;
  }
  
  public IPlayerNode FindPlayer() {
    return _tree.GetNodeOrNull<IPlayerNode>("/root/World/Player")
      ?? throw new InvalidOperationException("Player not found");
  }
}
```

## Testing Patterns

### 1. Component Testing

```csharp
public class HealthSystemTests : TestClass {
  private MockPlayer _player;
  private MockHealthUI _ui;
  private HealthSystem _system;
  
  public override void BeforeEach() {
    _player = new MockPlayer();
    _ui = new MockHealthUI();
    _system = new HealthSystem(_player, _ui);
  }
  
  [Test]
  public void UpdatesUIOnHealthChange() {
    _player.Health = 50f;
    Assert.That(_ui.LastHealthValue, Is.EqualTo(50f));
  }
}
```

### 2. Scene Testing

```csharp
public class WorldSceneTests : GodotIntegrationTest {
  private IWorldScene _world;
  private MockPlayer _player;
  
  public override async Task BeforeEach() {
    // Load and instance the scene
    _world = await LoadScene<IWorldScene>("res://world.tscn");
    _player = new MockPlayer();
    
    // Replace the real player with mock
    _world.ReplacePlayer(_player);
  }
  
  [Test]
  public void HandlesPlayerSpawn() {
    Assert.That(_player.Position, Is.EqualTo(Vector3.Zero));
    Assert.That(_player.Health, Is.EqualTo(100f));
  }
}
```

### 3. Signal Testing

```csharp
public class SignalTests : TestClass {
  private MockPlayer _player;
  private List<float> _healthValues;
  
  public override void BeforeEach() {
    _player = new MockPlayer();
    _healthValues = new List<float>();
    
    _player.HealthChanged.Connect(health => {
      _healthValues.Add(health);
    });
  }
  
  [Test]
  public void EmitsHealthChangedSignal() {
    _player.Health = 75f;
    _player.Health = 50f;
    
    Assert.That(_healthValues, Is.EqualTo(new[] { 75f, 50f }));
  }
}
```

## Best Practices

1. **Interface Design**
   - Keep interfaces focused and minimal
   - Use composition over inheritance
   - Document interface contracts

2. **Testing**
   - Create specific mocks for testing
   - Test signal connections
   - Verify property changes

3. **Node Abstraction**
   - Abstract common node operations
   - Use properties for state
   - Handle signals consistently

## Common Issues

1. **Signal Connection**
   - Verify signal names match
   - Check delegate signatures
   - Dispose signal connections

2. **Property Access**
   - Ensure property names are correct
   - Handle null values appropriately
   - Check property types

3. **Scene Loading**
   - Verify scene paths
   - Handle loading errors
   - Clean up test scenes

## See Also

- [AutoInject Integration](../auto-inject/index.md)
- [LogicBlocks Integration](../logic-blocks/index.md)
- [Testing Guide](./testing.md) 