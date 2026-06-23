namespace Sia.Physics;

internal static class ConvexHullMath
{
    public static Aabb ComputeLocalBounds(ReadOnlySpan<float3> vertices)
    {
        var minimum = vertices[0];
        var maximum = vertices[0];
        foreach (ref readonly var vertex in vertices[1..])
        {
            minimum = math.min(minimum, vertex);
            maximum = math.max(maximum, vertex);
        }

        return new Aabb(minimum, maximum);
    }

    public static float3 Support(ReadOnlySpan<float3> vertices, float3 direction)
    {
        var result = vertices[0];
        var maximumProjection = math.dot(result, direction);
        foreach (ref readonly var vertex in vertices[1..])
        {
            var projection = math.dot(vertex, direction);
            if (projection > maximumProjection)
            {
                maximumProjection = projection;
                result = vertex;
            }
        }

        return result;
    }

    public static MassProperties ComputeConservativeMass(in Aabb bounds, float density)
    {
        var halfExtents = bounds.HalfExtents;
        var mass = 8f * halfExtents.x * halfExtents.y * halfExtents.z * density;
        var squared = halfExtents * halfExtents;
        var inertia = mass / 3f * new float3(
            squared.y + squared.z,
            squared.x + squared.z,
            squared.x + squared.y);
        return new MassProperties(mass, inertia, bounds.Center);
    }
}

