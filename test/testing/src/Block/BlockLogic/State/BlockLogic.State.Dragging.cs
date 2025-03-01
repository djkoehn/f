namespace testing.Block;

public partial class BlockLogic {
  public partial record State {
    public sealed partial record Dragging : State, IGet<Input.Click>, IGet<Input.RightClick> {
      public Dragging() {
        OnAttach(() => Output(new Output.StateChanged("Dragging")));
      }

      public Transition On(in Input.Click input) => To<Placed>();
      public Transition On(in Input.RightClick input) => To<InToolbar>();
    }
  }
}
