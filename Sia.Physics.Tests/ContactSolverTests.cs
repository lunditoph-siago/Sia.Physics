namespace Sia.Physics.Tests;

public sealed class ContactSolverTests
{
    [Fact]
    public void ResolvesClosingNormalVelocityAndExportsToEcs()
    {
        using var world = new World();
        var configuration = world.GetPhysicsConfiguration();
        configuration.FixedDeltaTime = 1f / 60f;
        configuration.Gravity = float3.zero;
        var sphere = world.GetPhysicsShapes().Add(new SphereShape(1f));
        world.CreateStaticBody(RigidTransform.Identity, sphere);
        var dynamicBody = world.CreateDynamicBody(
            RigidTransform.Translate(new float3(1.5f, 0f, 0f)),
            sphere,
            1f,
            new PhysicsVelocity(new float3(-1f, 0f, 0f), float3.zero),
            damping: new PhysicsDamping(0f, 0f));

        using var stage = PhysicsPipeline.Default.CreateStage(world);
        stage.Tick();

        Assert.True(dynamicBody.Get<PhysicsVelocity>().Linear.x > 0f);
    }
}

