namespace Sia.Physics.Tests;

public sealed class ContinuousCollisionTests
{
    [Fact]
    public void ContinuousBodiesProduceSweptFrameBounds()
    {
        using var world = new World();
        var configuration = world.GetPhysicsConfiguration();
        configuration.FixedDeltaTime = 1f;
        configuration.Gravity = float3.zero;
        var sphere = world.GetPhysicsShapes().Add(new SphereShape(0.5f));
        world.CreateContinuousBody(
            RigidTransform.Identity,
            sphere,
            1f,
            new PhysicsVelocity(new float3(10f, 0f, 0f), float3.zero),
            damping: new PhysicsDamping(0f, 0f));
        using var stage = SystemChain.Empty
            .Add<BuildPhysicsFrameSystem>()
            .Add<IntegratePhysicsSystem>()
            .CreateStage(world);

        stage.Tick();

        var bounds = Assert.Single(world.GetAddon<PhysicsFrame>().Bounds.ToArray());
        Assert.Equal(-0.5f, bounds.Min.x);
        Assert.Equal(10.5f, bounds.Max.x);
    }
}
