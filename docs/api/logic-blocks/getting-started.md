---
title: "Getting Started with LogicBlocks"
description: "Quick start guide for using LogicBlocks in your Godot C# project"
category: "guides"
version: "1.0.0"
---

# Getting Started with LogicBlocks

This guide will help you get started with LogicBlocks in your Godot C# project.

## Installation

1. **Add NuGet Package**

Add the following to your `.csproj` file:

```xml
<ItemGroup>
  <PackageReference Include="Chickensoft.LogicBlocks" Version="4.0.0" />
  <!-- Optional: For state diagram generation -->
  <PackageReference Include="Chickensoft.LogicBlocks.Generator" Version="4.0.0" />
</ItemGroup>
```

2. **Install Dependencies**

```bash
dotnet restore
```

## Basic Usage

### 1. Create Your First Logic Block

```csharp
using Chickensoft.LogicBlocks;

[Meta]
public partial class GameState : LogicBlock<GameState.State> {
  // Define possible states
  public enum State {
    MainMenu,
    Playing,
    Paused
  }

  // Define inputs (events that can trigger state changes)
  public static class Input {
    public readonly record struct StartGame;
    public readonly record struct PauseGame;
    public readonly record struct ResumeGame;
  }

  // Define outputs (events emitted by the state machine)
  public static class Output {
    public readonly record struct GameStarted;
    public readonly record struct GamePaused;
    public readonly record struct GameResumed;
  }

  // Define state implementations
  public abstract record StateLogic : State {
    public record MainMenu : StateLogic, 
      IGet<Input.StartGame> {
      public Transition On(Input.StartGame input) => 
        To<Playing>();
    }

    public record Playing : StateLogic,
      IGet<Input.PauseGame> {
      public Transition On(Input.PauseGame input) =>
        To<Paused>();
    }

    public record Paused : StateLogic,
      IGet<Input.ResumeGame> {
      public Transition On(Input.ResumeGame input) =>
        To<Playing>();
    }
  }

  // Set initial state
  public override Transition GetInitialState() => 
    To<State.MainMenu>();
}
```

### 2. Use in a Godot Node

```csharp
public partial class GameController : Node {
  private GameState _gameState;
  private GameState.IBinding _binding;

  public override void _Ready() {
    _gameState = new GameState();
    
    // Bind to state machine outputs
    _binding = _gameState.Bind()
      .Handle<GameState.Output.GameStarted>(_ => {
        GD.Print("Game started!");
      })
      .Handle<GameState.Output.GamePaused>(_ => {
        GD.Print("Game paused!");
      })
      .Handle<GameState.Output.GameResumed>(_ => {
        GD.Print("Game resumed!");
      });

    // Start the state machine
    _gameState.Start();
  }

  public override void _Input(InputEvent @event) {
    if (@event.IsActionPressed("start_game")) {
      _gameState.Send(new GameState.Input.StartGame());
    }
    else if (@event.IsActionPressed("pause_game")) {
      _gameState.Send(new GameState.Input.PauseGame());
    }
    else if (@event.IsActionPressed("resume_game")) {
      _gameState.Send(new GameState.Input.ResumeGame());
    }
  }

  public override void _ExitTree() {
    _binding?.Dispose();
  }
}
```

## State Machine Visualization

Enable state diagram generation by adding the `Diagram` attribute:

```csharp
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class GameState : LogicBlock<GameState.State> {
  // ... rest of the implementation
}
```

The diagram will be generated as a `.g.puml` file next to your state machine class.

## Next Steps

- Learn about [Advanced State Management](./advanced-states.md)
- Explore [Testing LogicBlocks](./testing.md)
- See [Real-World Examples](./examples.md)
- Understand [Domain Integration](./domain-integration.md) 