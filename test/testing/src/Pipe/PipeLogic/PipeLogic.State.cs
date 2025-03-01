namespace testing.Pipe;

using Chickensoft.LogicBlocks;

public partial class PipeLogic {
  public abstract partial record State : StateLogic<State>;
}