using Godot;
using Chickensoft.AutoInject;
using Chickensoft.Serialization;
using Chickensoft.Serialization.Godot;
using Game.Foo.State;

namespace Game.Foo;

/// <summary>
/// Main Foo node that handles state management and provides itself as a service.
/// </summary>
[Singleton]
public partial class Foo : Node, IProvide<Foo>
{
  // The state machine that manages this Foo's behavior.
  private readonly FooLogic _logic;

  // Data that represents this Foo's current state.
  private readonly IFooData _data;

  // Add serializer instance
  private readonly GodotSerializer _serializer;

  /// <summary>
  /// Creates a new Foo instance and sets up its state machine.
  /// </summary>
  public Foo()
  {
    // Create initial data with a unique identifier
    _data = new FooData(Guid.NewGuid().ToString());
    _logic = new FooLogic();
    _serializer = new GodotSerializer();

    using var binding = _logic.Bind();

    // Log state changes for debugging
    binding.WhenStateChanged((previous, current) =>
      GD.Print($"State changed from {previous?.GetType().Name} to {current.GetType().Name}"));

    // Handle value processing outputs
    binding.Handle((in FooLogic.Output.ValueProcessed output) =>
      GD.Print($"Value processed: {output.Value} for ID: {output.Id}"));
  }

  /// <summary>
  /// Called when the node enters the scene tree.
  /// </summary>
  public override void _Ready()
  {
    // Register this instance as a provider
    this.Provide();

    // Start the state machine in its initial state
    _logic.Start<FooLogic.State.Bar1State>(_data);
  }

  /// <summary>
  /// Implementation of IProvide<Foo> that returns this instance.
  /// </summary>
  Foo IProvide<Foo>.Value() => this;

  // Add methods for serialization

  /// <summary>
  /// Serializes the current state of the logic block.
  /// </summary>
  public string SerializeState()
  {
    return _serializer.Serialize(_logic);
  }

  /// <summary>
  /// Deserializes and applies a previously saved state.
  /// </summary>
  public void DeserializeState(string serializedState)
  {
    _serializer.Deserialize(serializedState, _logic);
  }
}
