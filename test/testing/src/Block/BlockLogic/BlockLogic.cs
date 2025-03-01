namespace testing.Block;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class BlockLogic : LogicBlock<BlockLogic.State> {
  public BlockLogic() {
    // Pre-allocate states in the blackboard
    Set(new State.Placed());
    Set(new State.Dragging());
    Set(new State.InToolbar());
  }

  public override Transition GetInitialState() => To<State.Placed>();
}
