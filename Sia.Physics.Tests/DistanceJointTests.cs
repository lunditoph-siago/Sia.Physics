namespace Sia.Physics.Tests;

public sealed class DistanceJointTests
{
    [Fact]
    public void BuildSystemMapsEcsBodyIdsToFrameIndices()
    {
        using var world = new World();
        var sphere = world.GetPhysicsShapes().Add(new SphereShape(0.5f));
        var first = world.CreateStaticBody(RigidTransform.Identity, sphere);
        var second = world.CreateDynamicBody(
            RigidTransform.Translate(new float3(2f, 0f, 0f)),
            sphere,
            1f);
        world.CreateDistanceJoint(first, second, 1f);
        using var stage = SystemChain.Empty
            .Add<BuildDistanceJointsSystem>()
            .Add<BuildPhysicsFrameSystem>()
            .CreateStage(world);

        stage.Tick();

        var joint = Assert.Single(world.GetAddon<PhysicsFrame>().DistanceJoints.ToArray());
        Assert.Equal(0, joint.First);
        Assert.Equal(1, joint.Second);
        Assert.Equal(1f, joint.RestLength);
    }

    [Fact]
    public void PipelinePullsSeparatedBodyTowardRestLength()
    {
        using var world = new World();
        world.GetPhysicsConfiguration().Gravity = float3.zero;
        var sphere = world.GetPhysicsShapes().Add(new SphereShape(0.25f));
        var first = world.CreateStaticBody(RigidTransform.Identity, sphere);
        var second = world.CreateDynamicBody(
            RigidTransform.Translate(new float3(2f, 0f, 0f)),
            sphere,
            1f,
            damping: new PhysicsDamping(0f, 0f));
        world.CreateDistanceJoint(first, second, 1f);
        using var stage = PhysicsPipeline.Default.CreateStage(world);

        stage.Tick();

        Assert.True(second.Get<PhysicsVelocity>().Linear.x < 0f);
    }
}
