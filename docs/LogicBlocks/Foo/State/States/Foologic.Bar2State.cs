using Chickensoft.Introspection;

namespace Game.Foo.State.States;

[Meta]
public record Bar2State : FooLogic.State,
  IGet<FooLogic.Input.NextState>,
  IGet<FooLogic.Input.PreviousState>,
  IGet<FooLogic.Input.ProcessValue>
{
  private readonly IFooData _data;

  public Bar2State(IFooData data)
  {
    _data = data;
    
    this.OnEnter(() => {
      Output(new FooLogic.Output.StateChanged("Bar2"));
    });
  }

  public Transition On(in FooLogic.Input.NextState input) =>
    To<Bar3State>(_data);

  public Transition On(in FooLogic.Input.PreviousState input) =>
    To<Bar1State>(_data);

  public Transition On(in FooLogic.Input.ProcessValue input)
  {
    var processedValue = input.Value + 10;
    Output(new FooLogic.Output.ValueProcessed(_data.Id, processedValue));
    return Stay(new FooData(_data.Id, processedValue));
  }
}
