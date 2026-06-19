namespace Sia.Physics;

internal struct ContactConstraint
{
    public BodyPair Pair;
    public float3 Normal;
    public float3 Tangent;
    public float3 OffsetA;
    public float3 OffsetB;
    public float NormalMass;
    public float TangentMass;
    public float VelocityBias;
    public float Friction;
    public float AccumulatedNormalImpulse;
    public float AccumulatedTangentImpulse;
}

