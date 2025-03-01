namespace testing.Pipe;

using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using Godot;

[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class PipeLogic : LogicBlock<PipeLogic.State> {
    public PipeLogic() {
        // Pre-allocate states
        Set(new State.Connected());
        Set(new State.BlockHovering());
        Set(new State.Splitting());
    }

    public override Transition GetInitialState() => To<State.Connected>();

    public static class Input {
        public readonly record struct BlockEnter;
        public readonly record struct BlockExit;
        public readonly record struct Click(Vector2 Position);
        public readonly record struct SplitComplete;
    }

    public static class Output {
        public readonly record struct StateChanged(string StateName);
        public readonly record struct SplitRequested(Vector2 SplitPosition);
        public readonly record struct Disposed;
    }
}
