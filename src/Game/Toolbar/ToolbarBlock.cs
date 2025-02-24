using Chickensoft.AutoInject;
using Chickensoft.Introspection;
using F.Framework.Blocks;
using F.Framework.Logging;

namespace F.Game.Toolbar;

[Meta(typeof(IAutoNode))]
public partial class ToolbarBlock : BaseBlock, IProvide<Node>, IDependent
{
    [Dependency] public ToolbarBlockContainer Container => this.DependOn<ToolbarBlockContainer>();

    Node IProvide<Node>.Value()
    {
        return this;
    }

    public override void _Ready()
    {
        base._Ready();
        this.Provide();
    }

    public override void SetInToolbar(bool value)
    {
        Logger.UI.Print($"Block {Name} SetInToolbar: {value}");
        base.SetInToolbar(value);
        if (value)
        {
            Container.UpdateBlockPositions();
            Container.UpdateContainerSize();
        }
    }

    public override void _Notification(int what)
    {
        this.Notify(what);
    }
}