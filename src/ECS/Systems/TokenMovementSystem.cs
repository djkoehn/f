using Friflo.Engine.ECS;
using F.ECS.Components;
using Godot;

namespace F.ECS.Systems;

public class TokenMovementSystem
{
    private readonly EntityStore _store;
    
    public TokenMovementSystem(EntityStore store)
    {
        _store = store;
    }
    
    public void Update(float delta)
    {
        var query = _store.Query<TokenMovementComponent, TokenVisualComponent>();
        
        query.ForEachEntity((ref TokenMovementComponent movement, ref TokenVisualComponent visual, Entity entity) =>
        {
            if (!movement.IsMoving) return;
            
            // Simple linear movement for now
            var direction = (movement.TargetPosition - movement.Position).Normalized();
            var speed = 200.0f; // We'll move this to config later
            movement.Position += direction * speed * delta;
            
            // Update visual token position
            visual.VisualToken.GlobalPosition = movement.Position;
            
            // Check if we've reached the target
            if (movement.Position.DistanceTo(movement.TargetPosition) < 1.0f)
            {
                movement.Position = movement.TargetPosition;
                movement.IsMoving = false;
                
                // Update pipe position if we have one
                if (movement.CurrentPipe is { } pipe)
                {
                    pipe.EndTokenMovement(visual.VisualToken);
                    movement.CurrentPipe = null;
                }
            }
            else if (movement.CurrentPipe is { } pipe)
            {
                // Update pipe animation
                pipe.UpdateTokenPosition(visual.VisualToken);
            }
        });
    }
} 