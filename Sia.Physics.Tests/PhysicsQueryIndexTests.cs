namespace Sia.Physics.Tests;

public sealed class PhysicsQueryIndexTests
{
    [Fact]
    public void EcsSystemBuildsIndexFromPackedFrame()
    {
        using var world = new World();
        var sphere = world.GetPhysicsShapes().Add(new SphereShape(0.5f));
        for (var i = 0; i < 17; i++)
        {
            world.CreateStaticBody(RigidTransform.Translate(new float3(i, 0f, 0f)), sphere);
        }
        using var stage = SystemChain.Empty
            .Add<BuildPhysicsFrameSystem>()
            .Add<BuildPhysicsQueryIndexSystem>()
            .CreateStage(world);

        stage.Tick();

        var frame = world.GetAddon<PhysicsFrame>();
        Assert.Equal(frame.Bodies.Length, frame.QueryIndex.BodyCount);
        Assert.NotEqual(0, frame.QueryIndex.NodeCount);
    }

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

    [Fact]
    public void SweepIndexKeepsBothBoundaryOrdersCoherent()
    {
        using var index = new PhysicsQueryIndex();
        var bounds = new Aabb[24];
        for (var i = 0; i < bounds.Length; i++)
        {
            var center = new float3((i * 7) % bounds.Length, 0f, 0f);
            bounds[i] = Aabb.CreateFromCenterAndHalfExtents(center, new float3(i % 3 + 0.25f));
        }

        index.BuildSweep(bounds);

        Assert.Equal(SweepAxis.X, index.ProjectionAxis);
        AssertSorted(index.MinimumOrder, bounds, maximum: false);
        AssertSorted(index.MaximumOrder, bounds, maximum: true);

        index.BuildSweep(bounds);

        Assert.Equal(0, index.SweepSortSwapCount);
    }

    private static void AssertSorted(ReadOnlySpan<int> order, ReadOnlySpan<Aabb> bounds, bool maximum)
    {
        for (var i = 1; i < order.Length; i++)
        {
            var previous = maximum ? bounds[order[i - 1]].Max.x : bounds[order[i - 1]].Min.x;
            var current = maximum ? bounds[order[i]].Max.x : bounds[order[i]].Min.x;
            Assert.True(previous <= current);
        }
    }
}
