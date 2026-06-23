namespace Sia.Physics;

public readonly record struct ConvexHullShape
{
    internal ConvexHullShape(int vertexStart, int vertexCount, in Aabb localBounds)
    {
        VertexStart = vertexStart;
        VertexCount = vertexCount;
        LocalBounds = localBounds;
    }

    public int VertexStart { get; }

    public int VertexCount { get; }

    public Aabb LocalBounds { get; }
}

