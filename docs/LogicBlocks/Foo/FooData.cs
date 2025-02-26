using Chickensoft.Introspection;

namespace Game.Foo;

/// <summary>
/// Interface for accessing Foo component data.
/// </summary>
[Meta]
public interface IFooComponent
{
  /// <summary>
  /// Unique identifier for this Foo component.
  /// </summary>
  string Id { get; }

  /// <summary>
  /// The current value stored in this component.
  /// </summary>
  float CurrentValue { get; }
}

/// <summary>
/// Implementation of a Foo component's data.
/// </summary>
[Meta]
public record FooComponent : IFooComponent
{
  public string Id { get; }
  public float CurrentValue { get; }

  /// <summary>
  /// Creates a new Foo component with the specified ID and initial value.
  /// </summary>
  /// <param name="id">Unique identifier for this component.</param>
  /// <param name="currentValue">Initial value for this component.</param>
  public FooComponent(string id, float currentValue = 0)
  {
    Id = id;
    CurrentValue = currentValue;
  }
}