namespace Sia.Physics;

public readonly record struct RaycastHit(
    Entity Entity,
    int BodyIndex,
    float Distance,
    float3 Position,
    float3 Normal);

