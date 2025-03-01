namespace testing.Block;

public partial class BlockLogic {
    public static class Output {
        public readonly record struct StateChanged(string StateName);
    }
}
