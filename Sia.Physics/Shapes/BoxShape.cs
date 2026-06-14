namespace Sia.Physics;

public readonly record struct BoxShape : IConvexShape<BoxShape>
{
    public BoxShape(float3 halfExtents)
    {
        if (!(halfExtents.x > 0f && halfExtents.y > 0f && halfExtents.z > 0f))
        {
            throw new ArgumentOutOfRangeException(nameof(halfExtents));
        }

        HalfExtents = halfExtents;
    }

    public float3 HalfExtents { get; }

    public static Aabb ComputeBounds(in BoxShape shape, in RigidTransform pose) =>
        GeometryMath.Transform(new Aabb(-shape.HalfExtents, shape.HalfExtents), pose);

    public static MassProperties ComputeMass(in BoxShape shape, float density)
    {
        var mass = 8f * shape.HalfExtents.x * shape.HalfExtents.y * shape.HalfExtents.z * density;
        var squared = shape.HalfExtents * shape.HalfExtents;
        var inertia = mass / 3f * new float3(squared.y + squared.z, squared.x + squared.z, squared.x + squared.y);
        return new MassProperties(mass, inertia, float3.zero);
    }

    public static float3 Support(in BoxShape shape, float3 direction) => new(
        direction.x < 0f ? -shape.HalfExtents.x : shape.HalfExtents.x,
        direction.y < 0f ? -shape.HalfExtents.y : shape.HalfExtents.y,
        direction.z < 0f ? -shape.HalfExtents.z : shape.HalfExtents.z);
}
