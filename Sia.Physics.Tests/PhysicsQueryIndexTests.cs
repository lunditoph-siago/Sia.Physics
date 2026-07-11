namespace Sia.Physics.Tests;

public sealed class PhysicsQueryIndexTests
{
    [Fact]
    public void BvhBuildsCompactLeavesAndRefitsStableBodies()
    {
        using var index = new PhysicsQueryIndex();
        var bounds = new Aabb[32];
        for (var i = 0; i < bounds.Length; i++)
        {
            var center = new float3(i * 2f, i % 3, 0f);
            bounds[i] = Aabb.CreateFromCenterAndHalfExtents(center, new float3(0.5f));
        }

        index.BuildBvh(bounds);

        Assert.InRange(index.NodeCount, 1, bounds.Length - 1);
        Assert.Equal(bounds.Length, index.BodyCount);
        Assert.Equal(1, index.BvhRebuildCount);
        foreach (ref readonly var bodyBounds in bounds.AsSpan())
        {
            Assert.True(index.NodeBounds[0].Contains(bodyBounds));
        }

        bounds[0] = Aabb.CreateFromCenterAndHalfExtents(new float3(0.25f, 0f, 0f), new float3(0.5f));
        index.BuildBvh(bounds);

        Assert.Equal(1, index.BvhRebuildCount);
        Assert.True(index.NodeBounds[0].Contains(bounds[0]));
    }
}
