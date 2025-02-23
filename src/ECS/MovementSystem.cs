using Friflo.Engine.ECS;

namespace F.ECS;

public class MovementSystem
{
    private readonly EntityStore _store;
    
    public MovementSystem(EntityStore store)
    {
        _store = store;
    }
    
    public void Update()
    {
        var query = _store.Query<PositionComponent, VelocityComponent>();
        query.ForEachEntity((ref PositionComponent position, ref VelocityComponent velocity, Entity entity) =>
        {
            position.X += velocity.X;
            position.Y += velocity.Y;
        });
    }
} 