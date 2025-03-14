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
            .padding(25)
            .cornerRadius(10)
            .overlay(
                RoundedRectangle(cornerRadius: 5)
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
        }.overlay(
            RoundedRectangle(cornerRadius: 10)
                .stroke(.white, lineWidth: 20)
            )
    }
}

struct ContentView_Previews: PreviewProvider {
    static var previews: some View {
        Group {
            ContentView()
                
                .frame(width: 800, height: 600)
                .previewDisplayName("Large Window")
        }
    }
}
