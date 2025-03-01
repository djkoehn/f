namespace testing.foo;

using Chickensoft.LogicBlocks;

public partial class fooLogic {
  public partial record State {
    public sealed partial record foo : State, IGet<Input.toggle> {
      public foo() {
        OnAttach(() => Output(new Output.StateChanged("foo")));
      }

      public Transition On(in Input.toggle input) => To<bar>();
    }
  }
}
