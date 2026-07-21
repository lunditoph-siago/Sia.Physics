namespace Sia.Physics.Tests;

public sealed class PhysicsQueryTests
{
    [Fact]
    public void ClosestCollectorReturnsNearestShape()
    {
        using var world = new World();
        var shapes = world.GetPhysicsShapes();
        var sphere = shapes.Add(new SphereShape(1f));
        var near = world.CreateStaticBody(RigidTransform.Translate(new float3(3f, 0f, 0f)), sphere);
        world.CreateStaticBody(RigidTransform.Translate(new float3(7f, 0f, 0f)), sphere);
        using var stage = SystemChain.Empty
            .Add<BuildPhysicsFrameSystem>()
            .Add<BuildPhysicsQueryIndexSystem>()
            .CreateStage(world);
        stage.Tick();

        var collector = new ClosestHitCollector(10f);
        var ray = new PhysicsRay(float3.zero, new float3(1f, 0f, 0f), 10f);
        PhysicsQueries.Raycast(world.GetAddon<PhysicsFrame>(), shapes, ray, ref collector);

        Assert.True(collector.HasHit);
        Assert.Equal(near, collector.Hit.Entity);
        Assert.Equal(2f, collector.Hit.Distance, 5);
        Assert.Equal(new float3(-1f, 0f, 0f), collector.Hit.Normal);
    }

    [Fact]
    public void OverlapAabbHonorsCollisionFilter()
    {
        using var world = new World();
        var shapes = world.GetPhysicsShapes();
        var sphere = shapes.Add(new SphereShape(1f));
        var ignored = new PhysicsCollider(new CollisionFilter(1, 1), PhysicsMaterial.Default);
        world.CreateStaticBody(RigidTransform.Identity, sphere, ignored);
        var expected = world.CreateStaticBody(
            RigidTransform.Translate(new float3(2f, 0f, 0f)),
            sphere,
            new PhysicsCollider(new CollisionFilter(2, 2), PhysicsMaterial.Default));
        using var stage = SystemChain.Empty
            .Add<BuildPhysicsFrameSystem>()
            .Add<BuildPhysicsQueryIndexSystem>()
            .CreateStage(world);
        stage.Tick();

        var collector = new AnyOverlapCollector();
        var filter = new PhysicsQueryFilter(new CollisionFilter(2, 2));
        PhysicsQueries.OverlapAabb(
            world.GetAddon<PhysicsFrame>(),
            new Aabb(new float3(-1f), new float3(3f)),
            filter,
            ref collector);

        Assert.True(collector.HasHit);
        Assert.Equal(expected, collector.Hit.Entity);
    }

    [Fact]
    public void SweepOverlapMatchesPackedFrameScanNearUpperBoundary()
    {
        using var world = new World();
        var sphere = world.GetPhysicsShapes().Add(new SphereShape(0.4f));
        for (var i = 0; i < 101; i++)
        {
            world.CreateStaticBody(RigidTransform.Translate(new float3(i * 2f, i % 5, 0f)), sphere);
        }
        using var stage = SystemChain.Empty
            .Add<BuildPhysicsFrameSystem>()
            .Add<BuildPhysicsQueryIndexSystem>()
            .CreateStage(world);
        stage.Tick();

        var frame = world.GetAddon<PhysicsFrame>();
        var queryBounds = new Aabb(new float3(189f, -1f, -1f), new float3(201f, 6f, 1f));
        var expected = Enumerable.Range(0, frame.Bounds.Length)
            .Where(index => queryBounds.Overlaps(frame.Bounds[index]))
            .ToHashSet();
        var actual = new HashSet<int>();
        var collector = new BodyIndexCollector(actual);

        PhysicsQueries.OverlapAabb(frame, queryBounds, ref collector);

        Assert.Equal(expected, actual);
    }

    private readonly struct BodyIndexCollector(HashSet<int> bodyIndices) : IOverlapCollector
    {
        public bool AddHit(in OverlapHit hit)
        {
            bodyIndices.Add(hit.BodyIndex);
            return true;
        }
    }
}
