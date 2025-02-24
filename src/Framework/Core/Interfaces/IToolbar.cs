using Chickensoft.GodotNodeInterfaces;
using F.Framework.Blocks;

namespace F.Framework.Core.Interfaces;

public interface IToolbar : IControl
{
    IToolbarVisuals ToolbarVisuals { get; }
    IToolbarBlockContainer BlockContainer { get; }
    void ReturnBlockToToolbar(BaseBlock block);
}

public interface IToolbarVisuals : IControl
{
}

public interface IToolbarBlockContainer : IContainer
{
}