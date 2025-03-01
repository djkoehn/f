namespace testing.Pipe;

public partial class PipeLogic {
  public partial record State {
    public sealed partial record Connected : State, IGet<Input.BlockEnter> {
      public Connected() {
        OnAttach(() => Output(new Output.StateChanged("Connected")));
      }

      public Transition On(in Input.BlockEnter input) => To<BlockHovering>();
    }
  }
}