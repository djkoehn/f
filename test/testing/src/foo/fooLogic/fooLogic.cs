namespace testing.foo;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;

[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class fooLogic : LogicBlock<fooLogic.State> {
  public fooLogic() {
    // Pre-allocate states in the blackboard
    Set(new State.foo());
    Set(new State.bar());
  }

  public override Transition GetInitialState() => To<State.foo>();

}
