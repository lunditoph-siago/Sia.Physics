namespace Sia.Physics;

public readonly record struct ParticleDistanceConstraint
{
    public ParticleDistanceConstraint(EntityId first, EntityId second, float restLength, float compliance = 0f)
    {
        if (first == second)
        {
            throw new ArgumentException("A constraint requires two different particles.", nameof(second));
        }
        ArgumentOutOfRangeException.ThrowIfNegative(restLength);
        ArgumentOutOfRangeException.ThrowIfNegative(compliance);
        First = first;
        Second = second;
        RestLength = restLength;
        Compliance = compliance;
    }

    public EntityId First { get; }

    public EntityId Second { get; }

    public float RestLength { get; }

    public float Compliance { get; }
}

