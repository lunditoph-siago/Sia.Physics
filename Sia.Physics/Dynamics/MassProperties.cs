namespace Sia.Physics;

public readonly record struct MassProperties
{
    public MassProperties(float mass, float3 inertiaDiagonal, float3 centerOfMass)
    {
        if (!(mass > 0f) || !float.IsFinite(mass))
        {
            throw new ArgumentOutOfRangeException(nameof(mass));
        }

        if (!(inertiaDiagonal.x > 0f && inertiaDiagonal.y > 0f && inertiaDiagonal.z > 0f))
        {
            throw new ArgumentOutOfRangeException(nameof(inertiaDiagonal));
        }

        Mass = mass;
        InertiaDiagonal = inertiaDiagonal;
        CenterOfMass = centerOfMass;
    }

    public float Mass { get; }

    public float InverseMass => 1f / Mass;

    public float3 InertiaDiagonal { get; }

    public float3 InverseInertiaDiagonal => 1f / InertiaDiagonal;

    public float3 CenterOfMass { get; }
}

