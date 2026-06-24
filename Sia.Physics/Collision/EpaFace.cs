namespace Sia.Physics;

internal readonly record struct EpaFace(int A, int B, int C, float3 Normal, float Distance)
{
    public static bool TryCreate(
        ReadOnlySpan<SupportPoint> vertices,
        int a,
        int b,
        int c,
        out EpaFace face)
    {
        var normal = math.cross(
            vertices[b].Difference - vertices[a].Difference,
            vertices[c].Difference - vertices[a].Difference);
        var lengthSquared = math.lengthsq(normal);
        if (lengthSquared <= 1e-16f)
        {
            face = default;
            return false;
        }

        normal *= math.rsqrt(lengthSquared);
        var distance = math.dot(normal, vertices[a].Difference);
        if (distance < 0f)
        {
            (b, c) = (c, b);
            normal = -normal;
            distance = -distance;
        }

        face = new EpaFace(a, b, c, normal, distance);
        return true;
    }
}

