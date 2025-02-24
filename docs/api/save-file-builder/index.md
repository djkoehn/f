---
title: "SaveFileBuilder Documentation"
description: "Save file management utilities for Godot C# games"
category: "api"
version: "1.0.0"
---

# SaveFileBuilder

Chickensoft.SaveFileBuilder provides a robust system for managing game save files in Godot C# projects.

## Overview

SaveFileBuilder offers:
- Type-safe save file creation
- Automatic serialization
- Save file versioning
- Data migration support
- Corruption protection

## Installation

Add the NuGet package to your `.csproj` file:

```xml
<ItemGroup>
  <PackageReference Include="Chickensoft.SaveFileBuilder" Version="4.0.0" />
</ItemGroup>
```

## Basic Usage

### 1. Defining Save Data

```csharp
using Chickensoft.SaveFileBuilder;

public class PlayerSaveData {
  public Vector3 Position { get; set; }
  public int Health { get; set; }
  public List<string> Inventory { get; set; } = new();
}

public class GameSaveData {
  public PlayerSaveData Player { get; set; } = new();
  public Dictionary<string, bool> Achievements { get; set; } = new();
  public float PlayTime { get; set; }
}
```

### 2. Creating a Save File

```csharp
public class SaveManager {
  private readonly SaveFileBuilder<GameSaveData> _builder;
  
  public SaveManager() {
    _builder = new SaveFileBuilder<GameSaveData>("save.dat")
      .WithVersion(1)
      .WithCompression()
      .WithEncryption("secret_key");
  }
  
  public async Task SaveGame(GameSaveData data) {
    await _builder.Save(data);
  }
  
  public async Task<GameSaveData> LoadGame() {
    return await _builder.Load();
  }
}
```

## Advanced Features

### 1. Save File Versioning

```csharp
public class SaveFileManager {
  private readonly SaveFileBuilder<GameSaveData> _builder;
  
  public SaveFileManager() {
    _builder = new SaveFileBuilder<GameSaveData>("save.dat")
      .WithVersion(2)
      .WithMigration(1, MigrateV1ToV2)
      .WithBackup();
  }
  
  private GameSaveData MigrateV1ToV2(GameSaveData oldData) {
    // Migrate data from version 1 to version 2
    var newData = new GameSaveData {
      Player = oldData.Player,
      Achievements = oldData.Achievements,
      PlayTime = oldData.PlayTime
    };
    
    // Add new version 2 fields
    newData.Player.Level = 1;
    newData.Settings = new GameSettings();
    
    return newData;
  }
}
```

### 2. Auto-Saving

```csharp
public class AutoSaveManager {
  private readonly SaveFileBuilder<GameSaveData> _builder;
  private readonly Timer _autoSaveTimer;
  
  public AutoSaveManager() {
    _builder = new SaveFileBuilder<GameSaveData>("autosave.dat")
      .WithVersion(1)
      .WithBackup()
      .WithCompression();
    
    _autoSaveTimer = new Timer(TimeSpan.FromMinutes(5));
    _autoSaveTimer.Elapsed += OnAutoSave;
  }
  
  private async void OnAutoSave(object sender, ElapsedEventArgs e) {
    var currentData = GetCurrentGameData();
    await _builder.Save(currentData, "auto");
  }
}
```

### 3. Save Slots

```csharp
public class SaveSlotManager {
  private readonly Dictionary<int, SaveFileBuilder<GameSaveData>> _slots = new();
  
  public SaveSlotManager() {
    for (int i = 1; i <= 3; i++) {
      _slots[i] = new SaveFileBuilder<GameSaveData>($"save_{i}.dat")
        .WithVersion(1)
        .WithCompression()
        .WithBackup();
    }
  }
  
  public async Task SaveToSlot(int slot, GameSaveData data) {
    if (_slots.TryGetValue(slot, out var builder)) {
      await builder.Save(data);
    }
  }
  
  public async Task<GameSaveData> LoadFromSlot(int slot) {
    if (_slots.TryGetValue(slot, out var builder)) {
      return await builder.Load();
    }
    throw new ArgumentException($"Invalid save slot: {slot}");
  }
}
```

## Best Practices

1. **Data Safety**
   - Always use versioning
   - Implement data migration
   - Enable backups for critical saves

2. **Performance**
   - Use compression for large saves
   - Implement async operations
   - Consider save file size

3. **User Experience**
   - Provide multiple save slots
   - Implement auto-save
   - Show save progress feedback

## Common Patterns

### 1. Save File Validation

```csharp
public class SaveValidator {
  private readonly SaveFileBuilder<GameSaveData> _builder;
  
  public async Task<bool> ValidateSave(string path) {
    try {
      var data = await _builder.Load(path);
      
      // Validate required fields
      if (data.Player == null) return false;
      if (data.Achievements == null) return false;
      
      // Validate data ranges
      if (data.Player.Health < 0) return false;
      if (data.PlayTime < 0) return false;
      
      return true;
    }
    catch {
      return false;
    }
  }
}
```

### 2. Save File Recovery

```csharp
public class SaveRecovery {
  private readonly SaveFileBuilder<GameSaveData> _builder;
  
  public async Task<GameSaveData> RecoverSave(string path) {
    try {
      // Try loading from backup first
      return await _builder.LoadBackup(path);
    }
    catch {
      // If backup fails, create new save
      return new GameSaveData {
        Player = new PlayerSaveData {
          Health = 100,
          Position = Vector3.Zero
        },
        PlayTime = 0
      };
    }
  }
}
```

### 3. Save File Management

```csharp
public class SaveFileSystem {
  private readonly SaveFileBuilder<GameSaveData> _builder;
  
  public async Task<List<SaveInfo>> ListSaves() {
    var saves = new List<SaveInfo>();
    var saveDir = new DirectoryInfo("saves");
    
    foreach (var file in saveDir.GetFiles("*.dat")) {
      try {
        var metadata = await _builder.GetMetadata(file.FullName);
        saves.Add(new SaveInfo {
          Path = file.FullName,
          Version = metadata.Version,
          LastModified = file.LastWriteTime,
          Size = file.Length
        });
      }
      catch {
        // Skip corrupted saves
        continue;
      }
    }
    
    return saves;
  }
  
  public async Task CleanupOldSaves(TimeSpan maxAge) {
    var saves = await ListSaves();
    var now = DateTime.UtcNow;
    
    foreach (var save in saves) {
      if (now - save.LastModified > maxAge) {
        File.Delete(save.Path);
      }
    }
  }
}
```

## See Also

- [Serialization Integration](../serialization/index.md)
- [Serialization.Godot Integration](../serialization-godot/index.md)
- [Best Practices Guide](../guides/best-practices.md) 