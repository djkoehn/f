---
title: LogicBlocks Documentation
description: Guide for using ChickenSoft LogicBlocks in Godot 4.3
keywords: logicblocks, chickensoft, godot, c#, state management, state machine, game development
---

# LogicBlocks Documentation

Welcome to the LogicBlocks documentation. This guide covers ChickenSoft's LogicBlocks integration with Godot 4.3, focusing on state management and block behavior implementation.

## Quick Links

- [Structure](./structure/project_layout.md)
- [Block Patterns](./examples/block_patterns.md)
- [API Reference](./reference/index.md)

## What is LogicBlocks?

LogicBlocks is a state management library for Godot C# that helps organize game logic into discrete, manageable states. It provides:

- Clean state transitions
- Type-safe state management
- Input/Output handling
- Event-driven architecture

### Core Concepts

1. **States**
   - Self-contained behavior units
   - Clear entry/exit points
   - Typed input/output

2. **Logic Classes**
   - State containers
   - Data management
   - Event coordination

3. **Data Flow**
   - Immutable state data
   - Event-based communication
   - Type-safe transitions

## Getting Started

1. Install the LogicBlocks package:
```bash
dotnet add package Chickensoft.LogicBlocks
```

2. Create your first logic class:
```csharp
public partial class GameLogic : Logic<GameLogic.Data>
{
    public record Data(string CurrentState);
    
    protected override Data CreateInitialState()
        => new Data("Initial");
}
```

3. Define states:
```csharp
public partial class PlayingState : State<GameLogic>
{
    public override void OnEnter() 
        => Log.Print("Entered playing state");
}
```

## Key Features

### State Management
- Typed state transitions
- Automatic state tracking
- State history

### Input Handling
- Type-safe input methods
- Input validation
- Event propagation

### Output Events
- Strongly-typed events
- Event subscription
- Output validation

## Best Practices

1. **State Design**
   - Keep states focused
   - Use clear naming
   - Document transitions

2. **Data Management**
   - Use immutable records
   - Validate state changes
   - Track state history

3. **Testing**
   - Unit test states
   - Mock dependencies
   - Test transitions

## Examples

### Basic State Machine
```csharp
public partial class PlayerLogic : Logic<PlayerLogic.Data>
{
    public record Data(string State, int Health);
    
    public partial class IdleState : State<PlayerLogic>
    {
        public override void OnEnter()
            => Log.Print("Player is idle");
    }
    
    public partial class MovingState : State<PlayerLogic>
    {
        public override void OnEnter()
            => Log.Print("Player is moving");
    }
}
```

### Input Handling
```csharp
public partial class GameState : State<GameLogic>
{
    public override void OnInput(PlayerInput input)
    {
        switch (input)
        {
            case { Type: InputType.Move }:
                Logic.ChangeState<MovingState>();
                break;
        }
    }
}
```

## Integration with Godot

LogicBlocks works seamlessly with Godot's node system:

```csharp
public partial class Player : Node
{
    private PlayerLogic _logic;
    
    public override void _Ready()
    {
        _logic = new PlayerLogic();
        _logic.Initialize();
    }
}
```

## See Also

- [LogicBlocks GitHub](https://github.com/chickensoft-games/LogicBlocks)
- [API Documentation](./reference/index.md)
- [Examples](./examples/block_patterns.md) 