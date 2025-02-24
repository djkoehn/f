---
title: "Testing LogicBlocks"
description: "Comprehensive guide to testing LogicBlocks state machines"
category: "guides"
version: "1.0.0"
---

# Testing LogicBlocks

This guide covers testing strategies and best practices for LogicBlocks state machines.

## Unit Testing Basics

LogicBlocks is designed with testability in mind. Here's how to effectively test your state machines:

### 1. Basic State Machine Test

```csharp
using Chickensoft.GoDotTest;
using Chickensoft.LogicBlocks;

public class GameStateTests : TestClass {
  private GameState _gameState = null!;
  private GameState.IBinding _binding = null!;
  private List<GameState.Output.IOutput> _outputs;

  public override void BeforeEach() {
    _outputs = new List<GameState.Output.IOutput>();
    _gameState = new GameState();
    
    // Bind to all outputs
    _binding = _gameState.Bind()
      .HandleAll(output => _outputs.Add(output));
    
    _gameState.Start();
  }

  public override void AfterEach() {
    _binding.Dispose();
  }

  [Test]
  public void StartsInMainMenu() {
    Assert.That(_gameState.CurrentState, Is.EqualTo(GameState.State.MainMenu));
  }

  [Test]
  public void TransitionsToPlaying() {
    _gameState.Send(new GameState.Input.StartGame());
    
    Assert.That(_gameState.CurrentState, Is.EqualTo(GameState.State.Playing));
    Assert.That(_outputs, Has.Count.EqualTo(1));
    Assert.That(_outputs[0], Is.TypeOf<GameState.Output.GameStarted>());
  }
}
```

### 2. Testing with Dependencies

```csharp
public interface IPlayerPhysics {
  Vector2 CalculateVelocity(Vector2 direction);
  bool IsGrounded { get; }
}

public class MockPlayerPhysics : IPlayerPhysics {
  public Vector2 CalculateVelocity(Vector2 direction) => direction;
  public bool IsGrounded { get; set; } = true;
}

public class PlayerLogicTests : TestClass {
  private PlayerLogic _logic = null!;
  private MockPlayerPhysics _physics = null!;
  private List<PlayerLogic.Output.IOutput> _outputs;

  public override void BeforeEach() {
    _outputs = new List<PlayerLogic.Output.IOutput>();
    _physics = new MockPlayerPhysics();
    
    var context = new PlayerContext(_physics);
    _logic = new PlayerLogic(context);
    
    _logic.Bind().HandleAll(output => _outputs.Add(output));
    _logic.Start();
  }

  [Test]
  public void JumpsWhenGrounded() {
    _physics.IsGrounded = true;
    _logic.Send(new PlayerLogic.Input.Jump());
    
    Assert.That(_logic.CurrentState, Is.EqualTo(PlayerLogic.State.Airborne));
    Assert.That(
      _outputs.OfType<PlayerLogic.Output.AnimationChanged>()
        .First().AnimationName,
      Is.EqualTo("jump")
    );
  }
}
```

## Integration Testing

### 1. Testing with Godot Nodes

```csharp
public class PlayerNodeTests : GodotIntegrationTest {
  private Player _player = null!;
  private Node2D _world = null!;

  public override void BeforeEach() {
    _world = new Node2D();
    AddChild(_world);
    
    _player = new Player();
    _world.AddChild(_player);
    
    // Wait for _Ready to complete
    await ToSignal(_player, "ready");
  }

  [Test]
  public async Task MovesAndAnimates() {
    // Simulate input
    var inputEvent = new InputEventKey {
      Pressed = true,
      Keycode = Key.Space
    };
    _player._Input(inputEvent);
    
    // Wait for animation
    await ToSignal(
      _player.GetNode<AnimationPlayer>("AnimationPlayer"),
      "animation_finished"
    );
    
    Assert.That(_player.Position.y, Is.LessThan(0));
  }
}
```

### 2. Testing State Machine Composition

