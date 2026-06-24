namespace Sia.Physics;

public readonly record struct DistanceJoint
{
    public DistanceJoint(
        EntityId first,
        EntityId second,
        float restLength,
        float3 localAnchorA = default,
        float3 localAnchorB = default,
        float compliance = 0f)
    {
        if (first == second)
        {
            throw new ArgumentException("A joint requires two different bodies.", nameof(second));
        }
        ArgumentOutOfRangeException.ThrowIfNegative(restLength);
        ArgumentOutOfRangeException.ThrowIfNegative(compliance);
        First = first;
        Second = second;
        RestLength = restLength;
        LocalAnchorA = localAnchorA;
        LocalAnchorB = localAnchorB;
        Compliance = compliance;
    }

    public EntityId First { get; }

    public EntityId Second { get; }

    public float RestLength { get; }

    public float3 LocalAnchorA { get; }

    public float3 LocalAnchorB { get; }

    public float Compliance { get; }
}

