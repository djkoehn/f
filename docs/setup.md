---
title: "Setup Guide"
description: "Installation and configuration guide for Chickensoft"
---

# Setup Guide

This guide will walk you through setting up Chickensoft in your Godot project. Follow these steps carefully to ensure a smooth installation process.

## Prerequisites

Before installing Chickensoft, make sure you have:

- [Godot 4.x](https://godotengine.org/download) with .NET/C# support
- [.NET SDK 6.0+](https://dotnet.microsoft.com/download)
- A code editor (we recommend [Visual Studio Code](https://code.visualstudio.com/) with the C# extension)

## Installation Steps

### 1. Create a New Godot Project

1. Open Godot and create a new project
2. Enable C# support in your project:
   ```bash
   # In your project directory
   dotnet new godot
   ```

### 2. Install Chickensoft Packages

Add the required Chickensoft NuGet packages to your project:

```bash
# Core packages
dotnet add package Chickensoft.LogicBlocks
dotnet add package Chickensoft.AutoInject
dotnet add package Chickensoft.EventBus

# Optional but recommended packages
dotnet add package Chickensoft.GodotNodeInterfaces
dotnet add package Chickensoft.Serialization
```

### 3. Configure Your Project

1. Update your `.csproj` file to include Chickensoft configurations:

```xml
<Project Sdk="Godot.NET.Sdk/4.3.0">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <EnableDynamicLoading>true</EnableDynamicLoading>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  
  <ItemGroup>
    <!-- Your package references here -->
  </ItemGroup>
</Project>
```

2. Create a basic project structure:

```
YourProject/
├── src/
│   ├── States/        # State machine definitions
│   ├── Events/        # Custom event definitions
│   ├── Services/      # Service implementations
│   └── Components/    # Reusable game components
├── tests/            # Test files
└── scenes/           # Godot scene files
```

### 4. Initialize Chickensoft

1. Create a main game script (e.g., `Game.cs`):

```csharp
using Godot;
using Chickensoft.AutoInject;
using Chickensoft.EventBus;

public partial class Game : Node
{
    public override void _Ready()
    {
        // Initialize services
        Service.Initialize();
        
        // Register your services
        Service.Register<IGameService, GameService>();
        
        // Initialize EventBus
        EventBus.Initialize();
    }

    public override void _ExitTree()
    {
        // Cleanup
        EventBus.Dispose();
        Service.Reset();
    }
}
```

2. Attach this script to your main scene.

## Verification

To verify your installation:

1. Build your project:
```bash
dotnet build
```

2. Run the following test script:

```csharp
using Godot;
using Chickensoft.LogicBlocks;

public class TestState : State<TestContext>
{
    public override void Enter(TestContext context)
    {
        GD.Print("Chickensoft is working!");
    }
}
```

If everything is set up correctly, you should see no errors.

## IDE Setup

### Visual Studio Code

1. Install recommended extensions:
   - C# Dev Kit
   - Godot Tools
   - .NET Core Test Explorer

2. Add the following to your `.vscode/settings.json`:

```json
{
  "godot_tools.gdscript_lsp_server_port": 6005,
  "dotnet.defaultSolution": "YourProject.sln"
}
```

### Visual Studio

1. Install the following extensions:
   - .NET Core Test Explorer
   - Godot Tools for Visual Studio

2. Configure debugging:
   - Set the startup project to your game project
   - Add the Godot executable path in debug settings

## Common Issues

### Missing Dependencies
If you see "Could not load type..." errors:
1. Clean your solution: `dotnet clean`
2. Restore packages: `dotnet restore`
3. Rebuild: `dotnet build`

### Runtime Errors
If you encounter runtime errors:
1. Check that all services are properly registered
2. Ensure EventBus is initialized before use
3. Verify state machine configurations

### Build Errors
For build errors:
1. Verify your .NET SDK version
2. Check your .csproj configuration
3. Ensure all required packages are installed

## Next Steps

Now that you have Chickensoft installed and configured:

1. Follow the [Basic Usage Guide](./usage.md) to learn the fundamentals
2. Check out the [Examples](./examples/) for practical demonstrations
3. Read the [API Documentation](./api.md) for detailed reference

## Additional Configuration

### Testing Setup

1. Add test project:
```bash
dotnet new xunit -o tests
```

2. Add Chickensoft test packages:
```bash
cd tests
dotnet add package Chickensoft.GoDotTest
```

### CI/CD Configuration

If you're using CI/CD, add these environment variables:
- `GODOT_VERSION`: Your Godot version
- `DOTNET_VERSION`: Your .NET SDK version

### Performance Optimization

For better performance:
1. Enable assembly trimming in your .csproj
2. Configure appropriate garbage collection settings
3. Use conditional compilation for debug features

## Support

If you encounter any issues during setup:
1. Check the [Troubleshooting](./troubleshooting.md) guide
2. Visit our [Discord community](https://discord.gg/chickensoft)
3. Open an issue on [GitHub](https://github.com/chickensoft-games/issues) 