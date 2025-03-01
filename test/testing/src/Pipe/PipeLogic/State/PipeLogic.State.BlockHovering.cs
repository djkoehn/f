namespace testing.Pipe;

public partial class PipeLogic {
  public partial record State {
    public sealed partial record BlockHovering : State, 
      IGet<Input.BlockExit>,
      IGet<Input.Click> {
      
      public BlockHovering() {
        OnAttach(() => Output(new Output.StateChanged("BlockHovering")));
      }

      public Transition On(in Input.BlockExit input) => To<Connected>();
      
      public Transition On(in Input.Click input) {
        Output(new Output.SplitRequested(input.Position));
        return To<Splitting>();
      }
    }
  }
}