using Godot;
using F.Framework.Connections;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;

namespace F.Framework.Blocks;

[Meta(typeof(IAutoNode))]
public partial class ConnectionBlock : BaseBlock, IProvide<Node>, IDependent
{
    [Dependency]
    public ConnectionManager ConnectionManager => this.DependOn<ConnectionManager>();

    public override void _Ready()
    {
        base._Ready();
        this.Provide();
    }

    public override void CompleteConnection()
    {
        var oldState = State;
        State = BlockState.Connected;
        GD.Print($"[ConnectionBlock Debug] Block {Name} state changed: {oldState} -> {State} (CompleteConnection)");
    }

    public override void _Notification(int what) => this.Notify(what);

    Node IProvide<Node>.Value() => this;
}