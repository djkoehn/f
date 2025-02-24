---
title: "Installation Guide"
description: "How to install and set up ChickenSoft libraries in your Godot 4.3 C# project"
category: "guides"
version: "1.0.0"
---

# Installation Guide

This guide will help you set up ChickenSoft libraries in your Godot 4.3 C# project.

## Prerequisites

1. **Godot 4.3** with .NET/C# support
2. **.NET 8.0** SDK or later
3. **Visual Studio Code** or another C# IDE
4. **NuGet** package manager

## Quick Start

### 1. Create a New Godot Project

1. Open Godot 4.3
2. Click "New Project"
3. Enable "C# Support"
4. Create the project

### 2. Install Core Packages

Add the following to your `.csproj` file:

```xml
<ItemGroup>
  <!-- Core Libraries -->
  <PackageReference Include="Chickensoft.AutoInject" Version="4.0.0" />
  <PackageReference Include="Chickensoft.LogicBlocks" Version="4.0.0" />
  <PackageReference Include="Chickensoft.GoDotTest" Version="4.0.0" />
  <PackageReference Include="Chickensoft.GodotNodeInterfaces" Version="4.0.0" />
  
  <!-- Utility Libraries -->
  <PackageReference Include="Chickensoft.Collections" Version="4.0.0" />
  <PackageReference Include="Chickensoft.GoDotCollections" Version="4.0.0" />
  <PackageReference Include="Chickensoft.Log" Version="4.0.0" />
  
  <!-- Data Management -->
  <PackageReference Include="Chickensoft.SaveFileBuilder" Version="4.0.0" />
  <PackageReference Include="Chickensoft.Serialization" Version="4.0.0" />
  <PackageReference Include="Chickensoft.Serialization.Godot" Version="4.0.0" />
</ItemGroup>
```

### 3. Initialize Your Project

1. Build the project:
```bash
dotnet build
```

2. Add global usings to your `GlobalUsings.cs`:
```csharp
global using Chickensoft.AutoInject;
global using Chickensoft.LogicBlocks;
global using Chickensoft.GodotNodeInterfaces;
global using Chickensoft.Collections;
global using Chickensoft.Log;
```

## Verification

Create a simple test scene to verify the installation:

1. Create a new scene with a Node
2. Attach this script:

```csharp
using Godot;
using Chickensoft.LogicBlocks;

public partial class TestScene : Node {
    private GameState _gameState;
    
    public override void _Ready() {
        _gameState = new GameState();
        GD.Print("ChickenSoft libraries initialized successfully!");
    }
}

public class GameState : LogicBlock<GameState.State> {
    public enum State { Active }
    public GameState() : base(State.Active) { }
}
```

## Common Issues

### 1. Build Errors

If you encounter build errors:

1. Clean the solution:
```bash
dotnet clean
```

2. Restore packages:
```bash
dotnet restore
```

3. Rebuild:
```bash
dotnet build
```

### 2. Runtime Errors

If you see runtime errors:

1. Check Godot's output panel for error messages
2. Verify all packages are properly referenced
3. Ensure Godot's .NET support is properly installed

### 3. IDE Issues

If your IDE doesn't recognize the packages:

1. Reload the project
2. Regenerate project files:
```bash
dotnet restore
```

## Next Steps

1. Read the [Basic Usage Guide](./basic-usage.md)
2. Explore the [API Documentation](../api/index.md)
3. Check out the [Best Practices Guide](./best-practices.md)

## Support

If you encounter any issues:

1. Check the [Troubleshooting Guide](./troubleshooting.md)
2. Visit the [ChickenSoft GitHub](https://github.com/chickensoft-games)
3. Join the community on Discord 