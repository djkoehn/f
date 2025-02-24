---
title: "Serialization Documentation"
description: "Object serialization utilities for C# and Godot"
category: "api"
version: "1.0.0"
---

# Serialization

Chickensoft.Serialization provides a flexible and efficient serialization system for C# objects, with special consideration for game data.

## Overview

The Serialization library offers:
- Type-safe serialization
- Custom serialization formats
- Versioning support
- Performance optimizations
- Extensible architecture

## Installation

Add the NuGet package to your `.csproj` file:

```xml
<ItemGroup>
  <PackageReference Include="Chickensoft.Serialization" Version="4.0.0" />
</ItemGroup>
```

## Basic Usage

### 1. Basic Serialization

```csharp
using Chickensoft.Serialization;

[Serializable]
public class PlayerData {
  public string Name { get; set; } = "";
  public int Level { get; set; }
  public Vector3 Position { get; set; }
}

public class SerializationExample {
  public void SavePlayer(PlayerData player) {
    var serializer = new DataSerializer();
    var data = serializer.Serialize(player);
    File.WriteAllBytes("player.dat", data);
  }
  
  public PlayerData LoadPlayer() {
    var serializer = new DataSerializer();
    var data = File.ReadAllBytes("player.dat");
    return serializer.Deserialize<PlayerData>(data);
  }
}
```

### 2. Custom Type Handling

```csharp
public class CustomTypeSerializer : ITypeSerializer<CustomType> {
  public byte[] Serialize(CustomType value) {
    // Custom serialization logic
    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream);
    
    writer.Write(value.Id);
    writer.Write(value.Name);
    
    return stream.ToArray();
  }
  
  public CustomType Deserialize(byte[] data) {
    // Custom deserialization logic
    using var stream = new MemoryStream(data);
    using var reader = new BinaryReader(stream);
    
    return new CustomType {
      Id = reader.ReadInt32(),
      Name = reader.ReadString()
    };
  }
}
```

## Advanced Features

### 1. Versioned Serialization

```csharp
[Serializable]
[TypeVersion(2)]
public class GameState {
  public string GameVersion { get; set; } = "1.0.0";
  public Dictionary<string, object> Data { get; set; } = new();
  
  [SerializationConstructor]
  public GameState() { }
  
  [VersionUpgrade(1, 2)]
  public static GameState UpgradeV1ToV2(GameState old) {
    var newState = new GameState {
      GameVersion = old.GameVersion,
      Data = old.Data
    };
    
    // Add new version 2 fields
    newState.Data["newFeature"] = true;
    
    return newState;
  }
}
```

### 2. Collection Serialization

```csharp
public class CollectionSerializer {
  private readonly DataSerializer _serializer = new();
  
  public void SaveCollection<T>(IEnumerable<T> items, string path) {
    var serializedItems = items.Select(item => _serializer.Serialize(item));
    var collection = new SerializedCollection<T>(serializedItems);
    
    File.WriteAllBytes(path, _serializer.Serialize(collection));
  }
  
  public IEnumerable<T> LoadCollection<T>(string path) {
    var data = File.ReadAllBytes(path);
    var collection = _serializer.Deserialize<SerializedCollection<T>>(data);
    
    return collection.Items.Select(item => 
      _serializer.Deserialize<T>(item)
    );
  }
}
```

### 3. Compression Integration

```csharp
public class CompressedSerializer {
  private readonly DataSerializer _serializer = new();
  
  public async Task SaveCompressed<T>(T data, string path) {
    var serialized = _serializer.Serialize(data);
    
    using var fileStream = File.Create(path);
    using var compress = new GZipStream(
      fileStream, 
      CompressionLevel.Optimal
    );
    
    await compress.WriteAsync(serialized);
  }
  
  public async Task<T> LoadCompressed<T>(string path) {
    using var fileStream = File.OpenRead(path);
    using var decompress = new GZipStream(
      fileStream, 
      CompressionMode.Decompress
    );
    using var memoryStream = new MemoryStream();
    
    await decompress.CopyToAsync(memoryStream);
    var data = memoryStream.ToArray();
    
    return _serializer.Deserialize<T>(data);
  }
}
```

## Best Practices

1. **Type Safety**
   - Use attributes for versioning
   - Implement version upgrades
   - Handle null values properly

2. **Performance**
   - Cache serializers when possible
   - Use compression for large data
   - Consider binary formats

3. **Data Integrity**
   - Validate data after deserialization
   - Handle version mismatches
   - Implement error recovery

## Common Patterns

### 1. Data Contract

```csharp
[Serializable]
public class DataContract<T> {
  public string Version { get; set; }
  public DateTime Timestamp { get; set; }
  public T Data { get; set; }
  
  public DataContract(T data) {
    Version = typeof(T).Assembly.GetName().Version?.ToString() ?? "1.0.0";
    Timestamp = DateTime.UtcNow;
    Data = data;
  }
  
  [SerializationConstructor]
  private DataContract() { }
}

public class ContractSerializer {
  private readonly DataSerializer _serializer = new();
  
  public async Task Save<T>(T data, string path) {
    var contract = new DataContract<T>(data);
    var serialized = _serializer.Serialize(contract);
    await File.WriteAllBytesAsync(path, serialized);
  }
  
  public async Task<T> Load<T>(string path) {
    var serialized = await File.ReadAllBytesAsync(path);
    var contract = _serializer.Deserialize<DataContract<T>>(serialized);
    return contract.Data;
  }
}
```

### 2. Serialization Cache

```csharp
public class SerializationCache {
  private readonly Dictionary<Type, object> _serializers = new();
  private readonly DataSerializer _defaultSerializer = new();
  
  public ITypeSerializer<T> GetSerializer<T>() {
    var type = typeof(T);
    
    if (_serializers.TryGetValue(type, out var serializer)) {
      return (ITypeSerializer<T>)serializer;
    }
    
    var newSerializer = CreateSerializer<T>();
    _serializers[type] = newSerializer;
    return newSerializer;
  }
  
  private ITypeSerializer<T> CreateSerializer<T>() {
    // Create custom serializer based on type
    if (typeof(T) == typeof(Vector3)) {
      return (ITypeSerializer<T>)new Vector3Serializer();
    }
    
    return new DefaultSerializer<T>(_defaultSerializer);
  }
}
```

### 3. Batch Processing

```csharp
public class BatchSerializer {
  private readonly DataSerializer _serializer = new();
  private readonly int _batchSize;
  
  public BatchSerializer(int batchSize = 1000) {
    _batchSize = batchSize;
  }
  
  public async Task SaveBatch<T>(
    IEnumerable<T> items, 
    string directory
  ) {
    var batches = items.Chunk(_batchSize);
    var tasks = new List<Task>();
    
    foreach (var (batch, index) in batches.Select(
      (b, i) => (b, i)
    )) {
      var path = Path.Combine(directory, $"batch_{index}.dat");
      var task = SaveBatchFile(batch, path);
      tasks.Add(task);
    }
    
    await Task.WhenAll(tasks);
  }
  
  private async Task SaveBatchFile<T>(
    IEnumerable<T> batch, 
    string path
  ) {
    var serialized = _serializer.Serialize(batch.ToList());
    await File.WriteAllBytesAsync(path, serialized);
  }
}
```

## See Also

- [Serialization.Godot](../serialization-godot/index.md)
- [SaveFileBuilder Integration](../save-file-builder/index.md)
- [Best Practices Guide](../guides/best-practices.md) 