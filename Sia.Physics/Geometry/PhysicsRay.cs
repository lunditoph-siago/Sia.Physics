namespace Sia.Physics;

public readonly record struct PhysicsRay
{
    public PhysicsRay(float3 origin, float3 direction, float maximumDistance = float.PositiveInfinity)
    {
        if (!float.IsFinite(direction.x) || !float.IsFinite(direction.y) || !float.IsFinite(direction.z))
        {
            throw new ArgumentException("Direction must be finite.", nameof(direction));
        }

        var lengthSquared = math.lengthsq(direction);
        if (!(lengthSquared > 1e-20f))
        {
            throw new ArgumentException("Direction must be non-zero.", nameof(direction));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(maximumDistance);
        Origin = origin;
        Direction = direction * math.rsqrt(lengthSquared);
        MaximumDistance = maximumDistance;
    }

    public float3 Origin { get; }

    public float3 Direction { get; }

    public float MaximumDistance { get; }

    public float3 GetPoint(float distance) => Origin + Direction * distance;
}

