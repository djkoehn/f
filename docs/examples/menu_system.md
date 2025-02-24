---
title: "Menu System Example"
description: "Example implementation of a game menu system using Chickensoft"
---

# Menu System Example

This example demonstrates how to create a flexible menu system using Chickensoft's state machines and event system. We'll implement a main menu, options menu, and pause menu with smooth transitions and state management.

## Table of Contents
- [Project Setup](#project-setup)
- [Menu States](#menu-states)
- [Services](#services)
- [UI Components](#ui-components)
- [Implementation](#implementation)
- [Testing](#testing)

## Project Setup

1. Create the required directories:
```
UI/
├── States/
│   ├── MainMenuState.cs
│   ├── OptionsMenuState.cs
│   ├── PauseMenuState.cs
│   └── LoadingState.cs
├── Services/
│   ├── IUIService.cs
│   ├── UIService.cs
│   ├── IAudioService.cs
│   └── AudioService.cs
├── Components/
│   ├── MainMenu.tscn
│   ├── OptionsMenu.tscn
│   └── PauseMenu.tscn
└── MenuManager.cs
```

2. Install required packages:
```bash
dotnet add package Chickensoft.LogicBlocks
dotnet add package Chickensoft.AutoInject
dotnet add package Chickensoft.EventBus
```

## Menu States

### MenuContext

First, define the shared context for menu states:

```csharp
public class MenuContext
{
    public Control CurrentMenu { get; set; }
    public Dictionary<string, Control> MenuCache { get; set; }
    public bool IsTransitioning { get; set; }
    public float TransitionDuration { get; set; } = 0.3f;
}
```

### Events

Define events for menu navigation:

```csharp
public class MenuNavigationEvent
{
    public string TargetMenu { get; }
    public Dictionary<string, object> Parameters { get; }

    public MenuNavigationEvent(
        string targetMenu,
        Dictionary<string, object> parameters = null)
    {
        TargetMenu = targetMenu;
        Parameters = parameters ?? new Dictionary<string, object>();
    }
}

public class GamePausedEvent { }
public class GameResumedEvent { }
public class OptionsRequestedEvent { }
public class MainMenuRequestedEvent { }
```

### State Machine

Create the menu state machine:

```csharp
public partial class MenuStateMachine : StateMachine<MenuContext>
{
    protected override void Configure(IStateConfiguration<MenuContext> config)
    {
        // Set initial state
        config.SetInitialState<MainMenuState>();

        // Main Menu transitions
        config.AddTransition(t => t
            .From<MainMenuState>()
            .To<OptionsMenuState>()
            .On<OptionsRequestedEvent>()
            .When(context => !context.IsTransitioning)
        );

        config.AddTransition(t => t
            .From<MainMenuState>()
            .To<LoadingState>()
            .On<MenuNavigationEvent>()
            .When((context, evt) => evt.TargetMenu == "Game" &&
                                  !context.IsTransitioning)
        );

        // Options Menu transitions
        config.AddTransition(t => t
            .From<OptionsMenuState>()
            .To<MainMenuState>()
            .On<MainMenuRequestedEvent>()
            .When(context => !context.IsTransitioning)
        );

        // Pause Menu transitions
        config.AddTransition(t => t
            .From<MainMenuState>()
            .To<PauseMenuState>()
            .On<GamePausedEvent>()
            .When(context => !context.IsTransitioning)
        );

        config.AddTransition(t => t
            .From<PauseMenuState>()
            .To<MainMenuState>()
            .On<GameResumedEvent>()
            .When(context => !context.IsTransitioning)
        );
    }
}
```

### Individual States

#### MainMenuState

```csharp
public partial class MainMenuState : State<MenuContext>
{
    private readonly IUIService _ui;
    private readonly IAudioService _audio;

    public MainMenuState(IUIService ui, IAudioService audio)
    {
        _ui = ui;
        _audio = audio;
    }

    public override async void Enter(MenuContext context)
    {
        var menu = await _ui.LoadMenu<MainMenu>("res://UI/Components/MainMenu.tscn");
        
        // Cache menu if not already cached
        if (!context.MenuCache.ContainsKey("MainMenu"))
        {
            context.MenuCache["MainMenu"] = menu;
        }

        // Show menu with fade transition
        context.IsTransitioning = true;
        await _ui.TransitionTo(menu, context.TransitionDuration);
        context.IsTransitioning = false;
        
        // Play background music
        _audio.PlayMusic("main_menu");
        
        context.CurrentMenu = menu;
    }

    public override async void Exit(MenuContext context)
    {
        if (context.CurrentMenu != null)
        {
            // Fade out current menu
            context.IsTransitioning = true;
            await _ui.FadeOut(context.CurrentMenu, context.TransitionDuration);
            context.IsTransitioning = false;
        }
    }
}
```

#### OptionsMenuState

```csharp
public partial class OptionsMenuState : State<MenuContext>
{
    private readonly IUIService _ui;
    private readonly IAudioService _audio;

    public OptionsMenuState(IUIService ui, IAudioService audio)
    {
        _ui = ui;
        _audio = audio;
    }

    public override async void Enter(MenuContext context)
    {
        var menu = await _ui.LoadMenu<OptionsMenu>(
            "res://UI/Components/OptionsMenu.tscn");
        
        if (!context.MenuCache.ContainsKey("OptionsMenu"))
        {
            context.MenuCache["OptionsMenu"] = menu;
        }

        context.IsTransitioning = true;
        await _ui.TransitionTo(menu, context.TransitionDuration);
        context.IsTransitioning = false;
        
        _audio.PlaySound("menu_open");
        
        context.CurrentMenu = menu;
    }

    public override async void Exit(MenuContext context)
    {
        if (context.CurrentMenu != null)
        {
            context.IsTransitioning = true;
            await _ui.FadeOut(context.CurrentMenu, context.TransitionDuration);
            context.IsTransitioning = false;
            
            _audio.PlaySound("menu_close");
        }
    }
}
```

## Services

### IUIService

```csharp
public interface IUIService
{
    Task<T> LoadMenu<T>(string path) where T : Control;
    Task TransitionTo(Control menu, float duration);
    Task FadeOut(Control menu, float duration);
    void ClearMenus();
}
```

### UIService

```csharp
public class UIService : IUIService
{
    private readonly PackedScene _fadeTransition;
    private readonly Node _uiRoot;

    public UIService(Node uiRoot)
    {
        _uiRoot = uiRoot;
        _fadeTransition = GD.Load<PackedScene>(
            "res://UI/Components/FadeTransition.tscn");
    }

    public async Task<T> LoadMenu<T>(string path) where T : Control
    {
        var scene = GD.Load<PackedScene>(path);
        var menu = scene.Instantiate<T>();
        _uiRoot.AddChild(menu);
        menu.Modulate = new Color(1, 1, 1, 0);
        return menu;
    }

    public async Task TransitionTo(Control menu, float duration)
    {
        var tween = menu.CreateTween();
        tween.TweenProperty(menu, "modulate:a", 1.0f, duration);
        await ToSignal(tween, "finished");
    }

    public async Task FadeOut(Control menu, float duration)
    {
        var tween = menu.CreateTween();
        tween.TweenProperty(menu, "modulate:a", 0.0f, duration);
        await ToSignal(tween, "finished");
        menu.QueueFree();
    }

    public void ClearMenus()
    {
        foreach (var child in _uiRoot.GetChildren())
        {
            if (child is Control)
            {
                child.QueueFree();
            }
        }
    }
}
```

### IAudioService

```csharp
public interface IAudioService
{
    void PlayMusic(string track);
    void PlaySound(string sound);
    void StopMusic();
    void SetVolume(float volume);
}
```

## UI Components

### MainMenu.tscn

```gdscript
[gd_scene load_steps=2 format=3]
[ext_resource type="Script" path="res://UI/Components/MainMenu.cs" id=1]

[node name="MainMenu" type="Control"]
anchor_right = 1.0
anchor_bottom = 1.0
script = ExtResource("1")

[node name="VBoxContainer" type="VBoxContainer"]
anchor_left = 0.5
anchor_top = 0.5
anchor_right = 0.5
anchor_bottom = 0.5
offset_left = -100.0
offset_top = -100.0
offset_right = 100.0
offset_bottom = 100.0

[node name="StartButton" type="Button"]
text = "Start Game"

[node name="OptionsButton" type="Button"]
text = "Options"

[node name="QuitButton" type="Button"]
text = "Quit"
```

### MainMenu.cs

```csharp
public partial class MainMenu : Control
{
    public override void _Ready()
    {
        GetNode<Button>("VBoxContainer/StartButton")
            .Pressed += OnStartPressed;
        GetNode<Button>("VBoxContainer/OptionsButton")
            .Pressed += OnOptionsPressed;
        GetNode<Button>("VBoxContainer/QuitButton")
            .Pressed += OnQuitPressed;
    }

    private void OnStartPressed()
    {
        EventBus.Publish(new MenuNavigationEvent("Game"));
    }

    private void OnOptionsPressed()
    {
        EventBus.Publish(new OptionsRequestedEvent());
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();
    }
}
```

## Implementation

### MenuManager

```csharp
public partial class MenuManager : Node
{
    private MenuStateMachine _stateMachine;
    private MenuContext _context;

    public override void _Ready()
    {
        // Initialize services
        Service.Initialize();
        Service.Register<IUIService>(new UIService(this));
        Service.Register<IAudioService>(new AudioService());

        // Initialize context
        _context = new MenuContext
        {
            MenuCache = new Dictionary<string, Control>(),
            TransitionDuration = 0.3f
        };

        // Initialize state machine
        _stateMachine = new MenuStateMachine();
        _stateMachine.Start(_context);

        // Subscribe to game events
        EventBus.Subscribe<GamePausedEvent>(OnGamePaused);
        EventBus.Subscribe<GameResumedEvent>(OnGameResumed);
    }

    public override void _Process(double delta)
    {
        // Handle pause input
        if (Input.IsActionJustPressed("pause"))
        {
            if (_stateMachine.CurrentState is PauseMenuState)
            {
                EventBus.Publish(new GameResumedEvent());
            }
            else if (_stateMachine.CurrentState is not MainMenuState)
            {
                EventBus.Publish(new GamePausedEvent());
            }
        }

        _stateMachine.Update();
    }

    private void OnGamePaused(GamePausedEvent evt)
    {
        GetTree().Paused = true;
    }

    private void OnGameResumed(GameResumedEvent evt)
    {
        GetTree().Paused = false;
    }

    public override void _ExitTree()
    {
        // Cleanup
        _stateMachine.Stop();
        Service.Reset();
        EventBus.UnsubscribeAll(this);
    }
}
```

## Testing

### Menu State Tests

```csharp
public class MenuStateTests
{
    [Test]
    public void MainMenu_WhenOptionsRequested_TransitionsToOptions()
    {
        // Arrange
        var uiService = new Mock<IUIService>();
        var audioService = new Mock<IAudioService>();
        var context = new MenuContext
        {
            MenuCache = new Dictionary<string, Control>(),
            IsTransitioning = false
        };

        var stateMachine = new MenuStateMachine();
        stateMachine.Start(context);

        // Act
        EventBus.Publish(new OptionsRequestedEvent());

        // Assert
        Assert.That(stateMachine.CurrentState, Is.TypeOf<OptionsMenuState>());
    }

    [Test]
    public void Menu_WhenTransitioning_DoesNotAllowStateChange()
    {
        // Arrange
        var context = new MenuContext
        {
            MenuCache = new Dictionary<string, Control>(),
            IsTransitioning = true
        };

        var stateMachine = new MenuStateMachine();
        stateMachine.Start(context);

        // Act
        EventBus.Publish(new OptionsRequestedEvent());

        // Assert
        Assert.That(stateMachine.CurrentState, Is.TypeOf<MainMenuState>());
    }
}
```

## Usage

1. Create a new scene for the MenuManager
2. Add UI components (MainMenu.tscn, OptionsMenu.tscn, etc.)
3. Configure input actions in Project Settings:
   - `pause` (Escape key)
4. Add the MenuManager to your main scene
5. Run the game

## Best Practices

1. Cache frequently used menus
2. Use smooth transitions between states
3. Handle edge cases (e.g., rapid menu switching)
4. Clean up resources properly
5. Keep UI logic separate from game logic
6. Test menu flow thoroughly

## Common Issues

### Menu Not Appearing
- Check scene paths
- Verify UI service initialization
- Debug menu instantiation

### Transitions Not Working
- Check tween setup
- Verify transition duration
- Debug transition flags

### Input Issues
- Verify input action configuration
- Check input handling logic
- Debug event publishing

## Next Steps

1. Add more features:
   - Menu animations
   - Transition effects
   - Sound effects
   - Save/load system

2. Enhance UI:
   - Responsive design
   - Theme support
   - Localization
   - Accessibility features

3. Improve functionality:
   - Menu history
   - Menu parameters
   - Dynamic menu loading
   - Menu templates

## Additional Resources

- [State Machine Documentation](../api.md#logicblocks)
- [Event System Overview](../api.md#eventbus)
- [UI Best Practices](../api.md#ui-guidelines)
- [Testing Guide](../contributing.md#testing-guidelines) 