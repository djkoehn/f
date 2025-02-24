---
title: "LogicBlocks Documentation"
description: "Comprehensive guide to using LogicBlocks for state management in Godot C# games"
category: "api"
version: "1.0.0"
---

# LogicBlocks

LogicBlocks is a powerful state machine library that enables the creation of hierarchical, serializable state machines in C#. It's specifically designed to work seamlessly with Godot 4.3 and provides robust state management capabilities.

## Overview

LogicBlocks follows a clear pattern:
- **Receives inputs**: Handle external events and triggers
- **Maintains state**: Manages a single state instance
- **Produces outputs**: Generates events for other components to react to

## Core Concepts

### State Machine Basics

1. **States**: Enumerated values representing different states
2. **Transitions**: Rules for moving between states
3. **Events**: Triggers that cause state transitions
4. **Guards**: Conditions that must be met for transitions

### Architecture Benefits

1. **Separation of Concerns**
   - Visual components handle display only
   - Logic blocks manage behavior and state
   - Clean separation makes testing easier

2. **Domain-Driven Design Integration**
   - Logic blocks can share domain repositories
   - React to changes in the domain model
   - Event-bus style synchronization

3. **Visualization**
   - Automatic UML state diagram generation
   - Visual representation of complex state machines
   - Helps with documentation and understanding

## Real-World Examples

### 1. Simple UI State Management

```csharp
public partial class InGameUILogic {
  public record State : StateLogic, IState {
    public State(IContext context) : base(context) {
      var appRepo = context.Get<IAppRepo>();

      OnEnter<State>((previous) => {
        appRepo.NumCoinsCollected.Sync += OnNumCoinsCollected;
        appRepo.NumCoinsAtStart.Sync += OnNumCoinsAtStart;
      });

      OnExit<State>((next) => {
        appRepo.NumCoinsCollected.Sync -= OnNumCoinsCollected;
        appRepo.NumCoinsAtStart.Sync -= OnNumCoinsAtStart;
      });
    }

    public void OnNumCoinsCollected(int numCoinsCollected) {
      Context.Output(new Output.NumCoinsChanged(
        numCoinsCollected, 
        Get<IAppRepo>().NumCoinsAtStart.Value
      ));
    }

    public void OnNumCoinsAtStart(int numCoinsAtStart) {
      Context.Output(new Output.NumCoinsChanged(
        Get<IAppRepo>().NumCoinsCollected.Value, 
        numCoinsAtStart
      ));
    }
  }
}
```

### 2. Binding to Visual Components

```csharp
[Meta(typeof(IAutoNode))]
public partial class InGameUI : Control, IInGameUI {
  private InGameUILogic.IBinding InGameUIBinding { get; set; } = default!;

  public void OnResolved() {
    InGameUIBinding = InGameUILogic.Bind();

    InGameUIBinding
      .Handle<InGameUILogic.Output.NumCoinsChanged>(
        (output) => SetCoinsLabel(
          output.NumCoinsCollected, 
          output.NumCoinsAtStart
        )
      );

    InGameUILogic.Start();
  }

  public void SetCoinsLabel(int coins, int totalCoins) {
    CoinsLabel.Text = $"{coins}/{totalCoins}";
  }
}
```

### 3. Complex Game State Management

For more complex states like player movement or game flow, LogicBlocks supports hierarchical state machines:

```csharp
public class PlayerState : LogicBlock<PlayerState.State> {
  public enum State { 
    Idle,
    Moving,
    Jumping,
    Falling
  }
  
  public PlayerState() : base(State.Idle) {
    OnEnter<State>(state => {
      switch (state) {
        case State.Idle:
          HandleIdle();
          break;
        case State.Moving:
          HandleMoving();
          break;
        case State.Jumping:
          HandleJumping();
          break;
        case State.Falling:
          HandleFalling();
          break;
      }
    });
  }
}
```

## Best Practices

1. **Keep Visual Components Simple**
   - Visual components should only handle display
   - Forward all inputs to the logic block
   - Bind to logic block outputs for updates

2. **Use Domain Repositories**
   - Create repositories for shared game logic
   - Handle complex game rules in repositories
   - Use events to notify state changes

3. **State Machine Design**
   - Keep states focused and single-purpose
   - Use hierarchical states for complex behaviors
   - Consider state machine composition for separate concerns

4. **Testing**
   - Test logic blocks in isolation
   - Mock dependencies and repositories
   - Verify state transitions and outputs

## Visualization

LogicBlocks includes a source generator that creates UML state diagrams from your code:

1. **Enable Diagram Generation**
```csharp
[Meta, LogicBlock(typeof(State), Diagram = true)]
public class GameState : LogicBlock<GameState.State> {
  // Your state machine implementation
}
```

2. **View Generated Diagrams**
   - Look for `.g.puml` files next to your code
   - Use PlantUML or VSCode PlantUML extension to view
   - Diagrams update automatically when code changes

## Integration with Domain Logic

1. **Repository Pattern**
```csharp
public class AppRepo : IAppRepo {
  // Game state and rules
  public IObservable<int> NumCoinsCollected { get; }
  public IObservable<int> NumCoinsAtStart { get; }
  
  // Game logic methods
  public bool CollectCoin(int coinId) {
    // Implementation
  }
}
```

2. **State Machine Integration**
```csharp
public class CoinLogic : LogicBlock<CoinLogic.State> {
  private readonly IAppRepo _appRepo;
  
  public CoinLogic(IAppRepo appRepo) {
    _appRepo = appRepo;
  }
  
  public void OnCollected() {
    if (_appRepo.CollectCoin(Id)) {
      GoTo(State.Collected);
    }
  }
}
```

## Troubleshooting

1. **State Not Transitioning**
   - Check guard conditions
   - Verify event handlers are registered
   - Ensure state is properly initialized

2. **Memory Leaks**
   - Properly dispose of state machines
   - Unsubscribe from events
   - Clear handlers when needed

3. **Debugging**
```csharp
// Enable debug logging
LogicBlock.SetLogLevel(LogLevel.Debug);

// Generate state diagram
await StateMachineVisualizer.GenerateDiagram(myStateMachine);
```

## See Also

- [LogicBlocks.DiagramGenerator](../logic-blocks-diagram-generator/index.md)
- [LogicBlocks.Generator](../logic-blocks-generator/index.md)
- [AutoInject Integration](../auto-inject/index.md) 