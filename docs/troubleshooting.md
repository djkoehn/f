---
title: "Troubleshooting Guide"
description: "Solutions for common issues when using Chickensoft"
---

# Troubleshooting Guide

This guide helps you diagnose and fix common issues you might encounter while using Chickensoft packages.

## Table of Contents
- [Build and Setup Issues](#build-and-setup-issues)
- [State Machine Problems](#state-machine-problems)
- [Dependency Injection Issues](#dependency-injection-issues)
- [Event System Problems](#event-system-problems)
- [Performance Issues](#performance-issues)
- [Common Error Messages](#common-error-messages)

## Build and Setup Issues

### NuGet Package Resolution Failures

#### Symptoms
- "Package not found" errors
- Version conflicts
- Missing dependencies

#### Solutions
1. Clear NuGet cache:
```bash
dotnet nuget locals all --clear
```

2. Restore packages:
```bash
dotnet restore --force
```

3. Check package versions in .csproj:
```xml
<ItemGroup>
    <PackageReference Include="Chickensoft.LogicBlocks" Version="4.0.0" />
    <PackageReference Include="Chickensoft.AutoInject" Version="4.0.0" />
    <PackageReference Include="Chickensoft.EventBus" Version="4.0.0" />
</ItemGroup>
```

### Project Build Failures

#### Symptoms
- Compilation errors
- Missing type errors
- Reference errors

#### Solutions
1. Clean solution:
```bash
dotnet clean
dotnet build
```

2. Check SDK version in .csproj:
```xml
<Project Sdk="Godot.NET.Sdk/4.3.0">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
  </PropertyGroup>
</Project>
```

3. Verify Godot version compatibility:
```bash
godot --version
dotnet --version
```

## State Machine Problems

### State Machine Not Transitioning

#### Symptoms
- States don't change
- Transitions not triggering
- No error messages

#### Diagnostic Steps
1. Check state machine initialization:
```csharp
public override void _Ready()
{
    // Add debug logging
    GD.Print("Initializing state machine...");
    _stateMachine = new PlayerStateMachine();
    _stateMachine.Start(_context);
    GD.Print($"Initial state: {_stateMachine.CurrentState}");
}
```

2. Verify transition configuration:
```csharp
protected override void Configure(IStateConfiguration<PlayerContext> config)
{
    config.AddTransition(transition => transition
        .From<IdleState>()
        .To<WalkingState>()
        .On<MoveInputEvent>()
        .When(context =>
        {
            // Add debug logging
            GD.Print($"Checking transition: {context.IsGrounded}");
            return context.IsGrounded;
        })
    );
}
```

3. Check event publishing:
```csharp
public void TryMove()
{
    GD.Print("Publishing MoveInputEvent");
    EventBus.Publish(new MoveInputEvent());
}
```

#### Solutions
1. Add state machine logging:
```csharp
public partial class DebugState<TContext> : State<TContext>
{
    public override void Enter(TContext context)
    {
        GD.Print($"Entering {GetType().Name}");
    }

    public override void Exit(TContext context)
    {
        GD.Print($"Exiting {GetType().Name}");
    }
}
```

2. Verify guard conditions:
```csharp
// Before
.When(context => context.IsGrounded)

// After - with debugging
.When(context =>
{
    var canTransition = context.IsGrounded && !context.IsStunned;
    GD.Print($"Transition check: IsGrounded={context.IsGrounded}, " +
            $"IsStunned={context.IsStunned}, Result={canTransition}");
    return canTransition;
})
```

### State Update Not Called

#### Symptoms
- State behavior not executing
- Update logic not running

#### Solutions
1. Ensure Update is called:
```csharp
public override void _Process(double delta)
{
    try
    {
        _stateMachine?.Update();
    }
    catch (Exception e)
    {
        GD.PrintErr($"State machine update error: {e}");
    }
}
```

2. Check state implementation:
```csharp
public partial class MovementState : State<PlayerContext>
{
    public override void Update(PlayerContext context)
    {
        base.Update(context);
        GD.Print($"Updating MovementState: {context.Position}");
        // Update logic here
    }
}
```

## Dependency Injection Issues

### Service Resolution Failures

#### Symptoms
- NullReferenceException when accessing services
- Services not being injected
- Missing dependencies

#### Diagnostic Steps
1. Check service registration:
```csharp
public override void _Ready()
{
    try
    {
        Service.Initialize();
        
        // Register with logging
        GD.Print("Registering GameService...");
        Service.Register<IGameService, GameService>();
        
        // Verify registration
        var service = Service.Get<IGameService>();
        GD.Print($"Service resolved: {service != null}");
    }
    catch (Exception e)
    {
        GD.PrintErr($"Service registration error: {e}");
    }
}
```

2. Verify injection points:
```csharp
public partial class GameUI : Control
{
    [Inject] public IGameService GameService { get; set; }

    public override void _Ready()
    {
        base._Ready();
        
        // Verify injection
        if (GameService == null)
        {
            GD.PrintErr("GameService not injected!");
            return;
        }
        
        GD.Print("GameService successfully injected");
    }
}
```

#### Solutions
1. Add service registration logging:
```csharp
public static class ServiceRegistry
{
    public static void RegisterGameServices()
    {
        try
        {
            Service.Initialize();
            
            // Core services
            RegisterService<IGameService, GameService>("GameService");
            RegisterService<IInputService, InputService>("InputService");
            RegisterService<IStateFactory, StateFactory>("StateFactory");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Service registration failed: {e}");
            throw;
        }
    }

    private static void RegisterService<TInterface, TImplementation>(
        string name) where TImplementation : class, TInterface
    {
        GD.Print($"Registering {name}...");
        Service.Register<TInterface, TImplementation>();
        var instance = Service.Get<TInterface>();
        GD.Print($"{name} registered successfully: {instance != null}");
    }
}
```

2. Implement service validation:
```csharp
public static class ServiceValidator
{
    public static void ValidateRequiredServices()
    {
        ValidateService<IGameService>("GameService");
        ValidateService<IInputService>("InputService");
        ValidateService<IStateFactory>("StateFactory");
    }

    private static void ValidateService<T>(string name)
    {
        try
        {
            var service = Service.Get<T>();
            if (service == null)
            {
                throw new InvalidOperationException(
                    $"{name} failed to resolve");
            }
            GD.Print($"{name} validated successfully");
        }
        catch (Exception e)
        {
            GD.PrintErr($"{name} validation failed: {e}");
            throw;
        }
    }
}
```

## Event System Problems

### Event Subscription Issues

#### Symptoms
- Events not being received
- Multiple event handlers firing
- Memory leaks

#### Diagnostic Steps
1. Add subscription tracking:
```csharp
public class EventTracker
{
    private static readonly Dictionary<Type, int> _subscriptionCounts = new();

    public static void TrackSubscription<T>()
    {
        var type = typeof(T);
        if (!_subscriptionCounts.ContainsKey(type))
        {
            _subscriptionCounts[type] = 0;
        }
        _subscriptionCounts[type]++;
        
        GD.Print($"Subscribed to {type.Name}. " +
                $"Total subscriptions: {_subscriptionCounts[type]}");
    }

    public static void TrackUnsubscription<T>()
    {
        var type = typeof(T);
        if (_subscriptionCounts.ContainsKey(type))
        {
            _subscriptionCounts[type]--;
            GD.Print($"Unsubscribed from {type.Name}. " +
                    $"Total subscriptions: {_subscriptionCounts[type]}");
        }
    }
}
```

2. Debug event publishing:
```csharp
public static class EventDebugger
{
    public static void PublishWithLogging<T>(T evt)
    {
        GD.Print($"Publishing event: {typeof(T).Name}");
        GD.Print($"Event data: {JsonSerializer.Serialize(evt)}");
        
        EventBus.Publish(evt);
    }
}
```

#### Solutions
1. Implement safe subscription management:
```csharp
public class SafeEventSubscriber : Node
{
    private readonly List<Action> _cleanupActions = new();

    protected void SafeSubscribe<T>(Action<T> handler)
    {
        EventTracker.TrackSubscription<T>();
        EventBus.Subscribe(handler);
        _cleanupActions.Add(() =>
        {
            EventBus.Unsubscribe(handler);
            EventTracker.TrackUnsubscription<T>();
        });
    }

    public override void _ExitTree()
    {
        foreach (var cleanup in _cleanupActions)
        {
            cleanup();
        }
        base._ExitTree();
    }
}
```

2. Use event validation:
```csharp
public static class EventValidator
{
    public static void ValidateEvent<T>(T evt)
    {
        if (evt == null)
        {
            throw new ArgumentNullException(nameof(evt));
        }

        var properties = typeof(T).GetProperties();
        foreach (var prop in properties)
        {
            var value = prop.GetValue(evt);
            if (value == null)
            {
                GD.PrintErr($"Event {typeof(T).Name} has null " +
                          $"property: {prop.Name}");
            }
        }
    }
}
```

## Performance Issues

### Memory Leaks

#### Symptoms
- Increasing memory usage
- Degraded performance over time
- Scene cleanup issues

#### Solutions
1. Implement disposable pattern:
```csharp
public class GameSystem : IDisposable
{
    private bool _disposed;
    private readonly List<Action> _cleanupActions = new();

    protected void AddCleanupAction(Action action)
    {
        _cleanupActions.Add(action);
    }

    public void Dispose()
    {
        if (_disposed) return;
        
        foreach (var action in _cleanupActions)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                GD.PrintErr($"Cleanup action failed: {e}");
            }
        }
        
        _cleanupActions.Clear();
        _disposed = true;
    }
}
```

2. Monitor event subscriptions:
```csharp
public static class EventMonitor
{
    private static readonly Dictionary<Type, HashSet<WeakReference>> 
        _subscribers = new();

    public static void TrackSubscriber<T>(object subscriber)
    {
        var type = typeof(T);
        if (!_subscribers.ContainsKey(type))
        {
            _subscribers[type] = new HashSet<WeakReference>();
        }
        
        _subscribers[type].Add(new WeakReference(subscriber));
        CleanupDeadReferences(type);
        
        GD.Print($"Active subscribers for {type.Name}: " +
                $"{_subscribers[type].Count}");
    }

    private static void CleanupDeadReferences(Type type)
    {
        if (_subscribers.TryGetValue(type, out var refs))
        {
            refs.RemoveWhere(r => !r.IsAlive);
        }
    }
}
```

### State Machine Performance

#### Symptoms
- Slow state transitions
- High CPU usage
- Frame drops

#### Solutions
1. Optimize state updates:
```csharp
public partial class OptimizedState<TContext> : State<TContext>
{
    private readonly HashSet<Type> _validTransitions = new();
    private bool _needsUpdate;

    protected void RegisterValidTransition<T>()
    {
        _validTransitions.Add(typeof(T));
    }

    public override void Update(TContext context)
    {
        if (!_needsUpdate) return;
        
        // Perform update
        _needsUpdate = false;
    }

    protected bool CanTransitionTo<T>()
    {
        return _validTransitions.Contains(typeof(T));
    }
}
```

2. Implement state caching:
```csharp
public partial class CachedStateMachine<TContext> : StateMachine<TContext>
{
    private readonly Dictionary<Type, State<TContext>> _stateCache = new();

    protected State<TContext> GetOrCreateState<T>() where T : State<TContext>
    {
        var type = typeof(T);
        if (!_stateCache.TryGetValue(type, out var state))
        {
            state = Activator.CreateInstance<T>();
            _stateCache[type] = state;
        }
        return state;
    }
}
```

## Common Error Messages

### "Service not registered"

#### Error
```
System.InvalidOperationException: Service of type 'IGameService' is not registered
```

#### Solution
```csharp
public static class ServiceBootstrapper
{
    public static void Initialize()
    {
        try
        {
            Service.Initialize();
            RegisterRequiredServices();
            ValidateServices();
        }
        catch (Exception e)
        {
            GD.PrintErr($"Service initialization failed: {e}");
            throw;
        }
    }

    private static void RegisterRequiredServices()
    {
        var services = new Dictionary<Type, Type>
        {
            { typeof(IGameService), typeof(GameService) },
            { typeof(IInputService), typeof(InputService) },
            // Add more services here
        };

        foreach (var (interface, implementation) in services)
        {
            GD.Print($"Registering {interface.Name}...");
            Service.Register(interface, implementation);
        }
    }

    private static void ValidateServices()
    {
        var requiredServices = new[]
        {
            typeof(IGameService),
            typeof(IInputService)
        };

        foreach (var service in requiredServices)
        {
            if (Service.Get(service) == null)
            {
                throw new InvalidOperationException(
                    $"Required service {service.Name} is not registered");
            }
        }
    }
}
```

### "State machine not started"

#### Error
```
System.InvalidOperationException: State machine must be started before use
```

#### Solution
```csharp
public partial class SafeStateMachine<TContext> : StateMachine<TContext>
{
    private bool _isStarted;
    private readonly string _name;

    public SafeStateMachine(string name = null)
    {
        _name = name ?? GetType().Name;
    }

    public override void Start(TContext context)
    {
        try
        {
            GD.Print($"Starting state machine: {_name}");
            base.Start(context);
            _isStarted = true;
            GD.Print($"State machine started: {_name}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to start state machine {_name}: {e}");
            throw;
        }
    }

    public override void Update()
    {
        if (!_isStarted)
        {
            GD.PrintErr($"Attempted to update non-started state machine: {_name}");
            return;
        }

        try
        {
            base.Update();
        }
        catch (Exception e)
        {
            GD.PrintErr($"State machine update failed {_name}: {e}");
            throw;
        }
    }
}
```

## Additional Resources

- [API Documentation](./api.md)
- [Examples](./examples/)
- [FAQ](./faq.md)
- [GitHub Issues](https://github.com/chickensoft-games/issues)

If you encounter an issue not covered in this guide:
1. Check the [GitHub Issues](https://github.com/chickensoft-games/issues)
2. Join our [Discord community](https://discord.gg/chickensoft)
3. Submit a new issue with detailed reproduction steps 