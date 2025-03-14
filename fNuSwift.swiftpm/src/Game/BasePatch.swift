import SwiftUI

func basePatch() -> Patch {
    let input = Node(
        name: "",
        position: CGPoint(x:64, y:236),
        locked: true,
        outputs: ["out"],
        image: Image("input"))
    
    let output = Node(
        name: " ",
        position: CGPoint(x:608, y:236),
        locked: true,
        inputs: ["in"],
        image: Image("output"))
    
    let nodes = [input, output]
    
    let wires = Set([Wire(from: OutputID(0, 0), to: InputID(1, 0))])
    
    let patch = Patch(nodes: nodes, wires: wires)
    
    return patch
}
