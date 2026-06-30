namespace Sia.Physics;

public readonly record struct PhysicsParticle
{
    public PhysicsParticle(float inverseMass, float radius)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(inverseMass);
        if (!(radius > 0f) || !float.IsFinite(radius))
        {
            throw new ArgumentOutOfRangeException(nameof(radius));
        }
        InverseMass = inverseMass;
        Radius = radius;
    }

    public float InverseMass { get; }

    public float Radius { get; }

    public bool IsDynamic => InverseMass > 0f;
}

