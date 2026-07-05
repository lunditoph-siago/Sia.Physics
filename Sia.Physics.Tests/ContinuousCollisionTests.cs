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
            .Add<ApplyContinuousBoundsSystem>()
            .Add<IntegratePhysicsSystem>()
            .CreateStage(world);

        stage.Tick();

        var bounds = Assert.Single(world.GetAddon<PhysicsFrame>().Bounds.ToArray());
        Assert.Equal(-0.5f, bounds.Min.x);
        Assert.Equal(10.5f, bounds.Max.x);
    }

    [Fact]
    public void PipelineClampsFastSphereAtFirstImpact()
    {
        using var world = new World();
        var configuration = world.GetPhysicsConfiguration();
        configuration.FixedDeltaTime = 1f;
        configuration.Gravity = float3.zero;
        var sphere = world.GetPhysicsShapes().Add(new SphereShape(0.5f));
        world.CreateStaticBody(RigidTransform.Translate(new float3(5f, 0f, 0f)), sphere);
        var moving = world.CreateContinuousBody(
            RigidTransform.Identity,
            sphere,
            1f,
            new PhysicsVelocity(new float3(10f, 0f, 0f), float3.zero),
            damping: new PhysicsDamping(0f, 0f));
        using var stage = PhysicsPipeline.Default.CreateStage(world);

        stage.Tick();

        Assert.Equal(4f, moving.Get<RigidTransform>().Translation.x, 4);
        Assert.True(moving.Get<PhysicsVelocity>().Linear.x <= 0.01f);
    }
}
