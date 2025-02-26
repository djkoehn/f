---
title: Block Integration Example
description: Example of integrating a LogicBlock component in F
keywords: logicblock, integration, example, godot, c#, gdscript
---

# Block Integration Example

This guide demonstrates how to integrate a LogicBlock component in F using our standard project structure. We'll use a practical example of a "Calculator" block that performs mathematical operations on tokens.

## Project Structure

```
src/
  Calculator/
  ├── State/
  │   ├── States/                    # State implementations for different calculator modes
  │   │   ├── CalculatorLogic.AddState.cs      # Addition operation state
  │   │   ├── CalculatorLogic.SubtractState.cs # Subtraction operation state
  │   │   └── CalculatorLogic.MultiplyState.cs # Multiplication operation state
  │   ├── CalculatorLogic.Data.cs    # Data model for calculator state
  │   ├── CalculatorLogic.Input.cs   # Input handlers for calculator operations
  │   ├── CalculatorLogic.Output.cs  # Output definitions for calculator results
  │   ├── CalculatorLogic.State.cs   # State management for calculator modes
  │   └── CalculatorLogic.cs         # Main calculator logic implementation
  ├── Calculator.cs                  # C# component for calculator functionality
  ├── Calculator.tscn                # Godot scene for calculator block
  └── CalculatorData.cs             # Data structure for calculator configuration

addons/
└── CalculatorAddon/
    ├── plugin.cfg                   # Plugin configuration
    ├── plugin.gd                    # Plugin entry point
    ├── scenes/
    │   └── CalculatorComponent.tscn # Scene with calculator UI
    └── scripts/
        └── CalculatorComponent.gd   # GDScript for UI interaction
```

## Component Overview

### C# Logic Layer (`src/Calculator/`)

The C# layer handles all business logic using LogicBlocks:

1. **Calculator.cs**: Main component class that interfaces with Godot
2. **CalculatorData.cs**: Configuration and persistent data
3. **State/**: LogicBlock implementation following SOLID principles

### GDScript UI Layer (`addons/CalculatorAddon/`)

The GDScript layer handles UI and scene management:

1. **CalculatorComponent.gd**: UI logic and C# communication
2. **CalculatorComponent.tscn**: Visual representation and layout

## Implementation Details

### State Management

The `States/` directory contains different calculator modes:

```csharp
// CalculatorLogic.AddState.cs
public partial class AddState : State<CalculatorLogic>
{
    public override void OnEnter()
    {
        // Initialize addition mode
    }

    public override void OnInput(TokenData token)
    {
        // Process token for addition
        var result = token.Value + Logic.Data.StoredValue;
        Logic.Output.EmitResult(result);
    }
}
```

### Data Model

```csharp
// CalculatorLogic.Data.cs
public record CalculatorData
{
    public required float StoredValue { get; init; }
    public required OperationMode Mode { get; init; }
}
```

### Input Handling

```csharp
// CalculatorLogic.Input.cs
public interface ICalculatorInput
{
    void SetMode(OperationMode mode);
    void ProcessToken(TokenData token);
}
```

## Integration with Godot

### Plugin Setup

```gdscript
# plugin.gd
@tool
extends EditorPlugin

func _enter_tree():
    add_custom_type("CalculatorBlock", "Node", 
        preload("scripts/CalculatorComponent.gd"),
        preload("res://addons/CalculatorAddon/icon.png"))
```

### UI Component

```gdscript
# CalculatorComponent.gd
extends Node

var calculator_logic: Calculator

func _ready():
    calculator_logic = get_node("Calculator")
    connect_signals()
```

## Best Practices

1. **Separation of Concerns**: Keep C# logic separate from GDScript UI
2. **State Pattern**: Use LogicBlocks for state management
3. **Interface Segregation**: Define clear input/output interfaces
4. **Dependency Injection**: Use Chickensoft.AutoInject for services

## Testing

```csharp
// Example test structure
public class CalculatorTests : GodotTestBase
{
    [Test]
    public async Task TestAdditionState()
    {
        var calculator = new Calculator();
        await calculator.Initialize();
        // Test addition functionality
    }
}
```

## Common Pitfalls

1. Avoid direct path references in C# classes
2. Use interfaces for GDScript communication
3. Keep state transitions explicit and documented
4. Handle cleanup in OnExit states

## See Also

- [Project Structure Guide](../structure/project_layout.md)
- [API Reference](../reference/index.md)
- [Testing Guide](../testing/index.md) 