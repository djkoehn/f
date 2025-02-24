# F – A Node-Based Baseball Roguelike Game

## Project Structure

### Root Files
- `GlobalUsings.cs`: Global using directives
- `README.md`: Project documentation
- `project.godot`: Godot project configuration
- `icon.svg`: Project icon
- `godot.log`: Godot logging
- `BlockMetaData.json`: JSON file containing block metadata and configuration
- `Inventory.json`: Block inventory configuration

### Assets (`assets/`)
- `audio/`: Sound effects and music
- `fonts/`: Typography assets
- `images/`: Visual assets and sprites
- `shaders/`: Shader files for visual effects
  - `BlockLayerSkew.gdshader`: Visual effects for block layer
  - `SpaceBackground.gdshader`: Background effects

### Scenes (`scenes/`)
- `Main.tscn`: Primary game scene
- `Blocks/`: Block-related scenes
  - `Input.tscn`: Input block scene
  - `Output.tscn`: Output block scene
- `UI/`: Interface scenes
  - `Connection.tscn`: Connection pipe visuals
  - `Token.tscn`: Token visuals
  - `Toolbar.tscn`: Toolbar interface
- `Services/`: Core services
  - `Services.tscn`: Global service autoload
  - `SceneTreeService.tscn`: Scene tree management

## Core Architecture

### Service System
1. Global Services (`Services.cs`):
   - Autoloaded singleton managing core services
   - Handles initialization and dependency injection
   - Provides access to:
     - Game Manager
     - Input Manager
     - Connection Manager
     - Block Manager
     - Token Manager
     - Inventory
     - Block Metadata

2. Scene Tree Service:
   - Manages scene hierarchy
   - Provides path resolution
   - Handles node relationships

### Block System

#### Block Types
1. Interface Layer:
   - `IBlock`: Core block interface
   - Metadata-driven configuration:
     - SpawnOnSpace: Token generation control
     - DisplayValue: Value display toggle
     - ProcessTokenScript: Token processing logic
     - Socket configuration
     - State management

2. Block Hierarchy:
   - Stationary Blocks:
     - `Input`: Token generator
     - `Output`: Token collector
   - Dynamic Blocks:
     - `BaseBlock`: Foundation class
       - State machine integration
       - Parent management
       - Token processing
       - Socket handling
     - `ConnectionBlock`: Connection-aware block
     - `TokenBlock`: Token-handling block
     - `ToolbarBlock`: Toolbar-specific block

#### State Management
1. Block Logic Machine:
   - Implementation using Chickensoft.LogicBlocks v5.16.0
   - States:
     - InToolbar: Initial state for blocks in toolbar
     - Dragging: Active when block is being dragged
     - Placed: Block positioned in game area
     - Connected: Block connected to pipe system
     - ConnectedAndDragging: Connected block being repositioned
   - Transitions:
     - Interact: User interaction trigger
     - ReturnBlock: Return to toolbar action
     - HoveredOverPipe: Connection opportunity detected
   - Features:
     - State change event system
     - Automatic parent management
     - Logging integration
     - Null safety checks

### Connection System

#### Core Components
1. `ConnectionManager`:
   - Pipe management
   - Block connection handling
   - Connection validation
   - Visual feedback

2. `ConnectionPipe`:
   - Bezier curve visualization
   - Token movement effects
   - Interaction detection
   - Shader-based effects

3. Connection Factory:
   - Pipe creation
   - Connection validation
   - Block compatibility checks

#### Visual System
- Shader-driven effects:
  - Dynamic bulge visualization
  - Token progress indicators
  - Hover effects
- Z-index management:
  - Background: -10
  - Pipes: -5
  - Blocks: -4
  - Tokens: -1
  - UI: 15+

### Input System

#### Input Manager
1. Core Functionality:
   - Block dragging
   - Connection handling
   - Token spawning
   - Toolbar interaction

2. Input Actions:
   - Left click: Block interaction
   - Right click: Return to toolbar
   - Space: Spawn token

### Token System

#### Token Manager
1. Features:
   - Token creation
   - Movement handling
   - Value processing
   - Visual effects

2. Token Types:
   - Standard tokens
   - Value-carrying tokens
   - Visual feedback tokens

### UI System

#### Toolbar
1. Components:
   - `Toolbar`: Main container
   - `ToolbarBlockContainer`: Block layout
   - `ToolbarVisuals`: Visual effects
   - `ToolbarBlock`: Block variant

2. Features:
   - Dynamic block positioning
   - Hover animations
   - Block spawning
   - Return animations

### Configuration System

#### Block Configuration
- `BlockMetaData.json`: Block definitions
- `Inventory.json`: Available blocks
- `ZIndexConfig`: Layer management
- Enhanced metadata:
  - Socket configuration
  - Processing scripts
  - Visual parameters

### Debug System
- Integrated Chickensoft.Log.Godot logging framework
- Categorized logging:
  - Block: Block-related operations
  - Connection: Pipe and connection events
  - Game: Core game events
  - UI: Interface interactions
  - Token: Token management
- Scene tree analysis
- State tracking
- Connection debugging
- Performance monitoring

## Dependencies
- Chickensoft.AutoInject (2.5.0): Dependency injection
- Chickensoft.LogicBlocks (5.16.0): State management
- Chickensoft.LogicBlocks.Generator (4.2.2): State machine code generation
- Chickensoft.LogicBlocks.DiagramGenerator (5.16.0): State visualization
- Chickensoft.GodotNodeInterfaces (2.4.0): Node abstraction
- Chickensoft.Collections (1.13.0): Data structures
- Chickensoft.Log (1.0.0): Core logging functionality
- Chickensoft.Log.Godot (1.0.0): Godot-specific logging
- Chickensoft.Introspection (2.2.0): Runtime type inspection
- Chickensoft.Introspection.Generator (2.2.0): Metadata generation
- Chickensoft.GoDotCollections (1.4.4): Godot-specific collections
- Chickensoft.SaveFileBuilder (1.1.0): Save data management
- Chickensoft.Serialization (2.2.0): Data serialization
- Chickensoft.Serialization.Godot (0.7.5): Godot type serialization 