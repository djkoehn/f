---
title: "Contributing Guide"
description: "Guidelines for contributing to Chickensoft packages"
---

# Contributing to Chickensoft

Thank you for your interest in contributing to Chickensoft! This guide will help you understand how to contribute effectively to the project.

## Table of Contents
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Making Changes](#making-changes)
- [Testing Guidelines](#testing-guidelines)
- [Documentation](#documentation)
- [Pull Requests](#pull-requests)
- [Code Style](#code-style)

## Getting Started

### Prerequisites
- [Godot 4.x](https://godotengine.org/download) with .NET/C# support
- [.NET SDK 6.0+](https://dotnet.microsoft.com/download)
- Git
- Your favorite IDE (we recommend Visual Studio Code)

### Setting Up the Development Environment

1. Fork the repository you want to contribute to:
   - [LogicBlocks](https://github.com/chickensoft-games/LogicBlocks)
   - [AutoInject](https://github.com/chickensoft-games/AutoInject)
   - [EventBus](https://github.com/chickensoft-games/EventBus)

2. Clone your fork:
```bash
git clone https://github.com/your-username/repository-name.git
cd repository-name
```

3. Add the upstream remote:
```bash
git remote add upstream https://github.com/chickensoft-games/repository-name.git
```

4. Create a new branch:
```bash
git checkout -b feature/your-feature-name
```

## Development Setup

### Building from Source

1. Restore dependencies:
```bash
dotnet restore
```

2. Build the project:
```bash
dotnet build
```

3. Run tests:
```bash
dotnet test
```

### Development Tools

We recommend installing these tools:
- [.NET Core Test Explorer](https://marketplace.visualstudio.com/items?itemName=formulahendry.dotnet-test-explorer)
- [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)
- [EditorConfig](https://marketplace.visualstudio.com/items?itemName=EditorConfig.EditorConfig)

## Making Changes

### Branching Strategy

- `main` - Main development branch
- `feature/*` - New features
- `bugfix/*` - Bug fixes
- `docs/*` - Documentation updates
- `test/*` - Test improvements

### Commit Messages

Follow the conventional commits specification:
```
type(scope): description

[optional body]

[optional footer]
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `test`: Adding or modifying tests
- `refactor`: Code refactoring
- `style`: Code style changes
- `perf`: Performance improvements
- `chore`: Maintenance tasks

Example:
```
feat(state-machine): add support for hierarchical states

Added the ability to create nested state machines with parent-child relationships.
This enables more complex state management scenarios while maintaining clean code organization.

Closes #123
```

## Testing Guidelines

### Writing Tests

1. Use descriptive test names:
```csharp
[Test]
public void StateMachine_WhenTransitionConditionMet_ChangesToTargetState()
{
    // Arrange
    var context = new TestContext();
    var stateMachine = new TestStateMachine();
    
    // Act
    stateMachine.Start(context);
    EventBus.Publish(new TransitionTrigger());
    
    // Assert
    Assert.That(stateMachine.CurrentState, Is.TypeOf<TargetState>());
}
```

2. Follow the Arrange-Act-Assert pattern:
```csharp
[Test]
public void Service_WhenRegistered_CanBeResolved()
{
    // Arrange
    Service.Initialize();
    Service.Register<ITestService, TestService>();
    
    // Act
    var service = Service.Get<ITestService>();
    
    // Assert
    Assert.That(service, Is.Not.Null);
    Assert.That(service, Is.TypeOf<TestService>());
}
```

3. Test edge cases:
```csharp
[Test]
public void EventBus_WhenSubscriberDisposed_DoesNotReceiveEvents()
{
    // Arrange
    var subscriber = new TestSubscriber();
    var eventReceived = false;
    EventBus.Subscribe<TestEvent>(evt => eventReceived = true);
    
    // Act
    subscriber.Dispose();
    EventBus.Publish(new TestEvent());
    
    // Assert
    Assert.That(eventReceived, Is.False);
}
```

### Test Coverage

- Aim for 80%+ code coverage
- Focus on testing business logic
- Include both positive and negative test cases
- Test async operations thoroughly

## Documentation

### Code Documentation

Use XML documentation for public APIs:
```csharp
/// <summary>
/// Represents a state in a state machine.
/// </summary>
/// <typeparam name="TContext">The type of the state context.</typeparam>
public abstract class State<TContext>
{
    /// <summary>
    /// Called when entering this state.
    /// </summary>
    /// <param name="context">The state context.</param>
    public virtual void Enter(TContext context)
    {
        // Implementation
    }
}
```

### README Updates

When adding features, update:
- Feature list
- Usage examples
- API documentation
- Breaking changes

## Pull Requests

### PR Checklist

Before submitting a PR, ensure:
- [ ] Tests pass
- [ ] Code is documented
- [ ] README is updated
- [ ] Changelog is updated
- [ ] Code follows style guide
- [ ] Commits follow convention

### PR Template

```markdown
## Description
Brief description of the changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
Describe how you tested the changes

## Breaking Changes
List any breaking changes

## Checklist
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] Changelog updated
```

## Code Style

### General Guidelines

1. Use meaningful names:
```csharp
// Good
public class PlayerMovementState : State<PlayerContext>
{
    private readonly IInputService _inputService;
    private readonly IPhysicsService _physicsService;
}

// Bad
public class PS : State<PC>
{
    private readonly IS _i;
    private readonly PS _p;
}
```

2. Keep methods focused:
```csharp
// Good
public void HandlePlayerMovement()
{
    var input = GetPlayerInput();
    UpdatePosition(input);
    AnimateMovement(input);
}

// Bad
public void DoEverything()
{
    HandleInput();
    UpdatePhysics();
    ProcessAI();
    RenderGraphics();
    PlaySound();
}
```

3. Use dependency injection:
```csharp
// Good
public class GameManager
{
    private readonly IGameService _gameService;
    private readonly ISaveService _saveService;

    public GameManager(
        IGameService gameService,
        ISaveService saveService)
    {
        _gameService = gameService;
        _saveService = saveService;
    }
}

// Bad
public class GameManager
{
    private GameService _gameService = new();
    private SaveService _saveService = new();
}
```

### Naming Conventions

- PascalCase for types and methods
- camelCase for parameters and local variables
- _camelCase for private fields
- ALL_CAPS for constants
- Use descriptive names that convey intent

### File Organization

```csharp
// One type per file
// Filename: PlayerMovementState.cs
public partial class PlayerMovementState : State<PlayerContext>
{
    // Implementation
}

// Nested types in separate files
// Filename: PlayerMovementState.Running.cs
public partial class PlayerMovementState
{
    public partial class Running : State<PlayerContext>
    {
        // Implementation
    }
}
```

### Error Handling

1. Use exceptions for exceptional cases:
```csharp
public void RegisterService<T>(T service)
{
    if (service == null)
    {
        throw new ArgumentNullException(nameof(service));
    }

    if (_services.ContainsKey(typeof(T)))
    {
        throw new InvalidOperationException(
            $"Service of type {typeof(T)} is already registered");
    }

    _services[typeof(T)] = service;
}
```

2. Provide meaningful error messages:
```csharp
public State<TContext> GetState<T>() where T : State<TContext>
{
    var type = typeof(T);
    if (!_states.TryGetValue(type, out var state))
    {
        throw new InvalidOperationException(
            $"State of type {type.Name} is not registered in the state machine");
    }
    return state;
}
```

## Additional Resources

- [Chickensoft Discord](https://discord.gg/chickensoft)
- [GitHub Issues](https://github.com/chickensoft-games/issues)
- [API Documentation](./api.md)
- [Examples](./examples/)

## Questions?

If you have questions about contributing:
1. Check the [FAQ](./faq.md)
2. Ask in our [Discord community](https://discord.gg/chickensoft)
3. Open a [GitHub Discussion](https://github.com/chickensoft-games/discussions) 