---
title: "Log Documentation"
description: "Logging utilities for C# and Godot development"
category: "api"
version: "1.0.0"
---

# Log

Chickensoft.Log provides a flexible and powerful logging system designed specifically for C# and Godot development.

## Overview

The Log library offers:
- Structured logging
- Multiple output targets
- Log levels and filtering
- Godot integration
- Performance optimizations

## Installation

Add the NuGet package to your `.csproj` file:

```xml
<ItemGroup>
  <PackageReference Include="Chickensoft.Log" Version="4.0.0" />
</ItemGroup>
```

## Basic Usage

### 1. Simple Logging

```csharp
using Chickensoft.Log;

public class GameManager {
  private readonly ILog _log = Log.GetLogger<GameManager>();
  
  public void Initialize() {
    _log.Info("Game manager initializing...");
    
    try {
      // Initialization code
      _log.Info("Game manager initialized successfully");
    }
    catch (Exception ex) {
      _log.Error("Failed to initialize game manager", ex);
    }
  }
}
```

### 2. Log Levels

```csharp
public class PlayerController {
  private readonly ILog _log = Log.GetLogger<PlayerController>();
  
  public void Update() {
    _log.Debug("Player position updated"); // Development details
    _log.Info("Player entered new area"); // General information
    _log.Warning("Player health low"); // Potential issues
    _log.Error("Player collision failed"); // Errors
    _log.Fatal("Game state corrupted"); // Critical failures
  }
}
```

## Advanced Features

### 1. Structured Logging

```csharp
public class InventorySystem {
  private readonly ILog _log = Log.GetLogger<InventorySystem>();
  
  public void AddItem(Item item) {
    _log.Info("Adding item to inventory", new {
      ItemId = item.Id,
      ItemType = item.Type,
      Quantity = item.Quantity,
      Timestamp = DateTime.UtcNow
    });
  }
  
  public void RemoveItem(Item item, string reason) {
    _log.Info("Removing item from inventory", new {
      ItemId = item.Id,
      Reason = reason,
      RemainingSpace = CalculateRemainingSpace()
    });
  }
}
```

### 2. Custom Log Targets

```csharp
public class FileLogger : ILogTarget {
  private readonly string _filePath;
  
  public FileLogger(string filePath) {
    _filePath = filePath;
  }
  
  public void Log(LogMessage message) {
    var entry = $"[{message.Timestamp:yyyy-MM-dd HH:mm:ss}] " +
                $"[{message.Level}] " +
                $"[{message.Category}] " +
                $"{message.Message}";
    
    File.AppendAllText(_filePath, entry + Environment.NewLine);
  }
}

// Register the custom target
Log.AddTarget(new FileLogger("game.log"));
```

### 3. Godot Integration

```csharp
public class GodotLogger : Node {
  private readonly ILog _log = Log.GetLogger<GodotLogger>();
  
  public override void _Ready() {
    Log.AddTarget(new GodotLogTarget());
    _log.Info("Godot logger initialized");
  }
}

public class GodotLogTarget : ILogTarget {
  public void Log(LogMessage message) {
    var godotLevel = ConvertToGodotLevel(message.Level);
    GD.Print($"[{message.Level}] {message.Message}");
  }
  
  private int ConvertToGodotLevel(LogLevel level) {
    return level switch {
      LogLevel.Debug => 0,
      LogLevel.Info => 1,
      LogLevel.Warning => 2,
      LogLevel.Error => 3,
      LogLevel.Fatal => 4,
      _ => 1
    };
  }
}
```

## Best Practices

1. **Performance**
   - Use appropriate log levels
   - Enable/disable debug logging in production
   - Consider log target performance

2. **Organization**
   - Use consistent categories
   - Include relevant context
   - Structure log messages clearly

3. **Error Handling**
   - Log exceptions with stack traces
   - Include relevant state information
   - Use appropriate log levels

## Common Patterns

### 1. Game State Logging

```csharp
public class GameStateLogger {
  private readonly ILog _log = Log.GetLogger<GameStateLogger>();
  
  public void LogStateTransition(
    string fromState,
    string toState,
    Dictionary<string, object> context
  ) {
    _log.Info("Game state transition", new {
      From = fromState,
      To = toState,
      Context = context,
      Timestamp = DateTime.UtcNow
    });
  }
  
  public void LogGameEvent(string eventName, object eventData) {
    _log.Info($"Game event: {eventName}", eventData);
  }
}
```

### 2. Performance Monitoring

```csharp
public class PerformanceLogger {
  private readonly ILog _log = Log.GetLogger<PerformanceLogger>();
  private readonly Stopwatch _stopwatch = new();
  
  public IDisposable MeasureOperation(string operationName) {
    return new OperationTimer(_log, operationName);
  }
  
  private class OperationTimer : IDisposable {
    private readonly ILog _log;
    private readonly string _operationName;
    private readonly Stopwatch _stopwatch;
    
    public OperationTimer(ILog log, string operationName) {
      _log = log;
      _operationName = operationName;
      _stopwatch = Stopwatch.StartNew();
    }
    
    public void Dispose() {
      _stopwatch.Stop();
      _log.Debug($"Operation '{_operationName}' completed", new {
        Duration = _stopwatch.ElapsedMilliseconds,
        Operation = _operationName
      });
    }
  }
}
```

### 3. Error Tracking

```csharp
public class ErrorTracker {
  private readonly ILog _log = Log.GetLogger<ErrorTracker>();
  private readonly Dictionary<string, int> _errorCounts = new();
  
  public void TrackError(
    Exception ex,
    string context,
    Dictionary<string, object> metadata
  ) {
    var errorKey = $"{ex.GetType().Name}:{context}";
    
    _errorCounts.TryGetValue(errorKey, out var count);
    _errorCounts[errorKey] = count + 1;
    
    _log.Error("Error occurred", new {
      Exception = ex,
      Context = context,
      Metadata = metadata,
      OccurrenceCount = count + 1
    });
  }
}
```

## See Also

- [AutoInject Integration](../auto-inject/index.md)
- [Testing Guide](./testing.md)
- [Best Practices Guide](../guides/best-practices.md) 