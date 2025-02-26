---
title: Reference Documentation
description: Reference documentation for F game development with ChickenSoft and Godot
keywords: reference, documentation, godot, chickensoft, c#, gdscript
---

# Reference Documentation

This section provides comprehensive reference documentation for developing blocks in F using ChickenSoft libraries and Godot.

## External Documentation

### Godot Documentation
- [GDScript Basics](https://docs.godotengine.org/en/stable/tutorials/scripting/gdscript/gdscript_basics.html)
- [C# Basics](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html)
- [Custom Types](https://docs.godotengine.org/en/stable/tutorials/plugins/editor/creating_plugins.html)

### ChickenSoft Libraries
- [AutoInject](https://github.com/chickensoft-games/AutoInject)
  - Dependency injection for Godot
  - Service location and management
  - Automatic injection of dependencies

- [Introspection](https://github.com/chickensoft-games/Introspection)
  - Runtime type information
  - Dynamic object creation
  - Property and method reflection

- [LogicBlocks](https://github.com/chickensoft-games/LogicBlocks)
  - State management
  - Input/Output handling
  - Event-driven architecture

- [Serialization](https://github.com/chickensoft-games/Serialization)
  - Object serialization
  - Data persistence
  - Type conversion

- [Serialization.Godot](https://github.com/chickensoft-games/Serialization.Godot)
  - Godot-specific serialization
  - Scene data handling
  - Resource management

## Project-Specific Documentation

### Block Development

1. **State Management**
   - [State Pattern Implementation](./state_management.md)
   - [State Transitions](./state_transitions.md)
   - [Data Flow](./data_flow.md)

2. **Input/Output**
   - [Input Handling](./input_handling.md)
   - [Output Events](./output_events.md)
   - [Token Processing](./token_processing.md)

3. **UI Integration**
   - [GDScript Bridge](./gdscript_bridge.md)
   - [Scene Management](./scene_management.md)
   - [Plugin Development](./plugin_development.md)

### Best Practices

1. **Code Organization**
   ```csharp
   // Example of well-organized block structure
   public partial class Block : Node
   {
       private readonly IBlockLogic _logic;
       private readonly IBlockUI _ui;
       
       public Block(IBlockLogic logic, IBlockUI ui)
       {
           _logic = logic;
           _ui = ui;
       }
   }
   ```

2. **State Implementation**
   ```csharp
   // Example of state implementation
   public partial class ProcessingState : State<BlockLogic>
   {
       public override void OnEnter()
       {
           Logic.Output.EmitStatus("Processing");
       }

       public override void OnInput(TokenData token)
       {
           var result = ProcessToken(token);
           Logic.Output.EmitResult(result);
       }

       private TokenData ProcessToken(TokenData token)
       {
           // Token processing logic
           return token;
       }
   }
   ```

3. **UI Integration**
   ```gdscript
   # Example of GDScript UI integration
   extends Node
   
   var block_logic: Block
   
   func _ready():
       block_logic = get_node("Block")
       connect_signals()
   
   func connect_signals():
       block_logic.connect("result_ready", self, "_on_result_ready")
   ```

## API Reference

### Core Interfaces

```csharp
public interface IBlock
{
    void Initialize();
    void ProcessToken(TokenData token);
    event Action<TokenData> OnResult;
}

public interface IBlockLogic
{
    IBlockData Data { get; }
    IBlockInput Input { get; }
    IBlockOutput Output { get; }
}

public interface IBlockUI
{
    void UpdateDisplay(string value);
    void ShowError(string message);
    void Reset();
}
```

### Common Types

```csharp
public record TokenData
{
    public required float Value { get; init; }
    public required Dictionary<string, object> Metadata { get; init; }
}

public record BlockConfiguration
{
    public required string Id { get; init; }
    public required BlockType Type { get; init; }
    public required Dictionary<string, object> Settings { get; init; }
}
```

## Testing

### Unit Tests
```csharp
public class BlockTests : GodotTestBase
{
    private IBlock _block;
    private Mock<IBlockLogic> _mockLogic;
    private Mock<IBlockUI> _mockUI;

    [SetUp]
    public void Setup()
    {
        _mockLogic = new Mock<IBlockLogic>();
        _mockUI = new Mock<IBlockUI>();
        _block = new Block(_mockLogic.Object, _mockUI.Object);
    }

    [Test]
    public void ProcessToken_ValidInput_EmitsResult()
    {
        // Test implementation
    }
}
```

### Integration Tests
```csharp
public class BlockIntegrationTests : GodotIntegrationTestBase
{
    [Test]
    public async Task Block_FullFlow_Success()
    {
        // Test implementation
    }
}
```

## See Also

- [Block Integration Example](../examples/block_integration.md)
- [Project Structure](../structure/project_layout.md)
- [Testing Guide](../testing/index.md) 