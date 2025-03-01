namespace testing.foo;

using Chickensoft.LogicBlocks;

public partial class fooLogic {
    public static class Output {
      public readonly record struct StateChanged(string StateName);
    }
}
  
