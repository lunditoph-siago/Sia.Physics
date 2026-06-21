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
        using var stage = SystemChain.Empty.Add<BuildPhysicsFrameSystem>().CreateStage(world);
        stage.Tick();

        var collector = new ClosestHitCollector(10f);
        var ray = new PhysicsRay(float3.zero, new float3(1f, 0f, 0f), 10f);
        PhysicsQueries.Raycast(world.GetAddon<PhysicsFrame>(), shapes, ray, ref collector);

        Assert.True(collector.HasHit);
        Assert.Equal(near, collector.Hit.Entity);
        Assert.Equal(2f, collector.Hit.Distance, 5);
        Assert.Equal(new float3(-1f, 0f, 0f), collector.Hit.Normal);
    }
}

