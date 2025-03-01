namespace testing.Block;

using Chickensoft.LogicBlocks;
using Godot;

public partial class BlockNode : Control
{
	private BlockLogic _logic;
	private BlockLogic.IBinding _binding;
	private bool _isDragging;

	public override void _Ready()
	{
		_logic = new BlockLogic();

		_binding = _logic.Bind();
		_binding.Handle((in BlockLogic.Output.StateChanged output) =>
		{
			_isDragging = output.StateName == "Dragging";

			if (output.StateName == "InToolbar")
			{
				var toolbar = GetNode<ToolbarNode>("/root/BlockGameScene/Toolbar"); // Adjust path as needed
				Position = toolbar.GetCenterPosition(Size);
			}

			GD.Print($"Block state changed to: {output.StateName}");
			GD.Print($"_isDragging is now: {_isDragging}");
		});

		_logic.Start();
		GD.Print($"Initial state: {_logic.Value}");
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventMouseButton mouseEvent || !IsMouseOverBlock())
			return;

		if (mouseEvent.Pressed)
			HandleMousePress(mouseEvent.ButtonIndex);
	}

	private bool IsMouseOverBlock() =>
		GetGlobalRect().HasPoint(GetViewport().GetMousePosition());

	private void HandleMousePress(MouseButton button)
	{
		switch (button)
		{
			case MouseButton.Left:
				_logic.Input(new BlockLogic.Input.Click());
				break;
			case MouseButton.Right:
				_logic.Input(new BlockLogic.Input.RightClick());
				break;
		}
	}

	public override void _Process(double delta)
	{
		if (_isDragging)
			UpdateDraggingPosition();
	}

	private void UpdateDraggingPosition() =>
		Position = GetViewport().GetMousePosition() - Size / 2;

	public override void _ExitTree() =>
		_binding?.Dispose();
}
