# F - Game Project Brief

this is a video game called F.
it consists of a blender-node-editor like interface where users can place "blocks" and connect them together with "pipes" to send a "token" through.
this token has a base value, and the blocks manipulate that value until they reach the output block, where their final value is appended to a round score.

i don't know where it will go beyond this just yet.



## Technical Overview

### Game Engine & Framework
- Godot 4.3 (Mono version)
- Primary Languages:
  - C# (Backend/Game Logic)
  - GDScript (Frontend/UI)
- ChickenSoft Framework Stack:
  - Based on ChickenSo
  - LogicBlocks for state management
  - AutoInject for dependency injection
  - Introspection for reflection capabilities
  - Serialization for data persistence

### Architecture Design

#### Frontend (GDScript)
- Responsible for:
  - User Interface
  - Scene Management
  - Visual Components
  - Input Handling
  - Scene-specific Logic

#### Backend (C#)
- Core Game Logic
- State Management
- Data Processing
- Business Rules
- System Architecture

### Development Guidelines

#### Code Organization
1. **State Management**
   - Use LogicBlocks for managing game states
   - Implement state machines in separate partial classes
   - Keep states immutable and well-defined

2. **Dependency Injection**
   - Utilize AutoInject for dependency management
   - Follow constructor injection pattern
   - Implement IAutoNode for nodes requiring injection

3. **Scene Structure**
   - Maintain clear separation between UI and logic
   - Use signals for communication between nodes
   - Implement proper scene inheritance

#### Best Practices

1. **SOLID Principles**
   - Single Responsibility Principle
   - Open/Closed Principle
   - Liskov Substitution Principle
   - Interface Segregation
   - Dependency Inversion

2. **Domain-Driven Design**
   - Clear bounded contexts
   - Rich domain model
   - Aggregate roots
   - Value objects
   - Domain services

3. **Testing**
   - Unit tests for C# logic
   - Integration tests for critical paths
   - Mock interfaces for Godot nodes
   - Test automation where applicable

### Development Workflow

1. **Build and Testing**
   ```bash
   # Build the project
   dotnet build
   
   # Run the game
   /Applications/Godot_mono.app/Contents/MacOS/Godot
   ```

2. **Code Review Guidelines**
   - Ensure adherence to SOLID principles
   - Verify proper use of ChickenSoft frameworks
   - Check for proper separation of concerns
   - Validate test coverage

### Performance Considerations

1. **Optimization**
   - Minimize garbage collection
   - Efficient state management
   - Resource pooling
   - Scene optimization

2. **Memory Management**
   - Proper resource cleanup
   - Smart use of weak references
   - Efficient asset loading
   - Scene unloading

### Security

1. **Data Protection**
   - Secure serialization
   - Protected save files
   - Input validation
   - Safe resource loading

### Maintainability

1. **Documentation**
   - Code documentation
   - API documentation
   - Architecture documentation
   - Setup instructions

2. **Version Control**
   - Clear commit messages
   - Feature branching
   - Version tagging
   - Changelog maintenance

This brief serves as a living document and should be updated as the project evolves.
