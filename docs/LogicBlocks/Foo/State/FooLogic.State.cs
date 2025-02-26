namespace Game.Foo.State;

public partial class FooLogic
{
  public abstract record State : StateLogic<State>;
}
