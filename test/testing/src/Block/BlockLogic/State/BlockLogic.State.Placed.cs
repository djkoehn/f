namespace testing.Block;

public partial class BlockLogic {
  public partial record State {
    public sealed partial record Placed : State, IGet<Input.Click>, IGet<Input.RightClick> {
      public Placed() {
        OnAttach(() => Output(new Output.StateChanged("Placed")));
      }

      public Transition On(in Input.Click input) => To<Dragging>();
      public Transition On(in Input.RightClick input) => To<InToolbar>();
    }
  }
}
