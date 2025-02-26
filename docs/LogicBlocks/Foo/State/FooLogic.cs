using Chickensoft.LogicBlocks;
using Chickensoft.Introspection;

namespace Game.Foo.State;

/// <summary>
/// Logic block that manages the state of a Foo component.
/// </summary>
[Meta, LogicBlock(typeof(State), Diagram = true)]
public partial class FooLogic : LogicBlock<FooLogic.State>
{
  /// <summary>
  /// Returns the initial state for this logic block.
  /// </summary>
  public override Transition GetInitialState() => To<State.Bar1State>();
}