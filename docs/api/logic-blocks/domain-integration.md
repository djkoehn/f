---
title: "Domain Integration"
description: "Integrating LogicBlocks with domain logic and repositories"
category: "guides"
version: "1.0.0"
---

# Domain Integration

This guide covers how to integrate LogicBlocks with your game's domain logic using repositories and domain-driven design principles.

## Domain Architecture

### Repository Pattern

The repository pattern provides a clean interface to your game's domain logic:

```csharp
public interface IAppRepo {
  IObservable<int> NumCoinsCollected { get; }
  IObservable<int> NumCoinsAtStart { get; }
  IObservable<float> PlayerHealth { get; }
  IObservable<Dictionary<string, int>> Inventory { get; }
  
  bool CollectCoin(int coinId);
  void DamagePlayer(float amount);
  void AddInventoryItem(string itemId, int quantity);
}

public class AppRepo : IAppRepo {
  private readonly ReactiveProperty<int> _numCoinsCollected = new(0);
  private readonly ReactiveProperty<int> _numCoinsAtStart = new(0);
  private readonly ReactiveProperty<float> _playerHealth = new(100f);
  private readonly ReactiveProperty<Dictionary<string, int>> _inventory = 
    new(new Dictionary<string, int>());

  public IObservable<int> NumCoinsCollected => _numCoinsCollected;
  public IObservable<int> NumCoinsAtStart => _numCoinsAtStart;
  public IObservable<float> PlayerHealth => _playerHealth;
  public IObservable<Dictionary<string, int>> Inventory => _inventory;

  public bool CollectCoin(int coinId) {
    // Domain logic for coin collection
    _numCoinsCollected.Value++;
    return true;
  }

  public void DamagePlayer(float amount) {
    _playerHealth.Value = Math.Max(0, _playerHealth.Value - amount);
  }

  public void AddInventoryItem(string itemId, int quantity) {
    var inventory = new Dictionary<string, int>(_inventory.Value);
    if (!inventory.ContainsKey(itemId)) {
      inventory[itemId] = 0;
    }
    inventory[itemId] += quantity;
    _inventory.Value = inventory;
  }
}
```

### Domain Events

Use domain events to communicate between different parts of your game:

```csharp
public static class DomainEvents {
  public readonly record struct CoinCollected(int CoinId);
  public readonly record struct PlayerDamaged(float Amount);
  public readonly record struct ItemCollected(string ItemId, int Quantity);
  public readonly record struct LevelCompleted(int LevelId, int Score);
}
```

## Integration with LogicBlocks

### 1. State Machine with Domain Logic

```csharp
public partial class PlayerLogic : LogicBlock<PlayerLogic.State> {
  private readonly IAppRepo _appRepo;
  private readonly IGameWorld _gameWorld;

  public PlayerLogic(IContext context) {
    _appRepo = context.Get<IAppRepo>();
    _gameWorld = context.Get<IGameWorld>();
  }

  public abstract record StateLogic : State {
    public record Playing : StateLogic,
      IGet<Input.TakeDamage>,
      IGet<Input.CollectItem> {
      
      public Transition On(Input.TakeDamage input) {
        Get<IAppRepo>().DamagePlayer(input.Amount);
        
        if (Get<IAppRepo>().PlayerHealth.Value <= 0) {
          return To<Dead>();
        }
        return Stay();
      }

      public Transition On(Input.CollectItem input) {
        Get<IAppRepo>().AddInventoryItem(input.ItemId, input.Quantity);
        return Stay();
      }
    }

    public record Dead : StateLogic {
      public override void OnEnter() {
        Get<IGameWorld>().LoadScene("GameOver");
      }
    }
  }
}
```

### 2. Reactive State Updates

```csharp
public partial class UILogic : LogicBlock<UILogic.State> {
  public record State : StateLogic {
    private IDisposable? _healthSubscription;
    private IDisposable? _inventorySubscription;

    public override void OnEnter() {
      var appRepo = Get<IAppRepo>();
      
      // Subscribe to health changes
      _healthSubscription = appRepo.PlayerHealth
        .Subscribe(health => {
          Context.Output(new Output.HealthChanged(health));
        });

      // Subscribe to inventory changes
      _inventorySubscription = appRepo.Inventory
        .Subscribe(inventory => {
          Context.Output(new Output.InventoryChanged(inventory));
        });
    }

    public override void OnExit() {
      _healthSubscription?.Dispose();
      _inventorySubscription?.Dispose();
    }
  }
}
```

