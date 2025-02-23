# F – A Node-Based Baseball Roguelike Game

## Project Structure

### Root Files
- `GlobalUsings.cs`: Global using directives
- `README.md`: Project documentation
- `project.godot`: Godot project configuration
- `icon.svg`: Project icon
- `godot.log`: Godot logging
- `BlockMetadata.json`: JSON file containing block metadata and configuration

### Assets (`assets/`)
- `audio/`: Sound effects and music
- `fonts/`: Typography assets
- `images/`: Visual assets and sprites
- `shaders/`: Shader files for visual effects

### Scenes (`scenes/`)
- `Main.tscn`: Primary game scene
- `Blocks/`: Block-related scenes
  - `Input.tscn`: Input block scene (IBlock implementation)
  - `Output.tscn`: Output block scene (IBlock implementation)
  - `Add.tscn`: Operation block scene (now driven by BaseBlock and JSON metadata)
- `UI/`: Interface scenes
- `Utils/`: Utility scenes including AudioManager

## Core Architecture

### Block System

#### Block Types
1. Interface Layer:
   - `IBlock`: Interface defining block behavior including token processing and socket management
   - New metadata-driven configuration system with properties:
     - SpawnOnSpace: Controls token generation behavior
     - DisplayValue: Toggles value display
     - ProcessTokenScript: Defines block-specific token processing

2. Block Hierarchy:
   - Stationary Blocks:
     - `Input`: Generates tokens; fixed position.
     - `Output`: Collects tokens; fixed position.
   - Dynamic Blocks:
     - `BaseBlock`: Base for all placeable blocks; inherits Node2D and implements IBlock.
       - Handles dragging, state management (InToolbar/Dragging/Placed/Connected), and token processing.
       - Uses JSON metadata (via BlockMetadata) to drive block-specific behavior.
       - Integrated shader support
       - Dynamic token processing via scripting
       - Debug logging system
       - State management improvements
     - Operation Blocks:
       - Operation behavior (e.g. Add) is now defined via JSON metadata and processed by BaseBlock.
       - This approach removes the need for separate block-specific classes.

#### Socket System
- Node2D-based socket architecture.
- Socket Types:
  - Input Socket ("BlockInputSocket")
  - Output Socket ("BlockOutputSocket")
- Socket Distribution:
  - Input blocks (IBlock): have output sockets.
  - Output blocks (IBlock): have input sockets.
  - BaseBlock derivatives: include both sockets.

### Connection System

#### Core Components
1. `ConnectionManager`: Manages active connections, maps blocks to pipes, and handles the Input->Block->Output flow.
2. `ConnectionPipe`: Visual representation of connections using Bezier curves and precise distance calculations.
3. `ConnectionFactory`: Creates standard connections, validates sockets and block compatibility.
4. `ConnectionValidator`: Checks bounds, compatibility, and rewiring constraints.
- Enhanced PipeVisuals:
  - Shader-based effects
  - Token progress visualization
  - Bulge effects for token movement
  - Debug logging system

#### Connection Flow
1. Initial Setup:
   - Direct connection between stationary blocks.
2. Block Insertion:
   - Inserting a BaseBlock into an existing connection splits the connection into two segments.
3. Pipe Detection:
   - Uses curve-based hit detection with visual feedback.

#### Visual System
- Shader-driven pipe effects
- Dynamic bulge visualization
- Token progress indicators
- Configurable visual parameters

### Game Management (`F.Game.Core`)

1. `GameManager`: Central game state management and block coordination.
2. `Inventory`: Manages block availability and metadata (using BlockMetadata).
3. `BlockInteractionManager`: Handles input, block selection, and drag operations.
4. `BlockFactory`: Instantiates blocks using metadata and configures them appropriately.

### UI System (`F.Game.Toolbar`)

1. Core Components:
   - `Toolbar`: Main toolbar container.
   - `ToolbarBlockContainer`: Manages block layout in the toolbar.
   - `ToolbarVisuals`: Visual components for the toolbar.
2. Block Management:
   - Blocks are created using the metadata-driven system and managed in the toolbar.
   - Dedicated helper classes (e.g. `ToolbarHelper`) assist with block repositioning.

#### Core Components
- Precise positioning system:
  - Block width: 96f
  - Block height: 96f
  - Block spacing: 64f
  - Configurable animation durations

#### Layer Management
- Structured Z-index system:
  - Background: -10
  - Pipes: -5
  - Blocks: -4
  - Tokens: -1
  - UI: 15+

### Configuration System

#### Block Configuration
- `BlockMetadata.json`: Contains JSON metadata for block configurations (ScenePath, ProcessTokenScript, Name, Description).
- `BlockConfig`: Block-specific settings.
- `PipeConfig`: Connection pipe settings.
- `TokenConfig`: Token behavior settings.
- `ToolbarConfig`: UI component settings.
- `ZIndexConfig`: Manages visual layering.
- Enhanced metadata properties:
  - SpawnOnSpace
  - DisplayValue
  - ProcessTokenScript
  - Socket configuration

#### Visual Configuration
- PipeConfig:
  - Animation settings
  - Visual parameters
  - Shader effects
- ZIndexConfig:
  - Layered rendering system
  - Relative/absolute positioning

### Debug System
- Comprehensive logging
- Visual debugging
- State tracking
- Performance monitoring

## Audio System

### Sound Management
- `AudioManager`: Central audio control.
- `BlockSoundPlayer`: Plays block-specific sounds.
- `TokenSoundPlayer`: Plays token-related sounds.

## Multi-Channel Processing (MCP)

### Benefits
- Independent processing channels.
- Improved performance.
- Scalable architecture.
- Flexible event handling.
- Enhanced debugging.
- Responsive gameplay.

### Implementation
- Separate managers for different systems.
- Event-driven communication.
- State management per channel.
- Synchronized updates.
- Performance optimization 