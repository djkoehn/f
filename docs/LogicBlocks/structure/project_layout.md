---
title: Project Structure Guide
description: Standard project structure and conventions for F game blocks
keywords: project structure, conventions, organization, godot, c#, gdscript
---

# Project Structure Guide

This guide outlines the standard project structure for implementing blocks in F using ChickenSoft LogicBlocks.

## Directory Structure Overview

```
src/
  BlockName/                         # Main block directory
  ├── State/                        # LogicBlock state management
  │   ├── States/                   # Individual state implementations
  │   │   ├── BlockLogic.State1.cs # First state implementation
  │   │   ├── BlockLogic.State2.cs # Second state implementation
  │   │   └── BlockLogic.State3.cs # Third state implementation
  │   ├── BlockLogic.Data.cs       # State data model
  │   ├── BlockLogic.Input.cs      # Input interface and handlers
  │   ├── BlockLogic.Output.cs     # Output interface and events
  │   ├── BlockLogic.State.cs      # State management logic
  │   └── BlockLogic.cs            # Main logic implementation
  ├── Block.cs                     # Main C# component
  ├── Block.tscn                   # Godot scene definition
  └── BlockData.cs                 # Block configuration data

addons/
└── BlockAddon/                    # Block's Godot plugin
    ├── plugin.cfg                 # Plugin configuration
    ├── plugin.gd                  # Plugin entry point
    ├── scenes/                    # UI scenes
    │   └── BlockComponent.tscn    # Main UI scene
    └── scripts/                   # GDScript UI logic
        └── BlockComponent.gd      # UI implementation
```

## Component Responsibilities

### C# Components (`src/BlockName/`)

#### State Management (`State/`)
- **States/**: Individual state implementations
  - Each state handles specific block behavior
  - States should be focused and follow Single Responsibility Principle
  - State transitions should be explicit and documented

#### Core Files
- **Block.cs**: Main component class
  - Interfaces with Godot
  - Manages lifecycle
  - Handles initialization
- **BlockData.cs**: Configuration
  - Immutable data structure
  - Block-specific settings
  - Serialization support

### GDScript Components (`addons/BlockAddon/`)

#### Plugin Structure
- **plugin.cfg**: Plugin metadata
- **plugin.gd**: Plugin registration
- **scenes/**: UI definitions
- **scripts/**: UI logic

## Naming Conventions

1. **C# Files**
   - PascalCase for all files
   - Suffix with `.cs`
   - State files: `BlockLogic.StateName.cs`
   - Interface files: `IBlockName.cs`

2. **GDScript Files**
   - snake_case for files
   - Suffix with `.gd`
   - Component files: `block_component.gd`

3. **Scene Files**
   - PascalCase for scenes
   - Suffix with `.tscn`
   - Main scenes: `BlockName.tscn`
   - Component scenes: `BlockComponent.tscn`

## Implementation Guidelines

### State Files

```csharp
// BlockLogic.State1.cs
public partial class State1 : State<BlockLogic>
{
    public override void OnEnter() { }
    public override void OnExit() { }
    public override void OnInput(InputType input) { }
}
```

### Data Models

```csharp
// BlockLogic.Data.cs
public record BlockData
{
    public required string Id { get; init; }
    public required Dictionary<string, object> Settings { get; init; }
}
```

### Input/Output

```csharp
// BlockLogic.Input.cs
public interface IBlockInput
{
    void ProcessInput(InputData data);
}

// BlockLogic.Output.cs
public interface IBlockOutput
{
    event Action<OutputData> OnOutput;
}
```

## Best Practices

1. **File Organization**
   - Keep related files together
   - Use consistent naming
   - Follow the standard structure

2. **Code Organization**
   - One class per file
   - Clear separation of concerns
   - Interface-based communication

3. **Documentation**
   - Document public APIs
   - Include examples
   - Explain state transitions

4. **Testing**
   - Unit tests for each state
   - Integration tests for flows
   - UI tests for components

## Common Patterns

### State Management
```csharp
public partial class BlockLogic
{
    private readonly ILog _log = new Log<BlockLogic>();
    private readonly Dictionary<string, IState> _states;

    public BlockLogic()
    {
        _states = new Dictionary<string, IState>
        {
            { "State1", new State1(this) },
            { "State2", new State2(this) }
        };
    }
}
```

### Plugin Registration
```gdscript
@tool
extends EditorPlugin

func _enter_tree():
    add_custom_type("BlockName", "Node",
        preload("scripts/block_component.gd"),
        preload("icon.png"))

func _exit_tree():
    remove_custom_type("BlockName")
```

## See Also

- [Block Integration Example](../examples/block_integration.md)
- [API Reference](../reference/index.md)
- [Testing Guide](../testing/index.md) 