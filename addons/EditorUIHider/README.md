# Editor UI Hider

A Godot 4.3 addon that allows you to hide various UI elements in the Godot editor.

## Features

- Hide the Run/Build buttons in the top-right corner
- Hide the Main Screen Selector (2D, 3D, Script, etc.)
- Hide the Canvas Toolbar (Select, Move, etc.)

## Installation

1. Copy the `EditorUIHider` folder to your project's `addons` directory
2. Go to Project > Project Settings > Plugins
3. Enable the "Editor UI Hider" plugin

## Usage

Once enabled, the plugin adds a panel to the bottom-left dock with checkboxes for each UI element you can hide:

1. Check the boxes for the elements you want to hide
2. Click "Apply Changes" to hide the selected elements
3. Click "Reset All" to show all elements again

## Notes

- The hidden elements will reappear when you disable the plugin
- The plugin settings are not saved between editor sessions (yet)
- Some UI paths might need adjustment depending on your Godot version
- This plugin is written in GDScript and doesn't require compilation

## Compatibility

Tested with Godot 4.3.

## License

MIT License 