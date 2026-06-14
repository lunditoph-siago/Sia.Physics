using System.Runtime.CompilerServices;

namespace Sia.Physics;

public static class GeometryMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 NormalizeOr(float3 value, float3 fallback)
    {
        var lengthSquared = math.lengthsq(value);
        return lengthSquared > 1e-20f ? value * math.rsqrt(lengthSquared) : fallback;
    }

    public static Aabb Transform(in Aabb localBounds, in RigidTransform pose)
    {
        var localHalfExtents = localBounds.HalfExtents;
        var worldCenter = math.transform(pose, localBounds.Center);
        var axisX = math.abs(math.rotate(pose, new float3(localHalfExtents.x, 0f, 0f)));
        var axisY = math.abs(math.rotate(pose, new float3(0f, localHalfExtents.y, 0f)));
        var axisZ = math.abs(math.rotate(pose, new float3(0f, 0f, localHalfExtents.z)));
        var worldHalfExtents = axisX + axisY + axisZ;
        return new Aabb(worldCenter - worldHalfExtents, worldCenter + worldHalfExtents);
    }

    public static bool IntersectRay(in Aabb bounds, in PhysicsRay ray, out float distance)
    {
        var inverseDirection = 1f / ray.Direction;
        var first = (bounds.Min - ray.Origin) * inverseDirection;
        var second = (bounds.Max - ray.Origin) * inverseDirection;
        var near = math.min(first, second);
        var far = math.max(first, second);
        var entry = System.Math.Max(near.x, System.Math.Max(near.y, near.z));
        var exit = System.Math.Min(far.x, System.Math.Min(far.y, far.z));
        distance = System.Math.Max(0f, entry);
        return exit >= distance && distance <= ray.MaximumDistance;
    }
}
