---
title: "Introspection Documentation"
description: "Runtime type inspection and reflection utilities for C# and Godot"
category: "api"
version: "1.0.0"
---

# Introspection

Chickensoft.Introspection provides powerful runtime type inspection and reflection utilities optimized for C# and Godot development.

## Overview

The Introspection library offers:
- Fast runtime type inspection
- Cached reflection operations
- Type metadata utilities
- Godot-aware type handling

## Installation

Add the NuGet package to your `.csproj` file:

```xml
<ItemGroup>
  <PackageReference Include="Chickensoft.Introspection" Version="4.0.0" />
</ItemGroup>
```

## Basic Usage

### 1. Type Inspection

```csharp
using Chickensoft.Introspection;

public class TypeInspector {
  public void InspectType<T>() {
    var typeInfo = TypeCache<T>.Info;
    
    Console.WriteLine($"Type: {typeInfo.Name}");
    Console.WriteLine($"Is Class: {typeInfo.IsClass}");
    Console.WriteLine($"Base Type: {typeInfo.BaseType?.Name}");
    
    foreach (var property in typeInfo.Properties) {
      Console.WriteLine($"Property: {property.Name} ({property.Type.Name})");
    }
  }
}
```

### 2. Property Access

```csharp
public class PropertyAccessor {
  public T GetPropertyValue<T>(object obj, string propertyName) {
    var property = TypeCache.GetProperty(obj.GetType(), propertyName);
    return (T)property.GetValue(obj);
  }
  
  public void SetPropertyValue<T>(object obj, string propertyName, T value) {
    var property = TypeCache.GetProperty(obj.GetType(), propertyName);
    property.SetValue(obj, value);
  }
}
```

## Advanced Features

### 1. Type Metadata

```csharp
public class MetadataInspector {
  public void InspectMetadata<T>() {
    var metadata = TypeCache<T>.Metadata;
    
    // Get custom attributes
    var attributes = metadata.GetAttributes<CustomAttribute>();
    
    // Check interface implementations
    var implements = metadata.ImplementsInterface<IDisposable>();
    
    // Get generic arguments
    var genericArgs = metadata.GetGenericArguments();
  }
}
```

### 2. Method Reflection

```csharp
public class MethodInvoker {
  public object InvokeMethod(object obj, string methodName, params object[] args) {
    var method = TypeCache.GetMethod(obj.GetType(), methodName);
    return method.Invoke(obj, args);
  }
  
  public async Task<object> InvokeAsyncMethod(
    object obj, 
    string methodName, 
    params object[] args
  ) {
    var method = TypeCache.GetMethod(obj.GetType(), methodName);
    var result = method.Invoke(obj, args);
    
    if (result is Task task) {
      await task;
      return task.GetType().GetProperty("Result")?.GetValue(task);
    }
    
    return result;
  }
}
```

### 3. Godot Integration

```csharp
public class GodotTypeInspector {
  public void InspectGodotNode(Node node) {
    var typeInfo = TypeCache.GetTypeInfo(node.GetType());
    
    // Get Godot signals
    var signals = typeInfo.GetAttributes<SignalAttribute>();
    
    // Get exported properties
    var exports = typeInfo.GetProperties()
      .Where(p => p.HasAttribute<ExportAttribute>());
    
    // Get node paths
    var nodePaths = typeInfo.GetFields()
      .Where(f => f.HasAttribute<NodePathAttribute>());
  }
}
```

## Best Practices

1. **Performance**
   - Cache type information when possible
   - Use TypeCache for repeated operations
   - Avoid unnecessary reflection

2. **Type Safety**
   - Validate types before operations
   - Handle null values appropriately
   - Use generic methods when possible

3. **Memory Management**
   - Clear caches when needed
   - Handle large type hierarchies carefully
   - Be mindful of reflection overhead

## Common Patterns

### 1. Dynamic Object Creation

```csharp
public class ObjectFactory {
  public T CreateInstance<T>() where T : class {
    var constructor = TypeCache<T>.Constructor;
    return constructor.Invoke();
  }
  
  public T CreateWithParameters<T>(params object[] args) where T : class {
    var constructor = TypeCache<T>.GetConstructor(args.Select(a => a.GetType()));
    return constructor.Invoke(args) as T;
  }
}
```

### 2. Property Mapping

```csharp
public class PropertyMapper {
  public void MapProperties<TSource, TTarget>(TSource source, TTarget target) {
    var sourceProps = TypeCache<TSource>.Properties;
    var targetProps = TypeCache<TTarget>.Properties;
    
    foreach (var sourceProp in sourceProps) {
      var targetProp = targetProps.FirstOrDefault(p => p.Name == sourceProp.Name);
      if (targetProp != null && targetProp.CanWrite) {
        var value = sourceProp.GetValue(source);
        targetProp.SetValue(target, value);
      }
    }
  }
}
```

### 3. Type Validation

```csharp
public class TypeValidator {
  public bool ValidateType<T>(object obj) {
    var typeInfo = TypeCache<T>.Info;
    
    // Check if type implements required interfaces
    foreach (var requiredInterface in typeInfo.GetInterfaces()) {
      if (!obj.GetType().IsAssignableTo(requiredInterface)) {
        return false;
      }
    }
    
    // Validate required attributes
    var hasRequiredAttributes = typeInfo.GetAttributes<RequiredAttribute>()
      .All(attr => obj.GetType().HasAttribute(attr.GetType()));
    
    return hasRequiredAttributes;
  }
}
```

## See Also

- [Serialization Integration](../serialization/index.md)
- [AutoInject Integration](../auto-inject/index.md)
- [Testing Guide](./testing.md) 