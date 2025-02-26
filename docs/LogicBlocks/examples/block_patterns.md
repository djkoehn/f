---
title: LogicBlocks Patterns
description: Common patterns and examples for using LogicBlocks
keywords: patterns, examples, state machine, godot, c#
---

# LogicBlocks Patterns

This guide demonstrates common patterns and best practices when using LogicBlocks in your Godot C# projects.

## Basic State Machine

The simplest pattern is a basic state machine:

```csharp
public partial class GameLogic : Logic<GameLogic.Data>
{
    public record Data(string CurrentState, int Score);
    
    protected override Data CreateInitialState()
        => new Data("Menu", 0);
        
    public partial class MenuState : State<GameLogic>
    {
        public override void OnEnter()
            => Log.Print("Entered menu state");
            
        public override void OnInput(StartGameInput input)
            => Logic.ChangeState<PlayingState>();
    }
    
    public partial class PlayingState : State<GameLogic>
    {
        public override void OnEnter()
            => Log.Print("Game started");
            
        public override void OnInput(ScoreInput input)
            => Logic.UpdateData(data => data with { Score = data.Score + input.Points });
    }
}
```

## Input/Output Pattern

Handle inputs and outputs with type safety:

```csharp
public partial class PlayerLogic : Logic<PlayerLogic.Data>
{
    public record Data(Vector2 Position, int Health);
    
    // Input definitions
    public record MoveInput(Vector2 Direction);
    public record DamageInput(int Amount);
    
    // Output definitions
    public record PositionChanged(Vector2 NewPosition);
    public record HealthChanged(int NewHealth);
    
    public partial class AliveState : State<PlayerLogic>
    {
        public override void OnInput(MoveInput input)
        {
            var newPosition = Logic.Data.Position + input.Direction;
            Logic.UpdateData(data => data with { Position = newPosition });
            Logic.Output.Emit(new PositionChanged(newPosition));
        }
        
        public override void OnInput(DamageInput input)
        {
            var newHealth = Logic.Data.Health - input.Amount;
            Logic.UpdateData(data => data with { Health = newHealth });
            Logic.Output.Emit(new HealthChanged(newHealth));
            
            if (newHealth <= 0)
                Logic.ChangeState<DeadState>();
        }
    }
}
```

## Hierarchical States

Organize complex state machines hierarchically:

```csharp
public partial class CharacterLogic : Logic<CharacterLogic.Data>
{
    public record Data(string State, bool IsGrounded);
    
    // Base state for all movement states
    public abstract partial class MovementState : State<CharacterLogic>
    {
        protected bool CanJump => Logic.Data.IsGrounded;
        
        public override void OnInput(JumpInput input)
        {
            if (CanJump)
                Logic.ChangeState<JumpingState>();
        }
    }
    
    // Concrete movement states
    public partial class IdleState : MovementState
    {
        public override void OnInput(MoveInput input)
            => Logic.ChangeState<WalkingState>();
    }
    
    public partial class WalkingState : MovementState
    {
        public override void OnInput(StopInput input)
            => Logic.ChangeState<IdleState>();
    }
}
```

## State Data Management

Handle complex state data with immutable records:

```csharp
public partial class InventoryLogic : Logic<InventoryLogic.Data>
{
    public record Item(string Id, int Count);
    public record Data(ImmutableDictionary<string, Item> Items, int Capacity);
    
    protected override Data CreateInitialState()
        => new Data(ImmutableDictionary<string, Item>.Empty, 10);
        
    public partial class DefaultState : State<InventoryLogic>
    {
        public override void OnInput(AddItemInput input)
        {
            if (Logic.Data.Items.Count >= Logic.Data.Capacity)
            {
                Logic.Output.Emit(new InventoryFullError());
                return;
            }
            
            Logic.UpdateData(data =>
            {
                var items = data.Items;
                if (items.TryGetValue(input.ItemId, out var existing))
                {
                    items = items.SetItem(input.ItemId, existing with { Count = existing.Count + 1 });
                }
                else
                {
                    items = items.Add(input.ItemId, new Item(input.ItemId, 1));
                }
                return data with { Items = items };
            });
        }
    }
}
```

## Integration with Godot

Connect LogicBlocks to Godot nodes:

```csharp
public partial class Character : Node
{
    private CharacterLogic _logic;
    private AnimationPlayer _animator;
    
    public override void _Ready()
    {
        _logic = new CharacterLogic();
        _animator = GetNode<AnimationPlayer>("AnimationPlayer");
        
        // Subscribe to state changes
        _logic.OnStateChanged += HandleStateChanged;
        
        // Initialize logic
        _logic.Initialize();
    }
    
    private void HandleStateChanged(IState oldState, IState newState)
    {
        // Update animations based on state
        switch (newState)
        {
            case IdleState:
                _animator.Play("idle");
                break;
            case WalkingState:
                _animator.Play("walk");
                break;
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey eventKey)
        {
            if (eventKey.Pressed && eventKey.Keycode == Key.Space)
            {
                _logic.Input(new JumpInput());
            }
        }
    }
}
```

## Testing

Write clean, maintainable tests:

```csharp
public class PlayerLogicTests
{
    private PlayerLogic _logic;
    
    [SetUp]
    public void Setup()
    {
        _logic = new PlayerLogic();
        _logic.Initialize();
    }
    
    [Test]
    public void MovementUpdatesPosition()
    {
        var initialPos = _logic.Data.Position;
        var moveDirection = new Vector2(1, 0);
        
        _logic.Input(new MoveInput(moveDirection));
        
        Assert.That(_logic.Data.Position, Is.EqualTo(initialPos + moveDirection));
    }
    
    [Test]
    public void DamageChangesStateWhenHealthReachesZero()
    {
        _logic.Input(new DamageInput(_logic.Data.Health));
        
        Assert.That(_logic.CurrentState, Is.TypeOf<DeadState>());
    }
}
```

## Best Practices

1. **State Design**
   - Keep states focused on a single responsibility
   - Use clear, descriptive names for states and inputs
   - Document state transitions

2. **Data Management**
   - Use immutable records for state data
   - Validate state changes
   - Emit events for important changes

3. **Testing**
   - Test state transitions
   - Verify data updates
   - Mock external dependencies

## See Also

- [API Reference](../reference/index.md)
- [State Management](../reference/state_management.md) 