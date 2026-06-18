namespace Sia.Physics.Tests;

public sealed class BroadphaseTests
{
    [Fact]
    public void EmitsOnlyOverlappingFilteredPairs()
    {
        using var world = new World();
        var sphere = world.GetPhysicsShapes().Add(new SphereShape(1f));
        world.CreateStaticBody(RigidTransform.Identity, sphere);
        world.CreateDynamicBody(RigidTransform.Translate(new float3(1.5f, 0f, 0f)), sphere, 1f);
        world.CreateDynamicBody(RigidTransform.Translate(new float3(5f, 0f, 0f)), sphere, 1f);

        using var stage = SystemChain.Empty
            .Add<BuildPhysicsFrameSystem>()
            .Add<BroadphaseSystem>()
            .CreateStage(world);
        stage.Tick();

        var pairs = world.GetAddon<PhysicsFrame>().Pairs;
        Assert.Single(pairs.ToArray());
        Assert.Equal(new BodyPair(0, 1), pairs[0]);
    }
}

