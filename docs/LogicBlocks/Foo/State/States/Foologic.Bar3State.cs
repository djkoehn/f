using Chickensoft.Introspection;

namespace Game.Foo.State.States;

[Meta]
public record Bar3State : FooLogic.State,
  IGet<FooLogic.Input.PreviousState>,
  IGet<FooLogic.Input.ProcessValue>
{
  private readonly IFooData _data;

  public Bar3State(IFooData data)
  {
    _data = data;
    
    this.OnEnter(() => {
      Output(new FooLogic.Output.StateChanged("Bar3"));
    });
  }

  public Transition On(in FooLogic.Input.PreviousState input) =>
    To<Bar2State>(_data);

  public Transition On(in FooLogic.Input.ProcessValue input)
  {
    var processedValue = input.Value / 2;
    Output(new FooLogic.Output.ValueProcessed(_data.Id, processedValue));
    return Stay(new FooData(_data.Id, processedValue));
  }
}
