using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using Chickensoft.LogicBlocks;
using F.Framework.Logging;
using Godot;
using System;

namespace F.Framework.Blocks;

[Meta, LogicBlock(typeof(State))]
public partial class BlockLogicMachine : Node
{
    private LogicBlockImpl? _logicBlock;
    public event Action<State>? StateChanged;

    public State Value => _logicBlock?.Value ?? State.InToolbar;

    public override void _Ready()
    {
        base._Ready();
        _logicBlock = new LogicBlockImpl();
        _logicBlock.StateChanged += OnStateChanged;
        Logger.Block.Print($"BlockLogicMachine {Name} ready");
    }

    public void Send<TInput>(TInput input) where TInput : struct
    {
        if (_logicBlock == null)
        {
            Logger.Block.Err($"BlockLogicMachine {Name} has no logic block");
            return;
        }

        _logicBlock.Send(input);
    }

    private void OnStateChanged(State state)
    {
        Logger.Block.Print($"BlockLogicMachine {Name} state changed to {state}");
        StateChanged?.Invoke(state);
    }

    public override void _ExitTree()
    {
        if (_logicBlock != null)
        {
            _logicBlock.StateChanged -= OnStateChanged;
            _logicBlock = null;
        }
    }
}

[Meta, LogicBlock(typeof(State))]
public partial class LogicBlockImpl : LogicBlock<State>
{
    public LogicBlockImpl() : base(State.InToolbar)
    {
        Logger.Block.Print("BlockLogicMachine initialized with state transitions");
    }

    public override Chickensoft.LogicBlocks.Transition GetInitialState() => To<State.InToolbarState>();
}

public abstract record State : StateLogic<State>
{
    public record InToolbarState : State
    {
        public Chickensoft.LogicBlocks.Transition On(in Input.Interact _) => To<DraggingState>();
    }

    public record DraggingState : State
    {
        public Chickensoft.LogicBlocks.Transition On(in Input.Interact _) => To<PlacedState>();
        public Chickensoft.LogicBlocks.Transition On(in Input.HoveredOverPipe _) => To<ConnectedAndDraggingState>();
    }

    public record PlacedState : State
    {
        public Chickensoft.LogicBlocks.Transition On(in Input.Interact _) => To<DraggingState>();
        public Chickensoft.LogicBlocks.Transition On(in Input.HoveredOverPipe _) => To<ConnectedState>();
    }

    public record ConnectedState : State
    {
        public Chickensoft.LogicBlocks.Transition On(in Input.Interact _) => To<ConnectedAndDraggingState>();
        public Chickensoft.LogicBlocks.Transition On(in Input.ReturnBlock _) => To<InToolbarState>();
    }

    public record ConnectedAndDraggingState : State
    {
        public Chickensoft.LogicBlocks.Transition On(in Input.Interact _) => To<ConnectedState>();
        public Chickensoft.LogicBlocks.Transition On(in Input.ReturnBlock _) => To<InToolbarState>();
    }

    public static readonly State InToolbar = new InToolbarState();
}

public static class Input
{
    public readonly record struct Interact;
    public readonly record struct ReturnBlock;
    public readonly record struct HoveredOverPipe;
}