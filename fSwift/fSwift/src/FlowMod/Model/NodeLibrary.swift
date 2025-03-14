//
//  NodeLibrary.swift
//  fSwift
//
//  Created by dj koehn on 3/14/25.
//


import SwiftUI

/// A library of predefined node types that can be easily added to a patch
public class NodeLibrary {
    /// Shared instance for easy access
    public static let shared = NodeLibrary()

    /// Dictionary of node templates indexed by type name
    private var nodeTemplates: [String: Node] = [:]

    // Private initializer for singleton
    private init() {
        registerDefaultNodes()
    }

    /// Register a new node type in the library
    public func registerNode(type: String, node: Node) {
        nodeTemplates[type] = node
    }

    /// Create a node instance of a given type at a specific position
    public func createNode(type: String, at position: CGPoint) -> Node? {
        guard var node = nodeTemplates[type] else {
            print("Node type '\(type)' not found in library")
            return nil
        }

        // Set the position for the new instance
        node.position = position
        return node
    }

    /// Register the default node types
    private func registerDefaultNodes() {
        // Input node
        registerNode(type: "input", node: Node(
            name: "Input",
            titleBarColor: .green,
            locked: true,
            outputs: ["out"],
            image: Image("input"),
            processor: { value in return value }
        ))

        // Output node
        registerNode(type: "output", node: Node(
            name: "Output",
            titleBarColor: .purple,
            locked: true,
            inputs: ["in"],
            image: Image("output"),
            processor: { value in return value }
        ))

        // Math nodes
        registerNode(type: "double", node: Node(
            name: "Double",
            titleBarColor: .red,
            inputs: ["in"],
            outputs: ["out"],
            image: Image("double"),
            processor: { value in
                if case let .number(num) = value {
                    return .number(num * 2)
                }
                return value
            }
        ))
        

        registerNode(type: "add10", node: Node(
            name: "Add 10",
            titleBarColor: .blue,
            inputs: ["in"],
            outputs: ["out"],
            processor: { value in
                if case let .number(num) = value {
                    return .number(num + 10)
                }
                return value
            }
        ))

        registerNode(type: "multiply", node: Node(
            name: "Multiply",
            titleBarColor: .orange,
            inputs: ["in", "factor"],
            outputs: ["out"],
            processor: { value in
                // Note: In a real implementation, you would need to handle
                // the second input "factor" - this is simplified
                if case let .number(num) = value {
                    return .number(num * 2) // Using default factor of 2
                }
                return value
            }
        ))

        registerNode(type: "negate", node: Node(
            name: "Negate",
            titleBarColor: .red,
            inputs: ["in"],
            outputs: ["out"],
            processor: { value in
                if case let .number(num) = value {
                    return .number(-num)
                }
                return value
            }
        ))
}

    /// Add a custom node type
    public func addCustomNode(type: String, name: String, color: Color, inputs: [String], outputs: [String], processor: @escaping NodeProcessor) {
        let node = Node(
            name: name,
            titleBarColor: color,
            inputs: inputs,
            outputs: outputs,
            processor: processor
        )
        registerNode(type: type, node: node)
    }
}

// Extension with convenience functions for working with the node library
public extension Patch {
    /// Add a node from the library to this patch
    mutating func addNode(type: String, at position: CGPoint) -> NodeIndex? {
        guard let node = NodeLibrary.shared.createNode(type: type, at: position) else {
            return nil
        }

        nodes.append(node)
        return nodes.count - 1
    }

    /// Connect two nodes in the patch
    mutating func connect(from sourceNodeIndex: NodeIndex, output: PortIndex = 0,
                          to destNodeIndex: NodeIndex, input: PortIndex = 0) {
        let wire = Wire(
            from: OutputID(sourceNodeIndex, output),
            to: InputID(destNodeIndex, input)
        )
        wires.insert(wire)
    }

    /// Create a simple chain of nodes
    mutating func createChain(types: [String], startingAt position: CGPoint, spacing: CGFloat = 200) {
        var lastNodeIndex: NodeIndex?
        var currentPosition = position

        for type in types {
            // Add the node
            if let nodeIndex = addNode(type: type, at: currentPosition) {
                // Connect to previous node if there is one
                if let previousNodeIndex = lastNodeIndex {
                    connect(from: previousNodeIndex, to: nodeIndex)
                }

                lastNodeIndex = nodeIndex
                currentPosition.x += spacing
            }
        }
    }
}
