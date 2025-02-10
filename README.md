# Node-Based Roguelike

A roguelike game built with Godot 4.3 Mono where you connect nodes to process tokens and achieve high scores.

## Project Structure

```
f/
├── src/
│   ├── Blocks/       # Block-related scripts
│   │   ├── BaseBlock.cs  # Base block implementation for all types
│   │   ├── Token.cs  # Token implementation
│   │   └── Connection.cs
│   ├── UI/          # UI components
│   └── Game/        # Core game logic
├── scenes/         # Godot scene files
│   ├── Main.tscn   # Main game scene
│   ├── BaseBlock.tscn  # Base block scene (configurable for all types)
│   ├── Token.tscn
│   └── Connection.tscn
└── assets/
    ├── fonts/      # Game fonts
    ├── audio/      # Sound effects
    └── shaders/    # Visual effects
```

## Game Mechanics

- **Tokens**: Start with 3 tokens per round, press SPACE to spawn
- **Blocks**: Connect blocks to create token processing paths
  - Input Blocks (Green): Generate tokens with initial values
  - Operation Blocks (Blue/Purple): Modify token values
    - Add: Adds a fixed value
    - Multiply: Multiplies by a fixed value
  - Output Blocks (Red): Collect processed tokens for scoring

## Controls

- SPACE: Spawn token from random input block
- Left Click: Select and drag blocks
- Right Click: Return block to toolbar

## Block Types

All blocks are instances of a base Block scene, configured through the BlockType enum:
- Input: Generates tokens with a starting value
- Output: Collects tokens and adds to score
- Add: Adds its value to passing tokens
- Multiply: Multiplies passing tokens by its value

## Development

- Blocks are 256x256 pixels with adjusted socket placement.
- Shader effects are implemented using `shadowtest.gdshader`.
