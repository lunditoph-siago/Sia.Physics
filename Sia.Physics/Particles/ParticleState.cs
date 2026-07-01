namespace Sia.Physics;

internal struct ParticleState
{
    public float3 Position;
    public float3 Velocity;
    public float InverseMass;
    public float Radius;
    public float3 PositionBeforeSolve;
}

