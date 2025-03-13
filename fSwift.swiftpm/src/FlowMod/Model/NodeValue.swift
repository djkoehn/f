//
//  NodeValue.swift
//  fSwift
//
//  Created by dj koehn on 3/13/25.
//


// Create a new file: Sources/Flow/Model/NodeValue.swift
import Foundation

/// Represents the values that can flow between nodes
public enum NodeValue: Equatable {
    case number(Double)
    case text(String)
    case boolean(Bool)
    case array([NodeValue])
    case dictionary([String: NodeValue])
    case none
    
    // Helper methods to convert between types
    public var asNumber: Double? {
        if case let .number(value) = self {
            return value
        }
        return nil
    }
    
    public var asText: String? {
        if case let .text(value) = self {
            return value
        }
        return nil
    }
    
    // Add other conversion methods as needed
}
