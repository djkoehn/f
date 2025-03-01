namespace testing.Pipe;

using Godot;
using Chickensoft.LogicBlocks;

public partial class PipeNode : Node2D
{
	private Line2D _line;
	private Area2D _area;
	private CollisionPolygon2D _collision;
	internal PipeLogic _logic;  // Changed from private to internal
	private PipeLogic.IBinding _binding;

	public override void _Ready()
	{
		// Create Line2D
		_line = new Line2D();
		_line.Width = 10;
		_line.DefaultColor = new Color(0.4f, 0.4f, 0.4f);
		AddChild(_line);

		// Create Area2D
		_area = new Area2D();
		_area.CollisionLayer = 2;
		_area.CollisionMask = 4;
		AddChild(_area);

		// Create CollisionPolygon2D
		_collision = new CollisionPolygon2D();
		_area.AddChild(_collision);

		// Setup logic
		_logic = new PipeLogic();
		_binding = _logic.Bind();
		_binding.Handle((in PipeLogic.Output.StateChanged output) =>
		{
			GD.Print($"Pipe state changed to: {output.StateName}");
			UpdateVisuals(output.StateName);
		});

		_logic.Start();
	}

	public void Initialize(Vector2 start, Vector2 end)
	{
		_line.Points = new Vector2[] { start, end };
		UpdateCollisionShape();
	}

	private void UpdateCollisionShape()
	{
		if (_line.Points.Length < 2) return;

		var start = _line.Points[0];
		var end = _line.Points[1];
		var direction = (end - start).Normalized();
		var perpendicular = new Vector2(-direction.Y, direction.X) * _line.Width / 2;

		_collision.Polygon = new Vector2[] {
			start + perpendicular,
			end + perpendicular,
			end - perpendicular,
			start - perpendicular
		};
	}

	private void UpdateVisuals(string state)
	{
		Color color = state switch
		{
			"BlockHovering" => Colors.Yellow,
			"Splitting" => Colors.Orange,
			_ => new Color(0.4f, 0.4f, 0.4f)
		};

		_line.DefaultColor = color;
	}

	public override void _ExitTree()
	{
		if (_binding != null)
		{
			_binding.Dispose();
		}
		base._ExitTree();
	}
}
