namespace testing.foo;

using Chickensoft.LogicBlocks;

public partial class fooLogic {
  public partial record State {
    public sealed partial record bar : State, IGet<Input.toggle> {
      public bar() {
        OnAttach(() => Output(new Output.StateChanged("bar")));
      }

      public Transition On(in Input.toggle input) => To<foo>();
    }
  }
}
