namespace testing.foo;

using Chickensoft.LogicBlocks;

public partial class fooLogic {
  public abstract partial record State : StateLogic<State>;
}
