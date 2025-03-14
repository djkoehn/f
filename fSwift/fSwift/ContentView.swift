import SwiftUI
import SpriteKit

struct ContentView: View {
    @State var patch = basePatch()
    @State var selection = Set<NodeIndex>()

    func addNode() {
        let newNode = Node(name: "yorbo", titleBarColor: Color.red, inputs: ["in"], outputs: ["out"])
        patch.nodes.append(newNode)
    }

    var body: some View {
        ZStack(alignment: .topTrailing) {
            NodeEditor(patch: $patch, selection: $selection)
            Button("add node") {
                addNode()
            }
            .padding()
            .cornerRadius(10)
            .overlay(
                RoundedRectangle(cornerRadius: 20)
                    .fill(.blue)
                    .stroke(
                        .white,
                        lineWidth: 10)
            )
            .overlay(
                Text("add node")
                    .font(Font.custom("GambadoSansForte", size: 20))
            ).onTapGesture(perform: {
                addNode()
            })
        }
    }
}
