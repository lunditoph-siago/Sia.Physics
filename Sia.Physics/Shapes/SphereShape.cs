namespace Sia.Physics;

public readonly record struct SphereShape : IConvexShape<SphereShape>
{
    public SphereShape(float radius)
    {
        if (!(radius > 0f) || !float.IsFinite(radius))
        {
            throw new ArgumentOutOfRangeException(nameof(radius));
        }

        Radius = radius;
    }

    public float Radius { get; }

    public static Aabb ComputeBounds(in SphereShape shape, in RigidTransform pose) =>
        new(pose.Translation - shape.Radius, pose.Translation + shape.Radius);

    public static MassProperties ComputeMass(in SphereShape shape, float density)
    {
        var mass = (4f / 3f) * MathF.PI * shape.Radius * shape.Radius * shape.Radius * density;
        var inertia = 0.4f * mass * shape.Radius * shape.Radius;
        return new MassProperties(mass, new float3(inertia), float3.zero);
    }

    public static float3 Support(in SphereShape shape, float3 direction) =>
        GeometryMath.NormalizeOr(direction, new float3(1f, 0f, 0f)) * shape.Radius;
}
