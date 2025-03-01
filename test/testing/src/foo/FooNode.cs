namespace testing.foo;

using Chickensoft.LogicBlocks;
using Godot;
using System;

public partial class FooNode : Node
{
	private fooLogic _logic;
	private fooLogic.IBinding _binding;  // Use the nested IBinding interface

	public override void _Ready()
	{
		_logic = new fooLogic();

		// Create binding to handle outputs
		_binding = _logic.Bind();
		_binding.Handle((in fooLogic.Output.StateChanged output) =>
		{
			GD.Print($"State changed to: {output.StateName}");
		});

		_logic.Start();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("toggle_state"))
		{
			_logic.Input(new fooLogic.Input.toggle());
		}
	}

	public override void _ExitTree()
	{
		_binding?.Dispose();
	}
}
