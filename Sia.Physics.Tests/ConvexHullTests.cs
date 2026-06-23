namespace Sia.Physics.Tests;

public sealed class ConvexHullTests
{
    [Fact]
    public void RegistryStoresContiguousHullVertices()
    {
        using var world = new World();
        var shapes = world.GetPhysicsShapes();
        var hull = shapes.AddConvexHull([
            new float3(-1f, -1f, -1f),
            new float3(1f, -1f, -1f),
            new float3(0f, 1f, -1f),
            new float3(0f, 0f, 2f)
        ]);

        Assert.Equal(new float3(0f, 0f, 2f), shapes.Support(hull, new float3(0f, 0f, 1f)));
        var bounds = shapes.ComputeBounds(hull, RigidTransform.Translate(new float3(3f, 0f, 0f)));
        Assert.Equal(new float3(2f, -1f, -1f), bounds.Min);
        Assert.Equal(new float3(4f, 1f, 2f), bounds.Max);
    }
}
