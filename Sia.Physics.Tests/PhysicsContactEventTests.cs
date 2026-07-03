namespace Sia.Physics.Tests;

public sealed class PhysicsContactEventTests
{
    [Fact]
    public void PipelineDispatchesMirroredContactEvents()
    {
        using var world = new World();
        var sphere = world.GetPhysicsShapes().Add(new SphereShape(1f));
        var first = world.CreateStaticBody(RigidTransform.Identity, sphere);
        var second = world.CreateDynamicBody(
            RigidTransform.Translate(new float3(1.5f, 0f, 0f)),
            sphere,
            1f);
        var eventCount = 0;
        world.Dispatcher.Listen((Entity target, in PhysicsContactEvent contact) =>
        {
            eventCount++;
            Assert.Equal(target == first ? second : first, contact.Other);
            Assert.Equal(target == first ? 1f : -1f, contact.Normal.x, 4);
            return false;
        });
        using var stage = PhysicsPipeline.Default.CreateStage(world);

        stage.Tick();

        Assert.Equal(2, eventCount);
    }
}
