---
title: "Advanced State Management"
description: "Advanced features and patterns for LogicBlocks state machines"
category: "guides"
version: "1.0.0"
---

# Advanced State Management

This guide covers advanced features and patterns for LogicBlocks state machines.

## Hierarchical States

LogicBlocks supports hierarchical state machines (HSMs) which allow states to contain other states. This is particularly useful for complex game logic.

### Example: Player State Machine

```csharp
[Meta]
public partial class PlayerLogic : LogicBlock<PlayerLogic.State> {
  public enum State {
    Grounded,
    Airborne
  }

  public static class Input {
    public readonly record struct Jump;
    public readonly record struct Move(Vector2 Direction);
    public readonly record struct Land;
  }

  public static class Output {
    public readonly record struct MovementUpdated(Vector2 Velocity);
    public readonly record struct AnimationChanged(string AnimationName);
  }

  // Base state with shared functionality
  public abstract record StateLogic : State {
    protected readonly IPlayerPhysics Physics;
    
    protected StateLogic(IContext context) : base(context) {
      Physics = context.Get<IPlayerPhysics>();
    }

    // Shared movement logic
    protected void UpdateMovement(Vector2 direction) {
      var velocity = Physics.CalculateVelocity(direction);
      Context.Output(new Output.MovementUpdated(velocity));
    }
  }

  // Ground states hierarchy
  public abstract record Grounded : StateLogic {
    public record Idle : Grounded,
      IGet<Input.Move>,
      IGet<Input.Jump> {
      public Transition On(Input.Move input) {
        if (input.Direction != Vector2.Zero) {
          return To<Running>();
        }
        UpdateMovement(input.Direction);
        return Stay();
      }

      public Transition On(Input.Jump input) => To<Airborne.Rising>();

      public override void OnEnter() {
        Context.Output(new Output.AnimationChanged("idle"));
      }
    }

    public record Running : Grounded,
      IGet<Input.Move>,
      IGet<Input.Jump> {
      public Transition On(Input.Move input) {
        if (input.Direction == Vector2.Zero) {
          return To<Idle>();
        }
        UpdateMovement(input.Direction);
        return Stay();
      }

      public Transition On(Input.Jump input) => To<Airborne.Rising>();

      public override void OnEnter() {
        Context.Output(new Output.AnimationChanged("run"));
      }
    }
  }

  // Airborne states hierarchy
  public abstract record Airborne : StateLogic {
    public record Rising : Airborne,
      IGet<Input.Move> {
      private float _jumpTime = 0f;

      public Transition On(Input.Move input) {
        UpdateMovement(input.Direction);
        _jumpTime += GetProcessDeltaTime();
        
        if (_jumpTime >= MAX_JUMP_TIME) {
          return To<Falling>();
        }
        return Stay();
      }

      public override void OnEnter() {
        Context.Output(new Output.AnimationChanged("jump"));
      }
    }

    public record Falling : Airborne,
      IGet<Input.Move>,
      IGet<Input.Land> {
      public Transition On(Input.Move input) {
        UpdateMovement(input.Direction);
        return Stay();
      }

      public Transition On(Input.Land input) {
        return input.Direction == Vector2.Zero 
          ? To<Grounded.Idle>() 
          : To<Grounded.Running>();
      }

      public override void OnEnter() {
        Context.Output(new Output.AnimationChanged("fall"));
      }
    }
  }
}
```

## State Data and Context

LogicBlocks allows states to share data through context and state data objects.

### Shared State Data

```csharp
public class PlayerStateData {
  public Vector2 Position { get; set; }
  public Vector2 Velocity { get; set; }
  public float Health { get; set; }
  public Dictionary<string, object> Inventory { get; set; }
}

public partial class PlayerLogic : LogicBlock<PlayerLogic.State, PlayerStateData> {
  // States can access data via Data property
  public abstract record StateLogic : State {
    protected void UpdatePosition(Vector2 movement) {
      Data.Position += movement;
      Data.Velocity = movement / GetProcessDeltaTime();
    }
  }
}
```

### Context Dependencies

```csharp
public interface IPlayerContext {
  IPlayerPhysics Physics { get; }
  IPlayerAnimator Animator { get; }
  IGameWorld World { get; }
}

public class PlayerContext : IPlayerContext {
  public IPlayerPhysics Physics { get; }
  public IPlayerAnimator Animator { get; }
  public IGameWorld World { get; }

  public PlayerContext(
    IPlayerPhysics physics,
    IPlayerAnimator animator,
    IGameWorld world
  ) {
    Physics = physics;
    Animator = animator;
    World = world;
  }
}
```

## State Machine Composition

Complex behaviors can be achieved by composing multiple state machines:

```csharp
public class Player : Node {
  private PlayerLogic _movementLogic;
  private PlayerCombatLogic _combatLogic;
  private PlayerInventoryLogic _inventoryLogic;

  public override void _Ready() {
    var context = new PlayerContext(
      new PlayerPhysics(this),
      new PlayerAnimator(GetNode<AnimationPlayer>("AnimationPlayer")),
      GetNode<GameWorld>("/root/GameWorld")
    );

    _movementLogic = new PlayerLogic(context);
    _combatLogic = new PlayerCombatLogic(context);
    _inventoryLogic = new PlayerInventoryLogic(context);

    // Start all state machines
    _movementLogic.Start();
    _combatLogic.Start();
    _inventoryLogic.Start();
  }
}
```

## Advanced Patterns

### State Guards

```csharp
public record Running : Grounded,
  IGet<Input.Jump> {
  public Transition On(Input.Jump input) {
    if (!CanJump()) {
      return Stay();
    }
    return To<Airborne.Rising>();
  }

  private bool CanJump() {
    return !Physics.IsStunned && 
           Data.Stamina >= JUMP_STAMINA_COST;
  }
}
```

### State Entry/Exit Actions

```csharp
public record Combat : StateLogic {
  public override void OnEnter() {
    // Setup combat mode
    Context.Get<IPlayerAnimator>().SetCombatStance(true);
    Data.MovementSpeed *= COMBAT_SPEED_MODIFIER;
  }

  public override void OnExit() {
    // Cleanup combat mode
    Context.Get<IPlayerAnimator>().SetCombatStance(false);
    Data.MovementSpeed /= COMBAT_SPEED_MODIFIER;
  }
}
```

### State History

```csharp
public class PlayerLogic : LogicBlock<PlayerLogic.State> {
  private Stack<State> _stateHistory = new();

  protected override void OnStateChanged(State previous, State current) {
    _stateHistory.Push(previous);
    if (_stateHistory.Count > MAX_HISTORY) {
      _stateHistory.RemoveAt(0);
    }
  }

  public void RevertToPreviousState() {
    if (_stateHistory.Count > 0) {
      GoTo(_stateHistory.Pop());
    }
  }
}
```

## Best Practices

1. **State Organization**
   - Group related states in hierarchies
   - Use abstract base states for shared behavior
   - Keep state transitions clear and explicit

2. **Context Management**
   - Inject dependencies through context
   - Use interfaces for better testability
   - Keep context focused and minimal

3. **State Data**
   - Use immutable data where possible
   - Document data dependencies
   - Consider using records for state data

4. **Error Handling**
   - Handle invalid transitions gracefully
   - Log state changes for debugging
   - Validate state data integrity

## See Also

- [Testing State Machines](./testing.md)
- [Domain Integration](./domain-integration.md)
- [Real-World Examples](./examples.md) 