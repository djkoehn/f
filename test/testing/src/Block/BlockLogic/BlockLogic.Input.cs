namespace testing.Block;

using Chickensoft.LogicBlocks;

public partial class BlockLogic {
    public static class Input {
        public readonly record struct Click;
        public readonly record struct RightClick;
    }
}
