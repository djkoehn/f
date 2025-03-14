// Copyright AudioKit. All Rights Reserved. Revision History at http://github.com/AudioKit/Flow/

import CoreGraphics
import SwiftUI

/// Nodes are identified by index in `Patch/nodes``.
public typealias NodeIndex = Int

/// Function type for node value processing
public typealias NodeProcessor = (NodeValue) -> NodeValue

/// Nodes are identified by index in ``Patch/nodes``.
///
/// Using indices as IDs has proven to be easy and fast for our use cases. The ``Patch`` should be
/// generated from your own data model, not used as your data model, so there isn't a requirement that
/// the indices be consistent across your editing operations (such as deleting nodes).
public struct Node: Equatable {
    public var name: String
    public var position: CGPoint
    public var titleBarColor: Color
    public var locked = false
    public var inputs: [Port]
    public var outputs: [Port]
    public var image: Image?
    
    // Store the processor as a closure
    private var processorFunction: NodeProcessor?
    
    /// Process a value through this node
    public func process(_ value: NodeValue) -> NodeValue {
        if let processor = processorFunction {
            let result = processor(value)
            print("Node '\(name)' processed \(value) to \(result)")
            return result
        }
        print("Node '\(name)' has no processor, passing \(value) through unchanged")
        return value // If no processor, pass through unchanged
    }

    @_disfavoredOverload
    public init(name: String,
                position: CGPoint = .zero,
                titleBarColor: Color = Color.clear,
                locked: Bool = false,
                inputs: [Port] = [],
                outputs: [Port] = [],
                image: Image? = nil,
                processor: NodeProcessor? = nil)
    {
        self.name = name
        self.position = position
        self.titleBarColor = titleBarColor
        self.locked = locked
        self.inputs = inputs
        self.outputs = outputs
        self.image = image
        self.processorFunction = processor
    }
    
    public init(name: String,
                position: CGPoint = .zero,
                titleBarColor: Color = Color.clear,
                locked: Bool = false,
                inputs: [String] = [],
                outputs: [String] = [],
                image: Image? = nil,
                processor: NodeProcessor? = nil)
    {
        self.name = name
        self.position = position
        self.titleBarColor = titleBarColor
        self.locked = locked
        self.inputs = inputs.map { Port(name: $0) }
        self.outputs = outputs.map { Port(name: $0) }
        self.image = image
        self.processorFunction = processor
    }
    
    // For Equatable conformance, ignore the processor
    public static func ==(lhs: Node, rhs: Node) -> Bool {
        return lhs.name == rhs.name &&
               lhs.position == rhs.position &&
               lhs.titleBarColor == rhs.titleBarColor &&
               lhs.locked == rhs.locked &&
               lhs.inputs == rhs.inputs &&
               lhs.outputs == rhs.outputs
        // Note: we intentionally don't compare image or processor
    }
}
