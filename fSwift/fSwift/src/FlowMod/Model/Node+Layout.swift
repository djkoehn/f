// Copyright AudioKit. All Rights Reserved. Revision History at http://github.com/AudioKit/Flow/

import CoreGraphics
import Foundation

public extension Node {
    /// Calculates the bounding rectangle for a node.
    func rect(layout: LayoutConstants) -> CGRect {
        let size = CGSize(width: layout.nodeWidth,
                          height: layout.nodeHeight)

        return CGRect(origin: position, size: size)
    }

    /// Calculates the bounding rectangle for an input port (not including the name).
    func inputRect(input: PortIndex, layout: LayoutConstants) -> CGRect {
        let totalInputs = CGFloat(inputs.count)
        let spacing = layout.nodeHeight / (totalInputs + 1)
        let y = spacing * CGFloat(input + 1)
        
        return CGRect(origin: position + CGSize(width: layout.portSpacing, height: y - layout.portSize.height/2),
                      size: layout.portSize)
    }

    /// Calculates the bounding rectangle for an output port (not including the name).
    func outputRect(output: PortIndex, layout: LayoutConstants) -> CGRect {
        let totalOutputs = CGFloat(outputs.count)
        let spacing = layout.nodeHeight / (totalOutputs + 1)
        let y = spacing * CGFloat(output + 1)
        
        return CGRect(origin: position + CGSize(width: layout.nodeWidth - layout.portSpacing - layout.portSize.width, 
                                                height: y - layout.portSize.height/2),
                      size: layout.portSize)
    }
}
