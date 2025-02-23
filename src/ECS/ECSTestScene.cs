using Godot;
using F.ECS;
using Friflo.Engine.ECS;

public partial class ECSTestScene : Node2D
{
    private ECSWorld _ecsWorld = null!;
    private Entity _testEntity;
    private ColorRect _square = null!;

    public override void _Ready()
    {
        _ecsWorld = new ECSWorld(this);
        
        // Create a simple square
        _square = new ColorRect
        {
            Position = new Vector2(100, 100),
            Size = new Vector2(50, 50),
            Color = Colors.Blue
        };
        AddChild(_square);

        // Create an entity at the square's position
        _testEntity = _ecsWorld.CreateMovingEntity(
            _square.Position.X,
            _square.Position.Y,
            0, 0  // Initial velocity is 0
        );
    }

    public override void _Process(double delta)
    {
        // Update ECS world
        _ecsWorld.Update((float)delta);

        // When space is pressed, give the entity some velocity
        if (Input.IsActionJustPressed("ui_accept"))  // Space bar
        {
            _ecsWorld.SetEntityVelocity(_testEntity, 5, 0);  // Move right
        }

        // Update square position based on entity position
        var (x, y) = _ecsWorld.GetEntityPosition(_testEntity);
        _square.Position = new Vector2(x, y);
    }
} 