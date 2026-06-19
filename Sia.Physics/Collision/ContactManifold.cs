namespace Sia.Physics;

public readonly record struct ContactManifold(
    BodyPair Pair,
    float3 Point,
    float3 Normal,
    float Penetration,
    float Friction,
    float Restitution);

