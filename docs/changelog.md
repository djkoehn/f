---
title: "Changelog"
description: "Version history and changes for Chickensoft packages"
---

# Changelog

All notable changes to Chickensoft packages will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Support for Godot 4.3
- Enhanced state machine visualization
- Improved error messages and debugging tools

### Changed
- Updated dependency injection container for better performance
- Refined event handling system
- Enhanced documentation and examples

### Fixed
- Various bug fixes and performance improvements
- Memory leak in event subscription cleanup
- State transition edge cases

## [4.0.0] - 2024-03-15

### Added
- Full support for Godot 4.x
- New state machine features:
  - Hierarchical states
  - State machine visualization
  - Enhanced debugging tools
- Improved dependency injection:
  - Property injection
  - Scoped services
  - Better lifecycle management
- Enhanced event system:
  - Type-safe events
  - Event validation
  - Subscription tracking
- New testing utilities:
  - Mock framework integration
  - Test helpers
  - Coverage tools

### Changed
- Completely rewritten for Godot 4.x
- Modernized architecture and APIs
- Improved performance and memory usage
- Enhanced documentation and examples

### Removed
- Legacy Godot 3.x support
- Deprecated APIs and features
- Old testing framework

### Fixed
- Various bug fixes and improvements
- Memory leaks
- Threading issues
- Performance bottlenecks

## [3.2.0] - 2023-12-10

### Added
- Support for async state transitions
- Enhanced logging capabilities
- New debugging tools

### Changed
- Improved error handling
- Better performance for large state machines
- Updated documentation

### Fixed
- Memory leaks in event system
- State transition race conditions
- Service resolution issues

## [3.1.0] - 2023-09-20

### Added
- State machine visualization tools
- Enhanced debugging capabilities
- New test helpers

### Changed
- Improved performance
- Better error messages
- Updated documentation

### Fixed
- Various bug fixes
- Memory management issues
- Threading problems

## [3.0.0] - 2023-06-15

### Added
- Initial release for Godot 3.x
- Core features:
  - State machines
  - Dependency injection
  - Event system
- Basic documentation and examples

### Changed
- Complete rewrite from previous versions
- Modern architecture
- Better performance

## Migration Guides

### Migrating from 3.x to 4.x

#### State Machines

1. Update namespace imports:
```csharp
// Old
using Chickensoft.StateMachine;

// New
using Chickensoft.LogicBlocks;
```

2. Update state machine declarations:
```csharp
// Old
public class PlayerStateMachine : StateMachine
{
    protected override void Configure()
    {
        // Old configuration
    }
}

// New
public partial class PlayerStateMachine : StateMachine<PlayerContext>
{
    protected override void Configure(IStateConfiguration<PlayerContext> config)
    {
        // New configuration
    }
}
```

3. Update state declarations:
```csharp
// Old
public class IdleState : State
{
    public override void Enter()
    {
        // Old enter logic
    }
}

// New
public partial class IdleState : State<PlayerContext>
{
    public override void Enter(PlayerContext context)
    {
        // New enter logic
    }
}
```

#### Dependency Injection

1. Update service registration:
```csharp
// Old
Container.Register<IGameService, GameService>();

// New
Service.Register<IGameService, GameService>();
```

2. Update property injection:
```csharp
// Old
[Inject]
private IGameService GameService { get; set; }

// New
[Inject] public IGameService GameService { get; set; }
```

#### Event System

1. Update event publishing:
```csharp
// Old
EventManager.Publish(new GameEvent());

// New
EventBus.Publish(new GameEvent());
```

2. Update event subscription:
```csharp
// Old
EventManager.Subscribe<GameEvent>(OnGameEvent);

// New
EventBus.Subscribe<GameEvent>(OnGameEvent);
```

### Breaking Changes in 4.0

1. State Machine Changes:
   - States now require a context type parameter
   - Configuration API has changed
   - State lifecycle methods have new signatures

2. Dependency Injection Changes:
   - New service container implementation
   - Changed registration and resolution APIs
   - New scoping rules

3. Event System Changes:
   - New event bus implementation
   - Changed subscription model
   - New cleanup requirements

## Package Versions

### LogicBlocks
- 4.0.0 - Current stable release
- 3.2.0 - Legacy Godot 3.x support
- 3.1.0 - Legacy release
- 3.0.0 - Initial release

### AutoInject
- 4.0.0 - Current stable release
- 3.2.0 - Legacy Godot 3.x support
- 3.1.0 - Legacy release
- 3.0.0 - Initial release

### EventBus
- 4.0.0 - Current stable release
- 3.2.0 - Legacy Godot 3.x support
- 3.1.0 - Legacy release
- 3.0.0 - Initial release

## Support Policy

- Latest version (4.x):
  - Full support
  - Regular updates
  - Bug fixes
  - New features

- Previous version (3.x):
  - Security updates only
  - Critical bug fixes
  - No new features

- Older versions:
  - No support
  - No updates
  - Users should upgrade

## Reporting Issues

Please report any issues on GitHub:
- [LogicBlocks Issues](https://github.com/chickensoft-games/LogicBlocks/issues)
- [AutoInject Issues](https://github.com/chickensoft-games/AutoInject/issues)
- [EventBus Issues](https://github.com/chickensoft-games/EventBus/issues)

Include:
- Package version
- Godot version
- .NET version
- Reproduction steps
- Expected vs actual behavior 