namespace Sia.Physics.Tests;

public sealed class EcsIntegrationTests
{
    [Fact]
    public void IntegratesDynamicBodiesThroughUnmanagedEcsSlices()
    {
        using var world = new World();
        var configuration = world.GetPhysicsConfiguration();
        configuration.FixedDeltaTime = 0.5f;
        configuration.Gravity = new float3(0f, -4f, 0f);
        var sphere = world.GetPhysicsShapes().Add(new SphereShape(1f));
        var entity = world.CreateDynamicBody(
            RigidTransform.Identity,
            sphere,
            density: 1f,
            velocity: new PhysicsVelocity(new float3(2f, 0f, 0f), float3.zero),
            damping: new PhysicsDamping(0f, 0f));

        using var stage = SystemChain.Empty.Add<IntegratePhysicsSystem>().CreateStage(world);
        stage.Tick();

        var pose = entity.Get<RigidTransform>();
        var velocity = entity.Get<PhysicsVelocity>();
        Assert.Equal(new float3(1f, -1f, 0f), pose.Translation);
        Assert.Equal(new float3(2f, -2f, 0f), velocity.Linear);
        Assert.True(entity.Host.TryGetSequentialBytes(out var bytes));
        Assert.True(bytes.Length > 0);
    }
}
