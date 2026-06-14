namespace Sia.Physics;

public readonly record struct CapsuleShape : IConvexShape<CapsuleShape>
{
    public CapsuleShape(float radius, float halfLength)
    {
        if (!(radius > 0f) || !float.IsFinite(radius))
        {
            throw new ArgumentOutOfRangeException(nameof(radius));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(halfLength);
        Radius = radius;
        HalfLength = halfLength;
    }

    public float Radius { get; }

    public float HalfLength { get; }

    public static Aabb ComputeBounds(in CapsuleShape shape, in RigidTransform pose)
    {
        var extents = new float3(shape.Radius, shape.HalfLength + shape.Radius, shape.Radius);
        return GeometryMath.Transform(new Aabb(-extents, extents), pose);
    }

    public static MassProperties ComputeMass(in CapsuleShape shape, float density)
    {
        var radiusSquared = shape.Radius * shape.Radius;
        var cylinderMass = MathF.PI * radiusSquared * (2f * shape.HalfLength) * density;
        var sphereMass = (4f / 3f) * MathF.PI * radiusSquared * shape.Radius * density;
        var axial = 0.5f * cylinderMass * radiusSquared + 0.4f * sphereMass * radiusSquared;
        var radial = cylinderMass * (3f * radiusSquared + 4f * shape.HalfLength * shape.HalfLength) / 12f +
            sphereMass * (0.4f * radiusSquared + shape.HalfLength * shape.HalfLength);
        return new MassProperties(cylinderMass + sphereMass, new float3(radial, axial, radial), float3.zero);
    }

    public static float3 Support(in CapsuleShape shape, float3 direction)
    {
        var offset = direction.y < 0f ? -shape.HalfLength : shape.HalfLength;
        return GeometryMath.NormalizeOr(direction, new float3(1f, 0f, 0f)) * shape.Radius + new float3(0f, offset, 0f);
    }
}
