---
title: "Introduction to Chickensoft"
description: "Overview of Chickensoft and its benefits for game development"
---

# Introduction to Chickensoft

## What is Chickensoft?

Chickensoft is a comprehensive game development framework for Godot 4.x and C#, designed to bring modern software engineering practices to game development. It provides a collection of powerful tools and libraries that work together seamlessly to help you build maintainable, scalable games.

## Why Choose Chickensoft?

### 1. Modern Architecture
- **Clean, Modular Design** - Build your game using proven software design patterns
- **Type Safety** - Leverage C#'s strong type system for safer code
- **Dependency Injection** - Manage dependencies effectively with AutoInject
- **Event-Driven Architecture** - Implement decoupled communication between game components

### 2. State Management Made Easy
- **Powerful State Machines** - Model complex game behaviors using LogicBlocks
- **Hierarchical States** - Create nested state machines for advanced logic
- **Visual Debugging** - Generate state diagrams for easier debugging
- **Type-Safe Transitions** - Define state transitions with compile-time checking

### 3. Developer Experience
- **Fast Iteration** - Quick compile times and hot reload support
- **Great Tooling** - IDE support with code completion and refactoring
- **Testing Support** - Comprehensive testing utilities for TDD
- **Clear Error Messages** - Helpful diagnostics when things go wrong

## Core Components

### LogicBlocks
LogicBlocks is the heart of Chickensoft's state management system. It allows you to:
- Define states and transitions declaratively
- Handle complex game behaviors with ease
- Visualize state machines with automatic diagram generation
- Test state logic independently of your game

### AutoInject
AutoInject brings modern dependency injection to Godot:
- Automatic node binding and lifecycle management
- Constructor injection for clean dependency management
- Scoped and singleton services
- Easy testing with dependency mocking

### EventBus
The EventBus system enables clean communication between game components:
- Type-safe event publishing and subscription
- Automatic event cleanup
- Support for async event handling
- Easy testing with event verification

### Additional Tools
- **GodotNodeInterfaces** - Test-friendly node abstractions
- **Serialization** - Efficient game data serialization
- **SaveFileBuilder** - Structured save file management
- **Collections** - Enhanced collection utilities

## Getting Started

Ready to start building with Chickensoft? Check out our [Setup Guide](./setup.md) for installation instructions and basic configuration.

For hands-on examples, visit the [Examples](./examples/) section to see Chickensoft in action.

## Design Philosophy

Chickensoft is built on several key principles:

### 1. Type Safety First
We believe that catching errors at compile-time is better than at runtime. Chickensoft leverages C#'s type system to prevent common mistakes and provide better tooling support.

### 2. Testability by Design
Every component in Chickensoft is designed with testing in mind. This makes it easy to write unit tests, integration tests, and maintain high code quality.

### 3. Developer Productivity
Chickensoft aims to reduce boilerplate while maintaining flexibility. Common patterns are made easy, but you're never locked into a specific approach.

### 4. Performance Conscious
While focusing on clean architecture, Chickensoft is designed to be performant, with careful attention to memory allocation and processing overhead.

## Real-World Usage

Chickensoft is being used in various game projects, from small indie games to larger commercial titles. Some examples include:
- Action-adventure games with complex character states
- Strategy games with intricate UI flows
- Platformers with precise input handling
- RPGs with complex inventory and combat systems

## Next Steps

- Follow the [Setup Guide](./setup.md) to install Chickensoft
- Check out the [Basic Usage](./usage.md) guide for your first steps
- Explore [Examples](./examples/) for practical demonstrations
- Join our [Discord community](https://discord.gg/chickensoft) for support

## Additional Resources

- [API Documentation](./api.md) - Detailed API reference
- [FAQ](./faq.md) - Common questions and answers
- [Troubleshooting](./troubleshooting.md) - Solutions to common issues
- [Contributing](./contributing.md) - How to contribute to Chickensoft 