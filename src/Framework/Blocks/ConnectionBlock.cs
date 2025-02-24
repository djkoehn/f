using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using F.Framework.Connections;
using F.Framework.Logging;
using Godot;

namespace F.Framework.Blocks;

[Meta(typeof(IAutoNode))]
public partial class ConnectionBlock : BaseBlock, IProvide<Node>, IDependent
{
    [Dependency] public ConnectionManager ConnectionManager => this.DependOn<ConnectionManager>();

    private bool _isInputConnected;
    private bool _isOutputConnected;

    public bool IsInputConnected
    {
        get => _isInputConnected;
        set
        {
            var oldValue = _isInputConnected;
            _isInputConnected = value;
            if (oldValue != value)
            {
                Logger.Block.Print($"Block {Name} input connection state changed: {oldValue} -> {value}");
                HandleStateChanged();
            }
        }
    }

    public bool IsOutputConnected
    {
        get => _isOutputConnected;
        set
        {
            var oldValue = _isOutputConnected;
            _isOutputConnected = value;
            if (oldValue != value)
            {
                Logger.Block.Print($"Block {Name} output connection state changed: {oldValue} -> {value}");
                HandleStateChanged();
            }
        }
    }

    Node IProvide<Node>.Value()
    {
        return this;
    }

    public override void _Ready()
    {
        base._Ready();
        this.Provide();
    }

    private void HandleStateChanged()
    {
        var oldState = State;
        var newState = DetermineState();
        if (oldState != newState)
        {
            Logger.Block.Print($"Block {Name} state changed: {oldState} -> {newState} (CompleteConnection)");
            CompleteConnection();
        }
    }

    private BlockState DetermineState()
    {
        if (IsInputConnected && IsOutputConnected)
        {
            return BlockState.Connected;
        }
        else if (IsInputConnected || IsOutputConnected)
        {
            return BlockState.Placed;
        }
        else
        {
            return BlockState.InToolbar;
        }
    }

    public override void CompleteConnection()
    {
        var oldState = State;
        State = BlockState.Connected;
        Logger.Block.Print($"Block {Name} state changed: {oldState} -> {State} (CompleteConnection)");
    }

    public override void _Notification(int what)
    {
        this.Notify(what);
    }
}