### 3. Domain-Driven State Composition

```csharp
public class GameManager : Node {
  private readonly IAppRepo _appRepo;
  private readonly Dictionary<string, LogicBlock> _logicBlocks;

  public GameManager() {
    _appRepo = new AppRepo();
    _logicBlocks = new Dictionary<string, LogicBlock>();

    var context = new GameContext(_appRepo);

    // Create and register logic blocks
    _logicBlocks["player"] = new PlayerLogic(context);
    _logicBlocks["ui"] = new UILogic(context);
    _logicBlocks["inventory"] = new InventoryLogic(context);
    _logicBlocks["quest"] = new QuestLogic(context);

    // Start all logic blocks
    foreach (var block in _logicBlocks.Values) {
      block.Start();
    }
  }
}
```

## Advanced Domain Integration

### 1. Save State Management

```csharp
public interface ISaveState {
  Dictionary<string, object> Serialize();
  void Deserialize(Dictionary<string, object> data);
}

public class AppRepo : IAppRepo, ISaveState {
  public Dictionary<string, object> Serialize() {
    return new Dictionary<string, object> {
      ["coins"] = _numCoinsCollected.Value,
      ["health"] = _playerHealth.Value,
      ["inventory"] = _inventory.Value
    };
  }

  public void Deserialize(Dictionary<string, object> data) {
    _numCoinsCollected.Value = (int)data["coins"];
    _playerHealth.Value = (float)data["health"];
    _inventory.Value = (Dictionary<string, int>)data["inventory"];
  }
}
```

### 2. Domain Validation

```csharp
public class DomainRules {
  public static class Player {
    public const float MAX_HEALTH = 100f;
    public const float MIN_HEALTH = 0f;
    public const int MAX_INVENTORY_ITEMS = 50;
  }

  public static class Inventory {
    public static bool CanAddItem(
      Dictionary<string, int> inventory,
      string itemId,
      int quantity
    ) {
      var totalItems = inventory.Values.Sum();
      return totalItems + quantity <= Player.MAX_INVENTORY_ITEMS;
    }
  }
}

public class AppRepo : IAppRepo {
  public void AddInventoryItem(string itemId, int quantity) {
    var inventory = new Dictionary<string, int>(_inventory.Value);
    
    if (!DomainRules.Inventory.CanAddItem(inventory, itemId, quantity)) {
      throw new DomainException("Inventory full");
    }
    
    if (!inventory.ContainsKey(itemId)) {
      inventory[itemId] = 0;
    }
    inventory[itemId] += quantity;
    _inventory.Value = inventory;
  }
}
```

### 3. Domain Events with LogicBlocks

```csharp
public class DomainEventBus {
  private readonly Dictionary<Type, List<Action<object>>> _handlers = new();

  public void Subscribe<T>(Action<T> handler) where T : struct {
    if (!_handlers.ContainsKey(typeof(T))) {
      _handlers[typeof(T)] = new List<Action<object>>();
    }
    _handlers[typeof(T)].Add(obj => handler((T)obj));
  }

  public void Publish<T>(T @event) where T : struct {
    if (_handlers.TryGetValue(typeof(T), out var handlers)) {
      foreach (var handler in handlers) {
        handler(@event);
      }
    }
  }
}

public partial class PlayerLogic : LogicBlock<PlayerLogic.State> {
  private readonly DomainEventBus _eventBus;

  public PlayerLogic(IContext context) {
    _eventBus = context.Get<DomainEventBus>();
    
    // Subscribe to domain events
    _eventBus.Subscribe<DomainEvents.PlayerDamaged>(
      evt => Send(new Input.TakeDamage(evt.Amount))
    );
  }

  public record Playing : StateLogic {
    public override void OnEnter() {
      // Publish domain event
      Get<DomainEventBus>().Publish(
        new DomainEvents.PlayerStateChanged("playing")
      );
    }
  }
}
```

## Best Practices

1. **Repository Design**
   - Keep repositories focused on domain logic
   - Use interfaces for better testability
   - Implement proper validation

2. **State Management**
   - Use reactive properties for state
   - Handle state transitions atomically
   - Validate state changes

3. **Event Handling**
   - Use domain events for cross-cutting concerns
   - Keep events immutable
   - Document event flow

4. **Testing**
   - Mock repositories in tests
   - Verify domain rules
   - Test state transitions

## See Also

- [Advanced State Management](./advanced-states.md)
- [Testing LogicBlocks](./testing.md)
- [Real-World Examples](./examples.md) 