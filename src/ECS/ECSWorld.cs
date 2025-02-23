using Friflo.Engine.ECS;
using static Friflo.Engine.ECS.EntityExtensions;
using Friflo.Engine.ECS.Systems;  // For SystemRoot
using F.ECS.Systems;
using F.ECS.Components;
using Godot;
using F.Game.BlockLogic;
using F.Game.Connections;
using F.Game.Tokens;

namespace F.ECS;

public class ECSWorld
{
    private readonly EntityStore _entityStore;
    private readonly MovementSystem _movementSystem;
    private readonly TokenMovementSystem _tokenMovementSystem;
    private readonly TokenProcessingSystem _tokenProcessingSystem;
    private readonly Node _visualParent;  // Parent node for visual elements

    public ECSWorld(Node visualParent)
    {
        _entityStore = new EntityStore();
        _movementSystem = new MovementSystem(_entityStore);
        _tokenMovementSystem = new TokenMovementSystem(_entityStore);
        _tokenProcessingSystem = new TokenProcessingSystem(_entityStore);
        _visualParent = visualParent;
    }

    public Entity CreateMovingEntity(float x, float y, float vx, float vy)
    {
        var entity = _entityStore.CreateEntity();
        var pos = new PositionComponent { X = x, Y = y };
        var vel = new VelocityComponent { X = vx, Y = vy };
        entity.AddComponent(pos);
        entity.AddComponent(vel);
        return entity;
    }

    public Entity CreateToken(IBlock startBlock, float initialValue = 0)
    {
        var entity = _entityStore.CreateEntity();
        
        // Create visual token
        var tokenScene = GD.Load<PackedScene>("res://scenes/Token.tscn");
        var visualToken = tokenScene.Instantiate<Token>();
        _visualParent.AddChild(visualToken);
        visualToken.Initialize(startBlock, initialValue);
        
        // Add components
        entity.AddComponent(new TokenValueComponent { Value = initialValue });
        entity.AddComponent(new TokenBlockComponent 
        { 
            CurrentBlock = startBlock,
            ProcessedBlocks = new HashSet<IBlock>()
        });
        entity.AddComponent(new TokenMovementComponent
        {
            Position = startBlock.GetTokenPosition(),
            IsMoving = false
        });
        entity.AddComponent(new TokenVisualComponent
        {
            VisualToken = visualToken
        });
        
        return entity;
    }

    public void MoveToken(Entity token, IBlock targetBlock, ConnectionPipe? pipe = null)
    {
        if (!token.HasComponent<TokenMovementComponent>() || 
            !token.HasComponent<TokenBlockComponent>()) return;

        ref var movement = ref token.GetComponent<TokenMovementComponent>();
        ref var blockComponent = ref token.GetComponent<TokenBlockComponent>();

        movement.TargetPosition = targetBlock.GetTokenPosition();
        movement.IsMoving = true;
        movement.CurrentPipe = pipe;

        blockComponent.TargetBlock = targetBlock;

        if (pipe != null && token.HasComponent<TokenVisualComponent>())
        {
            ref var visual = ref token.GetComponent<TokenVisualComponent>();
            pipe.StartTokenMovement(visual.VisualToken);
        }
    }

    public void Update(float delta)
    {
        _movementSystem.Update();
        _tokenMovementSystem.Update(delta);
        _tokenProcessingSystem.Update();
    }

    public (float x, float y) GetEntityPosition(Entity entity)
    {
        var pos = entity.GetComponent<PositionComponent>();
        return (pos.X, pos.Y);
    }

    public void SetEntityVelocity(Entity entity, float vx, float vy)
    {
        var vel = new VelocityComponent { X = vx, Y = vy };
        entity.AddComponent(vel);
    }

    public (float x, float y) GetTokenPosition(Entity token)
    {
        if (!token.HasComponent<TokenMovementComponent>()) 
            return (0, 0);

        ref var movement = ref token.GetComponent<TokenMovementComponent>();
        return (movement.Position.X, movement.Position.Y);
    }

    public float GetTokenValue(Entity token)
    {
        if (!token.HasComponent<TokenValueComponent>()) 
            return 0;

        ref var value = ref token.GetComponent<TokenValueComponent>();
        return value.Value;
    }

    public void SetTokenValue(Entity token, float value)
    {
        if (!token.HasComponent<TokenValueComponent>()) 
            return;

        ref var valueComponent = ref token.GetComponent<TokenValueComponent>();
        valueComponent.Value = value;

        // Update visual token if it exists
        if (token.HasComponent<TokenVisualComponent>())
        {
            ref var visual = ref token.GetComponent<TokenVisualComponent>();
            visual.VisualToken.UpdateValue(value);
        }
    }
} 