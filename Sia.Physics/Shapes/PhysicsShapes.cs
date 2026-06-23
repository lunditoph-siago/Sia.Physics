namespace Sia.Physics;

public sealed class PhysicsShapes : IAddon, IDisposable
{
    private readonly ShapeBatch<SphereShape> _spheres = new(ShapeType.Sphere);
    private readonly ShapeBatch<BoxShape> _boxes = new(ShapeType.Box);
    private readonly ShapeBatch<CapsuleShape> _capsules = new(ShapeType.Capsule);
    private readonly ShapeBatch<ConvexHullShape> _convexHulls = new(ShapeType.ConvexHull);
    private readonly NativeList<float3> _convexVertices = new(128);

    public ShapeHandle Add(in SphereShape shape) => _spheres.Add(shape);

    public ShapeHandle Add(in BoxShape shape) => _boxes.Add(shape);

    public ShapeHandle Add(in CapsuleShape shape) => _capsules.Add(shape);

    public ShapeHandle AddConvexHull(ReadOnlySpan<float3> vertices)
    {
        if (vertices.Length < 4)
        {
            throw new ArgumentException("A convex hull requires at least four vertices.", nameof(vertices));
        }

        foreach (ref readonly var vertex in vertices)
        {
            if (!math.all(math.isfinite(vertex)))
            {
                throw new ArgumentException("Convex hull vertices must be finite.", nameof(vertices));
            }
        }

        var start = _convexVertices.AddRange(vertices);
        return _convexHulls.Add(new ConvexHullShape(
            start,
            vertices.Length,
            ConvexHullMath.ComputeLocalBounds(vertices)));
    }

    public Aabb ComputeBounds(ShapeHandle handle, in RigidTransform pose) => handle.Type switch
    {
        ShapeType.Sphere => SphereShape.ComputeBounds(_spheres.Get(handle), pose),
        ShapeType.Box => BoxShape.ComputeBounds(_boxes.Get(handle), pose),
        ShapeType.Capsule => CapsuleShape.ComputeBounds(_capsules.Get(handle), pose),
        ShapeType.ConvexHull => GeometryMath.Transform(_convexHulls.Get(handle).LocalBounds, pose),
        _ => throw new NotSupportedException($"Shape type {handle.Type} is not registered.")
    };

    public MassProperties ComputeMass(ShapeHandle handle, float density) => handle.Type switch
    {
        ShapeType.Sphere => SphereShape.ComputeMass(_spheres.Get(handle), density),
        ShapeType.Box => BoxShape.ComputeMass(_boxes.Get(handle), density),
        ShapeType.Capsule => CapsuleShape.ComputeMass(_capsules.Get(handle), density),
        ShapeType.ConvexHull => ConvexHullMath.ComputeConservativeMass(_convexHulls.Get(handle).LocalBounds, density),
        _ => throw new NotSupportedException($"Shape type {handle.Type} is not registered.")
    };

    public float3 Support(ShapeHandle handle, float3 direction) => handle.Type switch
    {
        ShapeType.Sphere => SphereShape.Support(_spheres.Get(handle), direction),
        ShapeType.Box => BoxShape.Support(_boxes.Get(handle), direction),
        ShapeType.Capsule => CapsuleShape.Support(_capsules.Get(handle), direction),
        ShapeType.ConvexHull => SupportConvexHull(_convexHulls.Get(handle), direction),
        _ => throw new NotSupportedException($"Shape type {handle.Type} is not registered.")
    };

    public bool Raycast(
        ShapeHandle handle,
        in RigidTransform pose,
        in PhysicsRay ray,
        out float distance,
        out float3 normal)
    {
        switch (handle.Type)
        {
            case ShapeType.Sphere:
                return ShapeRaycast.Cast(_spheres.Get(handle), pose, ray, out distance, out normal);
            case ShapeType.Box:
                return ShapeRaycast.Cast(_boxes.Get(handle), pose, ray, out distance, out normal);
            case ShapeType.Capsule:
                return ShapeRaycast.Cast(_capsules.Get(handle), pose, ray, out distance, out normal);
            default:
                distance = default;
                normal = default;
                return false;
        }
    }

    public void OnUninitialize(World world) => Dispose();

    public void Dispose()
    {
        _spheres.Dispose();
        _boxes.Dispose();
        _capsules.Dispose();
        _convexHulls.Dispose();
        _convexVertices.Dispose();
    }

    private float3 SupportConvexHull(in ConvexHullShape shape, float3 direction) =>
        ConvexHullMath.Support(_convexVertices.ReadOnlySpan.Slice(shape.VertexStart, shape.VertexCount), direction);
}
