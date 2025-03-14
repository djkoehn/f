//
//  TestGameView.swift
//  fSwift
//
//  Created by dj koehn on 3/14/25.
//


import SwiftUI

struct TestGameView: View {
    @State private var patch: Patch
    @State private var selection = Set<NodeIndex>()
    @StateObject private var tokenProcessor: Node.TokenProcessor
    @State private var selectedNodeType: String = "double"
    
    // Available node types to add from the library
    let availableNodeTypes = ["double", "add10", "negate", "multiply"]
    
    init() {
        // Create initial patch with input and output nodes
        let initialPatch = createInitialPatch()
        _patch = State(initialValue: initialPatch)
        _tokenProcessor = StateObject(wrappedValue: Node.TokenProcessor(patch: initialPatch))
    }
    
    var body: some View {
        VStack {
            // Display token values
            VStack {
                HStack {
                    Text("Input Value: \(tokenValueString(tokenProcessor.inputValue))")
                        .font(.headline)
                    Spacer()
                    Text("Output Value: \(tokenValueString(tokenProcessor.outputValue))")
                        .font(.headline)
                        .foregroundColor(tokenProcessor.outputValue != .none ? .green : .primary)
                }
                .padding()
                .background(Color.gray.opacity(0.2))
                .cornerRadius(8)
                
                // Show processing steps for debugging
//                if !tokenProcessor.processingSteps.isEmpty {
//                    ScrollView {
//                        VStack(alignment: .leading, spacing: 4) {
//                            Text("Processing Steps:")
//                                .font(.headline)
//                                .padding(.bottom, 4)
//                            
//                            ForEach(tokenProcessor.processingSteps, id: \.self) { step in
//                                Text(step)
//                                    .font(.caption)
//                                    .padding(.leading, 8)
//                            }
//                        }
//                        .frame(maxWidth: .infinity, alignment: .leading)
//                        .padding()
//                        .background(Color.gray.opacity(0.1))
//                        .cornerRadius(8)
//                    }
//                    .frame(height: 150)
//                }
            }
            .padding()
            
            // Node editor
            NodeEditor(patch: $patch, selection: $selection)
                .onChange(of: patch) { newPatch in
                    tokenProcessor.patch = newPatch
                }
            
            // Controls
            HStack {
                // Node type selection
                Picker("Node Type:", selection: $selectedNodeType) {
                    ForEach(availableNodeTypes, id: \.self) { type in
                        Text(type).tag(type)
                    }
                }
                .pickerStyle(MenuPickerStyle())
                .frame(width: 150)
                
                Button("Add Node") {
                    addNode(type: selectedNodeType)
                }
                
                Spacer()
                
                // Simple number input for token value
                Stepper(value: Binding(
                    get: {
                        if case let .number(value) = tokenProcessor.inputValue {
                            return value
                        }
                        return 0
                    },
                    set: { tokenProcessor.inputValue = .number($0) }
                ), in: -100...100) {
                    Text("Input: \(tokenValueString(tokenProcessor.inputValue))")
                }
                
                Button("Send Token") {
                    tokenProcessor.processToken()
                }
                .keyboardShortcut(.space, modifiers: [])
            }
            .padding()
        }
    }
    
    // Helper to get clean string representation of token values
    private func tokenValueString(_ value: NodeValue) -> String {
        switch value {
        case .number(let num):
            return String(format: "%.2f", num)
        case .text(let text):
            return "\"\(text)\""
        case .boolean(let bool):
            return bool ? "true" : "false"
        case .array:
            return "[Array]"
        case .dictionary:
            return "{Dictionary}"
        case .none:
            return "—"
        }
    }
    
    // Add a node from the library
    private func addNode(type: String) {
        // Find the selected node for positioning
        var position = CGPoint(x: 300, y: 200)
        
        if let selectedIndex = selection.first {
            // Place new node to the right of the selected node
            position = patch.nodes[selectedIndex].position
            position.x += 200
        }
        
        // Add the node
        if let newNodeIndex = patch.addNode(type: type, at: position) {
            // If there's a selected node, try to connect them
            if let selectedIndex = selection.first {
                // Try to connect from selected to new
                if !patch.nodes[selectedIndex].outputs.isEmpty {
                    patch.connect(from: selectedIndex, to: newNodeIndex)
                }
            }
            
            // Update selection to the new node
            selection = [newNodeIndex]
        }
    }
}

// Create the initial patch with just input and output nodes
private func createInitialPatch() -> Patch {
    var patch = Patch(nodes: [], wires: [])
    
    // Add input and output nodes
    guard let inputIndex = patch.addNode(type: "input", at: CGPoint(x: 100, y: 200)),
          let outputIndex = patch.addNode(type: "output", at: CGPoint(x: 500, y: 200)) else {
        fatalError("Failed to create basic nodes")
    }
    
    // Connect them directly
    patch.connect(from: inputIndex, to: outputIndex)
    
    return patch
}

// Register any custom nodes
private func registerCustomNodes() {
    // Example of adding a custom node
    NodeLibrary.shared.addCustomNode(
        type: "random",
        name: "Random",
        color: .pink,
        inputs: ["min", "max"],
        outputs: ["value"],
        processor: { _ in
            // Generate a random value between 0-100
            return .number(Double.random(in: 0...100))
        }
    )
}


struct TestGameView_Previews: PreviewProvider {
    static var previews: some View {
        Group {
            TestGameView()
                .frame(width: 800, height: 600)
                .previewDisplayName("Large Window")
        }
    }
}
