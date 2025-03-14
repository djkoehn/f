import SwiftUI

// Extension to add processing functionality to Node
extension Node {
    // Dictionary to store processing functions for nodes by name
    private static var processingFunctions: [String: (NodeValue) -> NodeValue] = [:]
    
    // Set the processing function for this node
    mutating func setProcessor(_ processor: @escaping (NodeValue) -> NodeValue) {
        // Use the node name as the key
        Node.processingFunctions[name] = processor
        print("Registered processor for node: \(name)")
    }
    
    // Get the processing function for this node
    func processor() -> ((NodeValue) -> NodeValue)? {
        return Node.processingFunctions[name]
    }
    
    // Process a value through this node
    //    func process(_ value: NodeValue) -> NodeValue {
    //        if let processor = processor() {
    //            let result = processor(value)
    //            print("Node '\(name)' processed \(value) to \(result)")
    //            return result
    //        }
    //        print("Node '\(name)' has no processor, passing \(value) through unchanged")
    //        return value // If no processor, pass through unchanged
    //    }
    //}
    
    // Simple token processor class
    class TokenProcessor: ObservableObject {
        var patch: Patch
        @Published var inputValue: NodeValue = .number(10)
        @Published var outputValue: NodeValue = .none
        @Published var processingSteps: [String] = []
        
        init(patch: Patch) {
            self.patch = patch
        }
        
        func processToken() {
            // Reset state
            processingSteps = []
            
            // Start from the input node (index 0)
            let result = processNodeAndFollow(nodeIndex: 0, value: inputValue)
            
            // Update the output value
            outputValue = result
            
            // Print the processing steps for debugging
            print("Processing steps:")
            for step in processingSteps {
                print(step)
            }
        }
        
        // Process a value through a node and follow all connections
        private func processNodeAndFollow(nodeIndex: Int, value: NodeValue) -> NodeValue {
            // Get the current node
            let node = patch.nodes[nodeIndex]
            
            // Log the step
            processingSteps.append("Processing node '\(node.name)' with value \(value)")
            
            // Process the value through this node
            let processedValue = node.process(value)
            
            // Find outgoing wires from this node
            // In the Wire model: wire.output is the source, wire.input is the destination
            let outgoingWires = patch.wires.filter { wire in
                wire.output.nodeIndex == nodeIndex
            }
            
            processingSteps.append("Node '\(node.name)' produced value \(processedValue)")
            processingSteps.append("Found \(outgoingWires.count) outgoing wires from node '\(node.name)'")
            
            // If no outgoing wires, this is the end of the line
            if outgoingWires.isEmpty {
                return processedValue
            }
            
            // Process each outgoing wire and return the final result
            // (In a real graph, you'd need to handle multiple paths, but for now we just follow all of them)
            var finalResult = processedValue
            
            for wire in outgoingWires {
                let targetNodeIndex = wire.input.nodeIndex
                processingSteps.append("Following wire to node \(targetNodeIndex)")
                
                // Process the next node with the value from this node
                finalResult = processNodeAndFollow(nodeIndex: targetNodeIndex, value: processedValue)
            }
            
            return finalResult
        }
    }
    
    // Create a node with a processing function
    func createNode(name: String, position: CGPoint, titleBarColor: Color = .blue, processor: @escaping (NodeValue) -> NodeValue) -> Node {
        var node = Node(
            name: name,
            position: position,
            titleBarColor: titleBarColor,
            inputs: ["in"],
            outputs: ["out"]
        )
        node.setProcessor(processor)
        return node
    }
}
