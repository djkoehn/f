namespace testing.Block;

public partial class BlockLogic {
  public partial record State {
    public sealed partial record InToolbar : State, IGet<Input.Click> {
      public InToolbar() {
        OnAttach(() => Output(new Output.StateChanged("InToolbar")));
      }

      public Transition On(in Input.Click input) => To<Dragging>();
    }
  }
}