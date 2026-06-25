namespace Sia.Physics;

internal struct DistanceJointConstraint
{
    public int First;
    public int Second;
    public float RestLength;
    public float3 LocalAnchorA;
    public float3 LocalAnchorB;
    public float Compliance;
    public float AccumulatedImpulse;
}

