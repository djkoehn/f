namespace Game.Foo.State;

public partial class FooLogic
{
  /// <summary>
  /// Inputs that can be sent to the Foo logic block.
  /// </summary>
  public static class Input
  {
    /// <summary>
    /// Advances to the next state.
    /// </summary>
    public readonly record struct NextState;

    /// <summary>
    /// Returns to the previous state.
    /// </summary>
    public readonly record struct PreviousState;

    /// <summary>
    /// Processes a value through the current state.
    /// </summary>
    /// <param name="Value">The value to process.</param>
    public readonly record struct ProcessValue(float Value);
  }
}
