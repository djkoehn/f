---
title: "Player Controller Example"
description: "Example implementation of a player controller using Chickensoft"
---

# Player Controller Example

This example demonstrates how to implement a player controller using Chickensoft's state machines and dependency injection. We'll create a simple 2D platformer character with basic movement, jumping, and state management.

## Table of Contents
- [Project Setup](#project-setup)
- [Player States](#player-states)
- [Services](#services)
- [Implementation](#implementation)
- [Testing](#testing)

## Project Setup

1. Create the required directories:
```
Player/
├── States/
│   ├── IdleState.cs
│   ├── WalkingState.cs
│   ├── JumpingState.cs
│   └── FallingState.cs
├── Services/
│   ├── IInputService.cs
│   └── InputService.cs
└── Player.cs
```

2. Install required packages:
```bash
dotnet add package Chickensoft.LogicBlocks
dotnet add package Chickensoft.AutoInject
```

## Player States

### PlayerContext

First, define the shared context for player states:

```csharp
public class PlayerContext
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public float Speed { get; set; } = 300f;
    public float JumpForce { get; set; } = 500f;
    public float Gravity { get; set; } = 980f;
    public bool IsGrounded { get; set; }
}
```

### State Machine

Create the player state machine:

```csharp
public partial class PlayerStateMachine : StateMachine<PlayerContext>
{
    protected override void Configure(IStateConfiguration<PlayerContext> config)
    {
        // Set initial state
        config.SetInitialState<IdleState>();

        // Configure transitions
        config.AddTransition(t => t
            .From<IdleState>()
            .To<WalkingState>()
            .On<MoveInputEvent>()
            .When(context => context.IsGrounded)
        );

        config.AddTransition(t => t
            .From<WalkingState>()
            .To<IdleState>()
            .On<StopInputEvent>()
            .When(context => context.IsGrounded)
        );

        config.AddTransition(t => t
            .From<IdleState>()
            .To<JumpingState>()
            .On<JumpInputEvent>()
            .When(context => context.IsGrounded)
        );

        config.AddTransition(t => t
            .From<WalkingState>()
            .To<JumpingState>()
            .On<JumpInputEvent>()
            .When(context => context.IsGrounded)
        );

        config.AddTransition(t => t
            .From<JumpingState>()
            .To<FallingState>()
            .When(context => context.Velocity.Y >= 0)
        );

        config.AddTransition(t => t
            .From<FallingState>()
            .To<IdleState>()
            .When(context => context.IsGrounded)
        );
    }
}
```

### Individual States

#### IdleState

```csharp
public partial class IdleState : State<PlayerContext>
{
    private readonly IInputService _input;

    public IdleState(IInputService input)
    {
        _input = input;
    }

    public override void Enter(PlayerContext context)
    {
        context.Velocity = Vector2.Zero;
        GD.Print("Entered Idle State");
    }

    public override void Update(PlayerContext context)
    {
        // Check for horizontal movement
        var movement = _input.GetMovementInput();
        if (movement.X != 0)
        {
            EventBus.Publish(new MoveInputEvent());
        }

        // Check for jump input
        if (_input.IsJumpPressed())
        {
            EventBus.Publish(new JumpInputEvent());
        }
    }
}
```

#### WalkingState

```csharp
public partial class WalkingState : State<PlayerContext>
{
    private readonly IInputService _input;

    public WalkingState(IInputService input)
    {
        _input = input;
    }

    public override void Update(PlayerContext context)
    {
        var movement = _input.GetMovementInput();
        
        // Update horizontal velocity
        context.Velocity = new Vector2(
            movement.X * context.Speed,
            context.Velocity.Y
        );

        // Check for stop
        if (movement.X == 0)
        {
            EventBus.Publish(new StopInputEvent());
        }

        // Check for jump
        if (_input.IsJumpPressed())
        {
            EventBus.Publish(new JumpInputEvent());
        }
    }
}
```

#### JumpingState

```csharp
public partial class JumpingState : State<PlayerContext>
{
    private readonly IInputService _input;

    public JumpingState(IInputService input)
    {
        _input = input;
    }

    public override void Enter(PlayerContext context)
    {
        // Apply initial jump force
        context.Velocity = new Vector2(
            context.Velocity.X,
            -context.JumpForce
        );
        GD.Print("Jumped!");
    }

    public override void Update(PlayerContext context)
    {
        // Apply horizontal movement
        var movement = _input.GetMovementInput();
        context.Velocity = new Vector2(
            movement.X * context.Speed,
            context.Velocity.Y
        );

        // Apply gravity
        context.Velocity = new Vector2(
            context.Velocity.X,
            context.Velocity.Y + context.Gravity * (float)GetProcessDeltaTime()
        );
    }
}
```

#### FallingState

```csharp
public partial class FallingState : State<PlayerContext>
{
    private readonly IInputService _input;

    public FallingState(IInputService input)
    {
        _input = input;
    }

    public override void Update(PlayerContext context)
    {
        // Apply horizontal movement
        var movement = _input.GetMovementInput();
        context.Velocity = new Vector2(
            movement.X * context.Speed,
            context.Velocity.Y
        );

        // Apply gravity
        context.Velocity = new Vector2(
            context.Velocity.X,
            context.Velocity.Y + context.Gravity * (float)GetProcessDeltaTime()
        );
    }
}
```

## Services

### IInputService

```csharp
public interface IInputService
{
    Vector2 GetMovementInput();
    bool IsJumpPressed();
}
```

### InputService

```csharp
public class InputService : IInputService
{
    public Vector2 GetMovementInput()
    {
        return new Vector2(
            Input.GetActionStrength("move_right") -
            Input.GetActionStrength("move_left"),
            0
        );
    }

    public bool IsJumpPressed()
    {
        return Input.IsActionJustPressed("jump");
    }
}
```

## Implementation

### Player Node

```csharp
public partial class Player : CharacterBody2D
{
    private PlayerStateMachine _stateMachine;
    private PlayerContext _context;

    public override void _Ready()
    {
        // Initialize services
        Service.Initialize();
        Service.Register<IInputService, InputService>();

        // Initialize context
        _context = new PlayerContext
        {
            Position = Position,
            Speed = 300f,
            JumpForce = 500f,
            Gravity = 980f
        };

        // Initialize state machine
        _stateMachine = new PlayerStateMachine();
        _stateMachine.Start(_context);
    }

    public override void _PhysicsProcess(double delta)
    {
        // Update state machine
        _stateMachine.Update();

        // Update physics
        _context.IsGrounded = IsOnFloor();
        Velocity = _context.Velocity;
        MoveAndSlide();
        _context.Position = Position;
        _context.Velocity = Velocity;
    }

    public override void _ExitTree()
    {
        // Cleanup
        _stateMachine.Stop();
        Service.Reset();
    }
}
```

## Testing

### State Tests

```csharp
public class PlayerStateTests
{
    [Test]
    public void IdleState_WhenMovementInput_TransitionsToWalking()
    {
        // Arrange
        var inputService = new Mock<IInputService>();
        inputService.Setup(s => s.GetMovementInput())
            .Returns(new Vector2(1, 0));

        var context = new PlayerContext { IsGrounded = true };
        var stateMachine = new PlayerStateMachine();
        stateMachine.Start(context);

        // Act
        EventBus.Publish(new MoveInputEvent());

        // Assert
        Assert.That(stateMachine.CurrentState, Is.TypeOf<WalkingState>());
    }

    [Test]
    public void JumpingState_WhenVelocityPositive_TransitionsToFalling()
    {
        // Arrange
        var inputService = new Mock<IInputService>();
        var context = new PlayerContext
        {
            IsGrounded = true,
            Velocity = new Vector2(0, 10)
        };

        var stateMachine = new PlayerStateMachine();
        stateMachine.Start(context);

        // Act
        EventBus.Publish(new JumpInputEvent());
        context.Velocity = new Vector2(0, 10);
        stateMachine.Update();

        // Assert
        Assert.That(stateMachine.CurrentState, Is.TypeOf<FallingState>());
    }
}
```

## Usage

1. Create a new scene with a CharacterBody2D node
2. Attach the Player script
3. Configure input actions in Project Settings:
   - `move_left` (Left Arrow)
   - `move_right` (Right Arrow)
   - `jump` (Space)
4. Add collision shapes and platforms
5. Run the game

## Best Practices

1. Keep states focused on single responsibilities
2. Use the context object to share data between states
3. Inject dependencies for better testing
4. Clean up resources when the node is removed
5. Use events for loose coupling
6. Test state transitions thoroughly

## Common Issues

### State Not Transitioning
- Verify guard conditions are met
- Check event publishing
- Ensure state machine is started

### Physics Issues
- Update context after physics operations
- Use `_PhysicsProcess` for consistent physics
- Check collision layers and masks

### Input Problems
- Verify input actions are configured
- Check input service implementation
- Debug input values with logging

## Next Steps

1. Add animations:
   - Create an animation service
   - Add animation states
   - Handle state-specific animations

2. Enhance movement:
   - Add double jumping
   - Implement wall sliding
   - Add dash ability

3. Improve physics:
   - Add coyote time
   - Implement jump buffering
   - Add variable jump height

## Additional Resources

- [State Machine Documentation](../api.md#logicblocks)
- [Dependency Injection Guide](../api.md#autoinject)
- [Event System Overview](../api.md#eventbus)
- [Testing Guide](../contributing.md#testing-guidelines) 