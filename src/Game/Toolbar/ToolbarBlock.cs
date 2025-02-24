using Godot;
using F.Framework.Blocks;
using Chickensoft.AutoInject;
using Chickensoft.Introspection;

namespace F.Game.Toolbar;

[Meta(typeof(IAutoNode))]
public partial class ToolbarBlock : BaseBlock, IProvide<Node>, IDependent
{
    [Dependency]
    public ToolbarBlockContainer Container => this.DependOn<ToolbarBlockContainer>();

    public override void _Ready()
    {
        base._Ready();
        this.Provide();
    }

    public override void SetInToolbar(bool value)
    {
        base.SetInToolbar(value);
        if (value)
        {
            Container.UpdateBlockPositions();
            Container.UpdateContainerSize();
        }
        GD.Print($"[ToolbarBlock] Block {Name} SetInToolbar: {value}");
    }

    public override void _Notification(int what) => this.Notify(what);

    Node IProvide<Node>.Value() => this;
}