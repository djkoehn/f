---
title: "API Documentation"
description: "Detailed API reference for Chickensoft packages"
---

# API Documentation

This document provides detailed API documentation for all Chickensoft packages. Each section covers a specific package, its key types, and their members.

## Table of Contents

- [LogicBlocks](#logicblocks)
- [AutoInject](#autoinject)
- [EventBus](#eventbus)
- [GodotNodeInterfaces](#godotnodeinterfaces)
- [Serialization](#serialization)

## LogicBlocks

### StateMachine<TContext>

Base class for creating state machines.

#### Properties

| Name | Type | Description |
|------|------|-------------|
| `CurrentState` | `State<TContext>` | The currently active state |
| `Context` | `TContext` | The shared context object |
| `IsStarted` | `bool` | Whether the state machine has been started |

#### Methods

| Name | Parameters | Return Type | Description |
|------|------------|-------------|-------------|
| `Start` | `TContext context` | `void` | Starts the state machine with the given context |
| `Stop` | | `void` | Stops the state machine |
| `Update` | | `void` | Updates the current state |
| `Configure` | `IStateConfiguration<TContext> config` | `void` | Configures state transitions |

#### Example Usage

```csharp
public partial class PlayerStateMachine : StateMachine<PlayerContext>
{
    protected override void Configure(IStateConfiguration<PlayerContext> config)
    {
        config.AddTransition(transition => transition
            .From<IdleState>()
            .To<WalkingState>()
            .On<MoveInputEvent>()
        );
    }
}
```

### State<TContext>

Base class for individual states in a state machine.

#### Methods

| Name | Parameters | Return Type | Description |
|------|------------|-------------|-------------|
| `Enter` | `TContext context` | `void` | Called when entering the state |
| `Exit` | `TContext context` | `void` | Called when exiting the state |
| `Update` | `TContext context` | `void` | Called each frame while in this state |

#### Example Usage

```csharp
public partial class IdleState : State<PlayerContext>
{
    public override void Enter(PlayerContext context)
    {
        // Setup for idle state
    }

    public override void Exit(PlayerContext context)
    {
        // Cleanup when leaving idle state
    }

    public override void Update(PlayerContext context)
    {
        // Update logic for idle state
    }
}
```

### IStateConfiguration<TContext>

Interface for configuring state machine transitions.

#### Methods

| Name | Parameters | Return Type | Description |
|------|------------|-------------|-------------|
| `AddTransition` | `Action<ITransitionBuilder<TContext>> builder` | `void` | Adds a new transition |
| `SetInitialState<TState>` | | `void` | Sets the initial state |

### ITransitionBuilder<TContext>

Interface for building state transitions.

#### Methods

| Name | Parameters | Return Type | Description |
|------|------------|-------------|-------------|
| `From<TState>` | | `ITransitionBuilder<TContext>` | Sets the source state |
| `To<TState>` | | `ITransitionBuilder<TContext>` | Sets the target state |
| `On<TEvent>` | | `ITransitionBuilder<TContext>` | Sets the triggering event |
| `When` | `Func<TContext, bool> condition` | `ITransitionBuilder<TContext>` | Adds a guard condition |
| `Do` | `Action<TContext> action` | `ITransitionBuilder<TContext>` | Adds an action to perform during transition |
| `WithPriority` | `int priority` | `ITransitionBuilder<TContext>` | Sets the transition priority |

## AutoInject

### Service

Static class for managing dependency injection.

#### Methods

| Name | Parameters | Return Type | Description |
|------|------------|-------------|-------------|
| `Initialize` | | `void` | Initializes the service container |
| `Reset` | | `void` | Resets the service container |
| `Register<TInterface, TImplementation>` | | `void` | Registers a service |
| `Singleton<TInterface, TImplementation>` | | `void` | Registers a singleton service |
| `Get<T>` | | `T` | Retrieves a service |

#### Example Usage

```csharp
// Registration
Service.Initialize();
Service.Register<IGameService, GameService>();
Service.Singleton<ILogger, FileLogger>();

// Usage
var gameService = Service.Get<IGameService>();
```

### [Inject] Attribute

Attribute for marking properties for injection.

#### Example Usage

```csharp
public partial class GameUI : Control
{
    [Inject] public IGameService GameService { get; set; }
    [Inject] public ILogger Logger { get; set; }
}
```

## EventBus

### EventBus

Static class for event publishing and subscription.

#### Methods

| Name | Parameters | Return Type | Description |
|------|------------|-------------|-------------|
| `Initialize` | | `void` | Initializes the event bus |
| `Dispose` | | `void` | Disposes the event bus |
| `Publish<T>` | `T evt` | `void` | Publishes an event |
| `Subscribe<T>` | `Action<T> handler` | `void` | Subscribes to an event |
| `Unsubscribe<T>` | `Action<T> handler` | `void` | Unsubscribes from an event |

#### Example Usage

```csharp
// Publishing
EventBus.Publish(new GameStartedEvent());

// Subscribing
EventBus.Subscribe<GameStartedEvent>(OnGameStarted);

// Unsubscribing
EventBus.Unsubscribe<GameStartedEvent>(OnGameStarted);
```

## GodotNodeInterfaces

### INode

Interface for Godot Node operations.

#### Properties

| Name | Type | Description |
|------|------|-------------|
| `Name` | `string` | Node name |
| `Owner` | `Node` | Node owner |
| `Parent` | `Node` | Parent node |

#### Methods

| Name | Parameters | Return Type | Description |
|------|------------|-------------|-------------|
| `GetNode` | `NodePath path` | `Node` | Gets a child node |
| `AddChild` | `Node node` | `void` | Adds a child node |
| `RemoveChild` | `Node node` | `void` | Removes a child node |

### INodeAdapter

Adapter for converting between Godot nodes and interfaces.

#### Methods

| Name | Parameters | Return Type | Description |
|------|------------|-------------|-------------|
| `As<T>` | | `T` | Converts node to interface |
| `To<T>` | | `T` | Converts interface to concrete type |

## Serialization

### JsonSerializer

Class for JSON serialization and deserialization.

#### Methods

| Name | Parameters | Return Type | Description |
|------|------------|-------------|-------------|
| `Serialize<T>` | `T value` | `string` | Serializes object to JSON |
| `Deserialize<T>` | `string json` | `T` | Deserializes JSON to object |

#### Example Usage

```csharp
// Serialization
var data = new GameData { Score = 100 };
var json = JsonSerializer.Serialize(data);

// Deserialization
var gameData = JsonSerializer.Deserialize<GameData>(json);
```

### GodotTypeConverters

Converters for Godot-specific types.

#### Supported Types

- `Vector2`
- `Vector3`
- `Color`
- `NodePath`
- `Transform2D`
- `Transform3D`

#### Example Usage

```csharp
// Register converters
GodotTypeConverters.Register();

// Serialize Godot types
var position = new Vector2(100, 200);
var json = JsonSerializer.Serialize(position);
```

## Additional Resources

- [LogicBlocks Documentation](./examples/logic_blocks.md)
- [AutoInject Documentation](./examples/auto_inject.md)
- [EventBus Documentation](./examples/event_bus.md)
- [Serialization Documentation](./examples/serialization.md)

## Version Compatibility

| Package | Godot Version | .NET Version |
|---------|--------------|--------------|
| LogicBlocks | 4.x | 6.0+ |
| AutoInject | 4.x | 6.0+ |
| EventBus | 4.x | 6.0+ |
| GodotNodeInterfaces | 4.x | 6.0+ |
| Serialization | 4.x | 6.0+ |

## Best Practices

1. Always initialize services and event bus in your main game node
2. Clean up event subscriptions when nodes are removed
3. Use interfaces for better testability
4. Keep state machines focused and single-purpose
5. Use dependency injection for better modularity

## Common Issues and Solutions

### Service Resolution Failures

If services fail to resolve:
1. Check that `Service.Initialize()` was called
2. Verify service registration
3. Ensure correct interface/implementation types

### Event Subscription Issues

If events aren't being received:
1. Verify `EventBus.Initialize()` was called
2. Check subscription/unsubscription timing
3. Ensure event types match exactly

### State Machine Problems

If state machines aren't working correctly:
1. Verify state machine is started
2. Check transition conditions
3. Ensure states are properly configured

## Contributing

For information about contributing to Chickensoft packages, see the [Contributing Guide](./contributing.md). 