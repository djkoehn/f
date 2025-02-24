---
title: "Frequently Asked Questions"
description: "Common questions and answers about using Chickensoft"
---

# Frequently Asked Questions

## General Questions

### What is Chickensoft?
Chickensoft is a collection of C# libraries designed specifically for Godot 4.x game development. It provides tools for state management, dependency injection, event handling, and more, helping you build maintainable and scalable games.

### Why should I use Chickensoft instead of vanilla Godot C#?
Chickensoft adds structure and modern software engineering practices to Godot development. It helps you:
- Manage complex game states with type-safe state machines
- Implement clean architecture with dependency injection
- Create decoupled systems using event-driven patterns
- Write testable code with clear separation of concerns

### Is Chickensoft compatible with Godot 4.x?
Yes, Chickensoft is fully compatible with Godot 4.x and C# (.NET 6.0+). All packages are regularly tested and updated to ensure compatibility.

## State Machines (LogicBlocks)

### How do I handle multiple state machines in one game object?
You can create separate state machines for different aspects of your game object:

```csharp
public partial class Player : Node2D
{
    private MovementStateMachine _movementSM;
    private CombatStateMachine _combatSM;
    private AnimationStateMachine _animationSM;

    public override void _Ready()
    {
        _movementSM = new MovementStateMachine();
        _combatSM = new CombatStateMachine();
        _animationSM = new AnimationStateMachine();
        
        // Start each state machine with its own context
        _movementSM.Start(new MovementContext());
        _combatSM.Start(new CombatContext());
        _animationSM.Start(new AnimationContext());
    }

    public override void _Process(double delta)
    {
        _movementSM.Update();
        _combatSM.Update();
        _animationSM.Update();
    }
}
```

### How do I share data between states?
Use the context object to share data between states:

```csharp
public class PlayerContext
{
    public Vector2 Position { get; set; }
    public float Health { get; set; }
    public Dictionary<string, object> SharedData { get; set; }
}
```

### Can states have dependencies?
Yes, you can inject dependencies into states using constructor injection:

```csharp
public partial class CombatState : State<PlayerContext>
{
    private readonly IWeaponService _weapons;
    private readonly IAnimationService _animations;

    public CombatState(IWeaponService weapons, IAnimationService animations)
    {
        _weapons = weapons;
        _animations = animations;
    }
}
```

## Dependency Injection (AutoInject)

### When should I use singleton services vs regular services?
- Use singletons for global services that should have only one instance:
  ```csharp
  Service.Singleton<IGameManager, GameManager>();
  Service.Singleton<ISaveSystem, SaveSystem>();
  ```
- Use regular registration for services that might need multiple instances:
  ```csharp
  Service.Register<IWeaponService, WeaponService>();
  Service.Register<IEnemyAI, EnemyAI>();
  ```

### How do I handle circular dependencies?
Avoid circular dependencies by:
1. Using events for communication between services
2. Extracting shared functionality into a separate service
3. Using property injection instead of constructor injection

### Can I use AutoInject with existing Godot nodes?
Yes, you can use the `[Inject]` attribute on properties in Godot nodes:

```csharp
public partial class PlayerUI : Control
{
    [Inject] public IGameService GameService { get; set; }
    [Inject] public IPlayerStats PlayerStats { get; set; }
}
```

## Event System (EventBus)

### How do I handle events across different scenes?
The EventBus is global, so events can be published and subscribed to from any scene:

```csharp
// In Scene A
EventBus.Publish(new GameEvent("SceneAEvent"));

// In Scene B
EventBus.Subscribe<GameEvent>(OnGameEvent);
```

### Are events ordered?
Events are processed in the order they are published, but subscribers receive them in subscription order. If you need strict ordering:
1. Use priority-based state transitions
2. Implement a queue system
3. Use sequential event processing

### How do I clean up event subscriptions?
Always unsubscribe in `_ExitTree`:

```csharp
public partial class GameSystem : Node
{
    private readonly List<Action> _cleanupActions = new();

    public override void _Ready()
    {
        void OnGameEvent(GameEvent evt) => HandleEvent(evt);
        EventBus.Subscribe<GameEvent>(OnGameEvent);
        _cleanupActions.Add(() => EventBus.Unsubscribe<GameEvent>(OnGameEvent));
    }

    public override void _ExitTree()
    {
        foreach (var cleanup in _cleanupActions)
        {
            cleanup();
        }
    }
}
```

## Performance

### Are state machines performance-intensive?
No, LogicBlocks state machines are designed to be lightweight:
- State transitions are O(1)
- Update calls only process the active state
- Memory allocation is minimized

### How many events can the EventBus handle?
The EventBus is designed to handle thousands of events per second. However:
- Keep event payloads small
- Unsubscribe from unused events
- Consider batching frequent events

### Does dependency injection impact runtime performance?
AutoInject's impact is minimal:
- Service resolution is cached
- Injection happens once during initialization
- No runtime reflection after setup

## Testing

### How do I test states in isolation?
Use dependency injection and mocking:

```csharp
[Test]
public void TestCombatState()
{
    // Arrange
    var weaponService = new Mock<IWeaponService>();
    var context = new PlayerContext();
    var state = new CombatState(weaponService.Object);

    // Act
    state.Enter(context);

    // Assert
    weaponService.Verify(w => w.PrepareWeapon(), Times.Once);
}
```

### Can I test event handling?
Yes, you can verify event publishing and handling:

```csharp
[Test]
public void TestEventHandling()
{
    // Arrange
    var eventReceived = false;
    EventBus.Subscribe<TestEvent>(evt => eventReceived = true);

    // Act
    EventBus.Publish(new TestEvent());

    // Assert
    Assert.True(eventReceived);
}
```

## Common Issues

### Why aren't my services being injected?
Check that:
1. `Service.Initialize()` was called
2. Services are registered before use
3. Properties have the `[Inject]` attribute
4. Classes are partial (for property injection)

### Why isn't my state machine transitioning?
Verify that:
1. The state machine is started
2. Transition conditions are met
3. Events are being published correctly
4. Guard conditions are evaluating as expected

### Why am I getting multiple event handlers?
Common causes:
1. Not unsubscribing when nodes are removed
2. Subscribing multiple times
3. Scene duplication

## Best Practices

### State Machines
1. Keep states focused and single-purpose
2. Use guard conditions for complex transitions
3. Share data through the context object
4. Clean up resources in state Exit methods

### Dependency Injection
1. Program to interfaces
2. Use constructor injection when possible
3. Keep services focused and cohesive
4. Register services at application startup

### Event System
1. Keep events immutable
2. Use meaningful event names
3. Clean up subscriptions
4. Consider event validation

## Additional Resources

- [API Documentation](./api.md)
- [Examples](./examples/)
- [Troubleshooting Guide](./troubleshooting.md)
- [GitHub Issues](https://github.com/chickensoft-games/issues) 