```csharp
public class GameSystemTests : TestClass {
  private AppRepo _appRepo = null!;
  private PlayerLogic _playerLogic = null!;
  private CoinLogic _coinLogic = null!;

  public override void BeforeEach() {
    _appRepo = new AppRepo();
    var context = new GameContext(_appRepo);
    
    _playerLogic = new PlayerLogic(context);
    _coinLogic = new CoinLogic(context);
    
    _playerLogic.Start();
    _coinLogic.Start();
  }

  [Test]
  public void CollectsCoin() {
    // Setup initial conditions
    _playerLogic.Send(new PlayerLogic.Input.Move(Vector2.Right));
    
    // Simulate coin collection
    _coinLogic.Send(new CoinLogic.Input.Collect());
    
    Assert.That(_appRepo.NumCoinsCollected.Value, Is.EqualTo(1));
  }
}
```

## Testing Best Practices

### 1. Mock Complex Dependencies

```csharp
public class MockGameWorld : IGameWorld {
  public List<string> LoadedScenes { get; } = new();
  public Dictionary<string, object> GameState { get; } = new();

  public void LoadScene(string sceneName) {
    LoadedScenes.Add(sceneName);
  }

  public T GetGameState<T>(string key) where T : class {
    return GameState[key] as T ?? throw new KeyNotFoundException();
  }
}
```

### 2. Test State Transitions

```csharp
[Test]
public void FollowsCorrectTransitionSequence() {
  var states = new List<PlayerLogic.State>();
  _playerLogic.OnStateChanged += (prev, curr) => states.Add(curr);

  _playerLogic.Send(new PlayerLogic.Input.Jump());
  _playerLogic.Send(new PlayerLogic.Input.Land());

  Assert.That(states, Is.EqualTo(new[] {
    PlayerLogic.State.Airborne.Rising,
    PlayerLogic.State.Airborne.Falling,
    PlayerLogic.State.Grounded.Idle
  }));
}
```

### 3. Test Output Generation

```csharp
[Test]
public void GeneratesCorrectOutputSequence() {
  var outputs = new List<string>();
  _playerLogic.Bind().HandleAll(output => {
    outputs.Add(output switch {
      PlayerLogic.Output.MovementUpdated m => "Movement",
      PlayerLogic.Output.AnimationChanged a => a.AnimationName,
      _ => output.GetType().Name
    });
  });

  _playerLogic.Send(new PlayerLogic.Input.Jump());

  Assert.That(outputs, Is.EqualTo(new[] {
    "jump",
    "Movement"
  }));
}
```

### 4. Test Error Conditions

```csharp
[Test]
public void HandlesInvalidTransitions() {
  _playerLogic.CurrentState = PlayerLogic.State.Airborne.Rising;
  
  // Should not be able to jump while already jumping
  _playerLogic.Send(new PlayerLogic.Input.Jump());
  
  Assert.That(_playerLogic.CurrentState, 
    Is.EqualTo(PlayerLogic.State.Airborne.Rising));
}
```

## Advanced Testing Patterns

### 1. State Machine Test Fixtures

```csharp
public class LogicBlockTestFixture<TState, TLogic>
  where TLogic : LogicBlock<TState> {
  
  protected TLogic Logic { get; }
  protected List<object> Outputs { get; } = new();
  protected IBinding Binding { get; }

  public LogicBlockTestFixture(TLogic logic) {
    Logic = logic;
    Binding = Logic.Bind().HandleAll(output => Outputs.Add(output));
    Logic.Start();
  }

  public void Reset() {
    Outputs.Clear();
  }

  public void Dispose() {
    Binding.Dispose();
  }
}
```

### 2. Async State Testing

```csharp
[Test]
public async Task HandlesAsyncTransitions() {
  var completion = new TaskCompletionSource<bool>();
  
  _playerLogic.Bind()
    .Handle<PlayerLogic.Output.LoadingComplete>(_ => {
      completion.SetResult(true);
    });

  _playerLogic.Send(new PlayerLogic.Input.LoadGame());
  
  // Wait for async operation with timeout
  var completed = await Task.WhenAny(
    completion.Task,
    Task.Delay(TimeSpan.FromSeconds(5))
  );
  
  Assert.That(completed, Is.EqualTo(completion.Task));
}
```

## See Also

- [Advanced State Management](./advanced-states.md)
- [Domain Integration](./domain-integration.md)
- [Real-World Examples](./examples.md) 