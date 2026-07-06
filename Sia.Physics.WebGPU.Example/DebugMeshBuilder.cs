using System.Numerics;
using System.Runtime.InteropServices;

namespace Sia.Physics.WebGPU.Example;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct DebugVertex(
    Vector3 Position,
    Vector3 Normal,
    Vector4 Color)
{
    public bool IsFinite =>
        float.IsFinite(Position.X) &&
        float.IsFinite(Position.Y) &&
        float.IsFinite(Position.Z) &&
        float.IsFinite(Normal.X) &&
        float.IsFinite(Normal.Y) &&
        float.IsFinite(Normal.Z) &&
        float.IsFinite(Color.X) &&
        float.IsFinite(Color.Y) &&
        float.IsFinite(Color.Z) &&
        float.IsFinite(Color.W);
}

internal sealed class DebugMeshBuilder
{
    public const int VertexStride = 40;

    private readonly List<DebugVertex> _vertices = new(32_768);

    public int VertexCount => _vertices.Count;

    public int TriangleCount => _vertices.Count / 3;

    public ReadOnlySpan<DebugVertex> Vertices => CollectionsMarshal.AsSpan(_vertices);

    public void Clear() => _vertices.Clear();

    public void AddTriangle(float3 a, float3 b, float3 c, Vector4 color)
    {
        var cross = math.cross(b - a, c - a);
        var lengthSquared = math.lengthsq(cross);
        if (!(lengthSquared > 1e-12f) || !math.isfinite(lengthSquared))
        {
            return;
        }

        var normal = ToVector3(cross * math.rsqrt(lengthSquared));
        _vertices.Add(new(ToVector3(a), normal, color));
        _vertices.Add(new(ToVector3(b), normal, color));
        _vertices.Add(new(ToVector3(c), normal, color));
    }

    public void AddQuad(float3 a, float3 b, float3 c, float3 d, Vector4 color)
    {
        AddTriangle(a, b, c, color);
        AddTriangle(a, c, d, color);
    }

    public void AddBox(in RigidTransform transform, float3 halfExtents, Vector4 color)
    {
        Span<float3> vertices = stackalloc float3[]
        {
            new(-halfExtents.x, -halfExtents.y, -halfExtents.z),
            new(halfExtents.x, -halfExtents.y, -halfExtents.z),
            new(halfExtents.x, halfExtents.y, -halfExtents.z),
            new(-halfExtents.x, halfExtents.y, -halfExtents.z),
            new(-halfExtents.x, -halfExtents.y, halfExtents.z),
            new(halfExtents.x, -halfExtents.y, halfExtents.z),
            new(halfExtents.x, halfExtents.y, halfExtents.z),
            new(-halfExtents.x, halfExtents.y, halfExtents.z),
        };
        Span<int3> triangles = stackalloc int3[]
        {
            new(0, 2, 1), new(0, 3, 2),
            new(4, 5, 6), new(4, 6, 7),
            new(0, 1, 5), new(0, 5, 4),
            new(3, 7, 6), new(3, 6, 2),
            new(0, 4, 7), new(0, 7, 3),
            new(1, 2, 6), new(1, 6, 5),
        };
        foreach (ref readonly var triangle in triangles)
        {
            AddTriangle(
                math.transform(transform, vertices[triangle.x]),
                math.transform(transform, vertices[triangle.y]),
                math.transform(transform, vertices[triangle.z]),
                color);
        }
    }

    public void AddSphere(
        in RigidTransform transform,
        float radius,
        Vector4 color,
        int longitudeSegments = 12,
        int latitudeSegments = 8)
    {
        longitudeSegments = System.Math.Max(longitudeSegments, 6);
        latitudeSegments = System.Math.Max(latitudeSegments, 4);
        for (var latitude = 0; latitude < latitudeSegments; latitude++)
        {
            var phi0 = -math.PI * 0.5f + (float)latitude / latitudeSegments * math.PI;
            var phi1 = -math.PI * 0.5f + (float)(latitude + 1) / latitudeSegments * math.PI;
            for (var longitude = 0; longitude < longitudeSegments; longitude++)
            {
                var theta0 = longitude * 2f * math.PI / longitudeSegments;
                var theta1 = (longitude + 1) * 2f * math.PI / longitudeSegments;
                var a = SpherePoint(phi0, theta0) * radius;
                var b = SpherePoint(phi0, theta1) * radius;
                var c = SpherePoint(phi1, theta1) * radius;
                var d = SpherePoint(phi1, theta0) * radius;
                if (latitude > 0)
                {
                    AddTriangle(
                        math.transform(transform, a),
                        math.transform(transform, c),
                        math.transform(transform, b),
                        color);
                }
                if (latitude + 1 < latitudeSegments)
                {
                    AddTriangle(
                        math.transform(transform, a),
                        math.transform(transform, d),
                        math.transform(transform, c),
                        color);
                }
            }
        }
    }

    public void AddCapsule(
        in RigidTransform transform,
        float radius,
        float halfLength,
        Vector4 color)
    {
        var first = math.transform(transform, new float3(0f, -halfLength, 0f));
        var second = math.transform(transform, new float3(0f, halfLength, 0f));
        AddSphere(RigidTransform.Translate(first), radius, color, 10, 6);
        AddSphere(RigidTransform.Translate(second), radius, color, 10, 6);
        AddCylinderBetween(first, second, radius, 10, color);
    }

    public void AddLine(float3 start, float3 end, float thickness, Vector4 color) =>
        AddCylinderBetween(start, end, thickness, 8, color);

    public void AddMarker(float3 position, float radius, Vector4 color) =>
        AddSphere(RigidTransform.Translate(position), radius, color, 10, 6);

    private void AddCylinderBetween(
        float3 first,
        float3 second,
        float radius,
        int sideCount,
        Vector4 color)
    {
        var axis = second - first;
        var lengthSquared = math.lengthsq(axis);
        if (!(lengthSquared > 1e-12f))
        {
            return;
        }

        var direction = axis * math.rsqrt(lengthSquared);
        var reference = math.abs(direction.y) < 0.9f
            ? new float3(0f, 1f, 0f)
            : new float3(1f, 0f, 0f);
        var tangent = math.normalize(math.cross(direction, reference));
        var bitangent = math.cross(direction, tangent);
        for (var side = 0; side < sideCount; side++)
        {
            var angle0 = side * 2f * math.PI / sideCount;
            var angle1 = (side + 1) * 2f * math.PI / sideCount;
            var offset0 = (tangent * math.cos(angle0) + bitangent * math.sin(angle0)) * radius;
            var offset1 = (tangent * math.cos(angle1) + bitangent * math.sin(angle1)) * radius;
            AddQuad(
                first + offset0,
                first + offset1,
                second + offset1,
                second + offset0,
                color);
        }
    }

    private static float3 SpherePoint(float latitude, float longitude)
    {
        var radial = math.cos(latitude);
        return new(
            radial * math.cos(longitude),
            math.sin(latitude),
            radial * math.sin(longitude));
    }

    private static Vector3 ToVector3(float3 value) => new(value.x, value.y, value.z);
}
