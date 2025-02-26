namespace Game.Foo.State;

public partial class FooLogic
{
  /// <summary>
  /// Outputs that can be emitted by the Foo logic block.
  /// </summary>
  public static class Output
  {
    /// <summary>
    /// Emitted when the state changes.
    /// </summary>
    /// <param name="StateName">Name of the new state.</param>
    public readonly record struct StateChanged(string StateName);

    /// <summary>
    /// Emitted when a value has been processed.
    /// </summary>
    /// <param name="Id">ID of the Foo instance.</param>
    /// <param name="Value">The processed value.</param>
    public readonly record struct ValueProcessed(string Id, float Value);
  }
}
