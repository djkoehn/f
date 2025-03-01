namespace testing.Block;

using Chickensoft.LogicBlocks;

public partial class BlockLogic {
  public abstract partial record State : StateLogic<State>;
}
