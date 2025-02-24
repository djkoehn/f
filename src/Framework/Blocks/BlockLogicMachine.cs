using Chickensoft.LogicBlocks;
using Chickensoft.Introspection;
using Chickensoft.AutoInject;
using Godot;

namespace F.Framework.Blocks;

public static class BlockLogicInput
{
    public readonly record struct Interact;
    public readonly record struct ReturnBlock;
    public readonly record struct HoveredOverPipe;
}

[Meta(typeof(IDependent))]
public partial class BlockLogicMachine : Node, IDependent
{
    private LogicBlockImpl? _logicBlock;

    [Dependency]
    public new Node Owner => this.DependOn<Node>();

    public event System.Action<LogicBlockImpl.BlockLogicState>? StateChanged;

    public override void _Ready()
    {
        _logicBlock = new LogicBlockImpl(this);
        _logicBlock.Start();
        GD.Print("[BlockLogicMachine Debug] State machine initialized");
    }

    public void Send<TInput>(TInput input) where TInput : struct
    {
        if (_logicBlock == null) return;
        var oldState = _logicBlock.Value;
        var newState = _logicBlock.Input(input);
        if (!ReferenceEquals(oldState, newState))
        {
            GD.Print($"[BlockLogicMachine Debug] State transition: {oldState.GetType().Name} -> {newState.GetType().Name}");
            StateChanged?.Invoke(newState);
        }
    }

    public override void _Notification(int what) => this.Notify(what);

    [LogicBlock(typeof(BlockLogicState))]
    public class LogicBlockImpl : LogicBlock<LogicBlockImpl.BlockLogicState>
    {
        private readonly BlockLogicMachine _machine;

        public LogicBlockImpl(BlockLogicMachine machine)
        {
            _machine = machine;
            // Initialize all possible states
            Set(new BlockLogicState.InToolbarState());
            Set(new BlockLogicState.DraggingState());
            Set(new BlockLogicState.PlacedState());
            Set(new BlockLogicState.ConnectedState());
            Set(new BlockLogicState.ConnectedAndDraggingState());
        }

        public override Transition GetInitialState() => To<BlockLogicState.InToolbarState>();

        public abstract record BlockLogicState : StateLogic<BlockLogicState>
        {
            public record InToolbarState : BlockLogicState, IGet<BlockLogicInput.Interact>
            {
                public InToolbarState() => OnAttach(() => GD.Print("[BlockLogicMachine Debug] Entered InToolbarState"));
                public Transition On(in BlockLogicInput.Interact _)
                {
                    GD.Print("[BlockLogicMachine Debug] InToolbarState received Interact input, transitioning to DraggingState");
                    return To<DraggingState>();
                }
            }

            public record DraggingState : BlockLogicState,
              IGet<BlockLogicInput.Interact>,
              IGet<BlockLogicInput.ReturnBlock>,
              IGet<BlockLogicInput.HoveredOverPipe>
            {
                public DraggingState() => OnAttach(() => GD.Print("[BlockLogicMachine Debug] Entered DraggingState"));
                public Transition On(in BlockLogicInput.Interact _) => To<PlacedState>();
                public Transition On(in BlockLogicInput.ReturnBlock _) => To<InToolbarState>();
                public Transition On(in BlockLogicInput.HoveredOverPipe _) => To<ConnectedState>();
            }

            public record PlacedState : BlockLogicState,
              IGet<BlockLogicInput.Interact>,
              IGet<BlockLogicInput.ReturnBlock>
            {
                public PlacedState() => OnAttach(() => GD.Print("[BlockLogicMachine Debug] Entered PlacedState"));
                public Transition On(in BlockLogicInput.Interact _) => To<DraggingState>();
                public Transition On(in BlockLogicInput.ReturnBlock _) => To<InToolbarState>();
            }

            public record ConnectedState : BlockLogicState,
              IGet<BlockLogicInput.Interact>,
              IGet<BlockLogicInput.ReturnBlock>
            {
                public ConnectedState() => OnAttach(() => GD.Print("[BlockLogicMachine Debug] Entered ConnectedState"));
                public Transition On(in BlockLogicInput.Interact _) => To<ConnectedAndDraggingState>();
                public Transition On(in BlockLogicInput.ReturnBlock _) => To<InToolbarState>();
            }

            public record ConnectedAndDraggingState : BlockLogicState,
              IGet<BlockLogicInput.Interact>,
              IGet<BlockLogicInput.ReturnBlock>
            {
                public ConnectedAndDraggingState() => OnAttach(() => GD.Print("[BlockLogicMachine Debug] Entered ConnectedAndDraggingState"));
                public Transition On(in BlockLogicInput.Interact _) => To<ConnectedState>();
                public Transition On(in BlockLogicInput.ReturnBlock _) => To<InToolbarState>();
            }
        }
    }
}