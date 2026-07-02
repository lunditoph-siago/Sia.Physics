namespace Sia.Physics;

public readonly record struct PhysicsContactEvent(
    Entity Other,
    float3 Point,
    float3 Normal,
    float Penetration) : IEvent;

