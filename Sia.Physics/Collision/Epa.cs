namespace Sia.Physics;

internal static class Epa
{
    private const int MaximumVertices = 64;
    private const int MaximumFaces = 128;
    private const int MaximumEdges = 192;
    private const float Tolerance = 1e-4f;

    public static bool TrySolve(
        PhysicsShapes shapes,
        in BodyState first,
        in BodyState second,
        in GjkSimplex simplex,
        out float3 point,
        out float3 normal,
        out float penetration)
    {
        point = default;
        normal = default;
        penetration = default;
        if (simplex.Count != 4)
        {
            return false;
        }

        Span<SupportPoint> vertices = stackalloc SupportPoint[MaximumVertices];
        for (var i = 0; i < 4; i++)
        {
            vertices[i] = simplex[i];
        }
        var vertexCount = 4;

        Span<EpaFace> faces = stackalloc EpaFace[MaximumFaces];
        var faceCount = 0;
        AddFace(vertices, faces, ref faceCount, 0, 1, 2);
        AddFace(vertices, faces, ref faceCount, 0, 3, 1);
        AddFace(vertices, faces, ref faceCount, 0, 2, 3);
        AddFace(vertices, faces, ref faceCount, 1, 3, 2);

        Span<EpaEdge> edges = stackalloc EpaEdge[MaximumEdges];
        for (var iteration = 0; iteration < MaximumVertices - 4; iteration++)
        {
            var closestFaceIndex = FindClosestFace(faces[..faceCount]);
            ref readonly var closestFace = ref faces[closestFaceIndex];
            var support = ConvexSupport.Get(shapes, first, second, closestFace.Normal);
            var projection = math.dot(support.Difference, closestFace.Normal);
            if (projection - closestFace.Distance <= Tolerance)
            {
                ResolveContact(vertices, closestFace, out point);
                normal = closestFace.Normal;
                penetration = projection;
                return true;
            }

            if (vertexCount == MaximumVertices)
            {
                break;
            }

            var newVertex = vertexCount++;
            vertices[newVertex] = support;
            var edgeCount = 0;
            for (var faceIndex = faceCount - 1; faceIndex >= 0; faceIndex--)
            {
                var face = faces[faceIndex];
                if (math.dot(face.Normal, support.Difference - vertices[face.A].Difference) <= Tolerance)
                {
                    continue;
                }

                AddBoundaryEdge(edges, ref edgeCount, face.A, face.B);
                AddBoundaryEdge(edges, ref edgeCount, face.B, face.C);
                AddBoundaryEdge(edges, ref edgeCount, face.C, face.A);
                faces[faceIndex] = faces[--faceCount];
            }

            for (var edgeIndex = 0; edgeIndex < edgeCount; edgeIndex++)
            {
                if (faceCount == MaximumFaces)
                {
                    return false;
                }
                AddFace(vertices, faces, ref faceCount, edges[edgeIndex].Start, edges[edgeIndex].End, newVertex);
            }
        }

        return false;
    }

    private static void ResolveContact(
        ReadOnlySpan<SupportPoint> vertices,
        in EpaFace face,
        out float3 point)
    {
        ref readonly var a = ref vertices[face.A];
        ref readonly var b = ref vertices[face.B];
        ref readonly var c = ref vertices[face.C];
        var target = face.Normal * face.Distance;
        var weights = Barycentric(target, a.Difference, b.Difference, c.Difference);
        var pointA = a.PointA * weights.x + b.PointA * weights.y + c.PointA * weights.z;
        var pointB = a.PointB * weights.x + b.PointB * weights.y + c.PointB * weights.z;
        point = (pointA + pointB) * 0.5f;
    }

    private static float3 Barycentric(float3 point, float3 a, float3 b, float3 c)
    {
        var first = b - a;
        var second = c - a;
        var offset = point - a;
        var d00 = math.dot(first, first);
        var d01 = math.dot(first, second);
        var d11 = math.dot(second, second);
        var d20 = math.dot(offset, first);
        var d21 = math.dot(offset, second);
        var denominator = d00 * d11 - d01 * d01;
        if (MathF.Abs(denominator) <= 1e-16f)
        {
            return new float3(1f, 0f, 0f);
        }

        var y = (d11 * d20 - d01 * d21) / denominator;
        var z = (d00 * d21 - d01 * d20) / denominator;
        return new float3(1f - y - z, y, z);
    }

    private static int FindClosestFace(ReadOnlySpan<EpaFace> faces)
    {
        var result = 0;
        for (var i = 1; i < faces.Length; i++)
        {
            if (faces[i].Distance < faces[result].Distance)
            {
                result = i;
            }
        }
        return result;
    }

    private static void AddFace(
        ReadOnlySpan<SupportPoint> vertices,
        Span<EpaFace> faces,
        ref int faceCount,
        int a,
        int b,
        int c)
    {
        if (EpaFace.TryCreate(vertices, a, b, c, out var face))
        {
            faces[faceCount++] = face;
        }
    }

    private static void AddBoundaryEdge(Span<EpaEdge> edges, ref int edgeCount, int start, int end)
    {
        for (var i = 0; i < edgeCount; i++)
        {
            if (edges[i].Start != end || edges[i].End != start)
            {
                continue;
            }

            edges[i] = edges[--edgeCount];
            return;
        }

        if (edgeCount < edges.Length)
        {
            edges[edgeCount++] = new EpaEdge(start, end);
        }
    }
}

