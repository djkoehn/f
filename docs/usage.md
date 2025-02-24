---
title: "Basic Usage Guide"
description: "Getting started with Chickensoft's core features"
---

# Basic Usage Guide

This guide covers the fundamental concepts and patterns for building games with Chickensoft. We'll explore state machines, dependency injection, and event handling through practical examples.

## Table of Contents
- [State Machines with LogicBlocks](#state-machines-with-logicblocks)
- [Dependency Injection with AutoInject](#dependency-injection-with-autoinject)
- [Event Handling with EventBus](#event-handling-with-eventbus)
- [Common Patterns and Best Practices](#common-patterns-and-best-practices)

## State Machines with LogicBlocks

### Creating a Basic State Machine

1. Define your state context:
```csharp
public class PlayerContext
{
    public Vector2 Position { get; set; }
    public float Speed { get; set; }
    public bool IsGrounded { get; set; }
}
```

2. Create state classes:
```csharp
public partial class IdleState : State<PlayerContext>
{
    public override void Enter(PlayerContext context)
    {
        GD.Print("Entered Idle State");
    }

    public override void Exit(PlayerContext context)
    {
        GD.Print("Exited Idle State");
    }
}

public partial class WalkingState : State<PlayerContext>
{
    public override void Enter(PlayerContext context)
    {
        GD.Print("Started Walking");
    }

    public override void Exit(PlayerContext context)
    {
        GD.Print("Stopped Walking");
    }
}
```

3. Define your state machine:
```csharp
public partial class PlayerStateMachine : StateMachine<PlayerContext>
{
    protected override void Configure(IStateConfiguration<PlayerContext> config)
    {
        // Define transitions
        config.AddTransition(transition => transition
            .From<IdleState>()
            .To<WalkingState>()
            .On<MoveInputEvent>()
            .When(context => context.IsGrounded)
        );

        config.AddTransition(transition => transition
            .From<WalkingState>()
            .To<IdleState>()
            .On<StopInputEvent>()
        );
    }
}
```

4. Use the state machine in your game:
```csharp
public partial class Player : Node2D
{
    private PlayerStateMachine _stateMachine;
    private PlayerContext _context;

    public override void _Ready()
    {
        _context = new PlayerContext
        {
            Position = Position,
            Speed = 100f,
            IsGrounded = true
        };

        _stateMachine = new PlayerStateMachine();
        _stateMachine.Start(_context);
    }

    public override void _Process(double delta)
    {
        _stateMachine.Update();
    }
}
```

### Advanced State Machine Features

#### Hierarchical States
```csharp
public partial class MovementState : State<PlayerContext>
{
    public partial class GroundedState : State<PlayerContext> { }
    public partial class AirborneState : State<PlayerContext> { }
}
```

#### State with Dependencies
```csharp
public partial class CombatState : State<PlayerContext>
{
    private readonly IWeaponService _weaponService;

    public CombatState(IWeaponService weaponService)
    {
        _weaponService = weaponService;
    }

    public override void Enter(PlayerContext context)
    {
        _weaponService.PrepareWeapon();
    }
}
```

## Dependency Injection with AutoInject

### Service Registration

1. Define your service interface:
```csharp
public interface IGameService
{
    void StartGame();
    void PauseGame();
}
```

2. Implement the service:
```csharp
public class GameService : IGameService
{
    public void StartGame()
    {
        GD.Print("Game Started");
    }

    public void PauseGame()
    {
        GD.Print("Game Paused");
    }
}
```

3. Register the service:
```csharp
public partial class Game : Node
{
    public override void _Ready()
    {
        Service.Initialize();
        Service.Register<IGameService, GameService>();
    }
}
```

### Using Services

1. Constructor injection:
```csharp
public partial class MainMenu : Node
{
    private readonly IGameService _gameService;

    public MainMenu(IGameService gameService)
    {
        _gameService = gameService;
    }

    public void OnStartButtonPressed()
    {
        _gameService.StartGame();
    }
}
```

2. Property injection:
```csharp
public partial class GameUI : Control
{
    [Inject] public IGameService GameService { get; set; }

    public override void _Ready()
    {
        // Properties are injected automatically
    }
}
```

## Event Handling with EventBus

### Defining Events
```csharp
public class PlayerDamagedEvent
{
    public int Damage { get; }
    public Vector2 HitPosition { get; }

    public PlayerDamagedEvent(int damage, Vector2 hitPosition)
    {
        Damage = damage;
        HitPosition = hitPosition;
    }
}
```

### Publishing Events
```csharp
public partial class Enemy : Node2D
{
    public void AttackPlayer(Vector2 position)
    {
        EventBus.Publish(new PlayerDamagedEvent(10, position));
    }
}
```

### Subscribing to Events
```csharp
public partial class HealthSystem : Node
{
    public override void _Ready()
    {
        EventBus.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
    }

    private void OnPlayerDamaged(PlayerDamagedEvent evt)
    {
        GD.Print($"Player took {evt.Damage} damage at {evt.HitPosition}");
    }

    public override void _ExitTree()
    {
        EventBus.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
    }
}
```

## Common Patterns and Best Practices

### State Machine Best Practices

1. Keep states focused:
```csharp
// Good: Single responsibility
public partial class JumpingState : State<PlayerContext>
{
    public override void Enter(PlayerContext context)
    {
        ApplyJumpForce(context);
    }
}

// Bad: Too many responsibilities
public partial class PlayerState : State<PlayerContext>
{
    public override void Enter(PlayerContext context)
    {
        HandleMovement();
        HandleJumping();
        HandleCombat();
        HandleInventory();
    }
}
```

2. Use guard conditions effectively:
```csharp
config.AddTransition(transition => transition
    .From<IdleState>()
    .To<JumpState>()
    .On<JumpInputEvent>()
    .When(context => 
        context.IsGrounded && 
        !context.IsStunned && 
        context.HasJumpEnergy
    )
);
```

### Service Organization

1. Use service interfaces:
```csharp
public interface IInputService
{
    bool IsActionPressed(string action);
    Vector2 GetMovementInput();
}

public interface ISaveService
{
    Task SaveGame();
    Task LoadGame();
}
```

2. Compose services:
```csharp
public class GameManager
{
    private readonly IInputService _input;
    private readonly ISaveService _save;
    private readonly IGameService _game;

    public GameManager(
        IInputService input,
        ISaveService save,
        IGameService game)
    {
        _input = input;
        _save = save;
        _game = game;
    }
}
```

### Event Best Practices

1. Use immutable events:
```csharp
public class GameStateChangedEvent
{
    public GameState OldState { get; }
    public GameState NewState { get; }

    public GameStateChangedEvent(GameState oldState, GameState newState)
    {
        OldState = oldState;
        NewState = newState;
    }
}
```

2. Clean up subscriptions:
```csharp
public partial class UIComponent : Control
{
    private readonly List<Action> _cleanupActions = new();

    public override void _Ready()
    {
        void OnGamePaused(GamePausedEvent evt) => HandlePause();
        EventBus.Subscribe<GamePausedEvent>(OnGamePaused);
        _cleanupActions.Add(() => 
            EventBus.Unsubscribe<GamePausedEvent>(OnGamePaused));
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

## Next Steps

- Check out the [Examples](./examples/) section for more complex scenarios
- Read the [API Documentation](./api.md) for detailed reference
- Learn about [Advanced Features](./advanced.md) for more sophisticated usage
- Visit the [Troubleshooting](./troubleshooting.md) guide if you encounter issues 