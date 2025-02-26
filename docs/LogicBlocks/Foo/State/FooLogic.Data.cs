using Chickensoft.Introspection;

namespace Game.Foo.State;

/// <summary>
/// Interface for accessing Foo state data.
/// </summary>
[Meta]
public interface IFooData
{
  /// <summary>
  /// Unique identifier for this Foo instance.
  /// </summary>
  string Id { get; }

  /// <summary>
  /// The last processed value.
  /// </summary>
  float Value { get; }
}

/// <summary>
/// Implementation of Foo state data.
/// </summary>
[Meta]
public record FooData : IFooData
{
  public string Id { get; }
  public float Value { get; }

  /// <summary>
  /// Creates a new Foo data instance.
  /// </summary>
  /// <param name="id">Unique identifier for this Foo instance.</param>
  /// <param name="value">Initial value.</param>
  public FooData(string id, float value = 0)
  {
    Id = id;
    Value = value;
  }
}
