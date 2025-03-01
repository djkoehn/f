namespace testing.Pipe;

public partial class PipeLogic {
  public partial record State {
    public sealed partial record Splitting : State, IGet<Input.SplitComplete> {
      public Splitting() {
        OnAttach(() => Output(new Output.StateChanged("Splitting")));
      }

      public Transition On(in Input.SplitComplete input) {
        Output(new Output.Disposed());
        return ToSelf(); // Stay in current state
      }
    }
  }
}
