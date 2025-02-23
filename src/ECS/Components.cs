using Friflo.Engine.ECS;

namespace F.ECS;

public struct PositionComponent : IComponent
{
    public float X;
    public float Y;
}

public struct VelocityComponent : IComponent
{
    public float X;
    public float Y;
} 