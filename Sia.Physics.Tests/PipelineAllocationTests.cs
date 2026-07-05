namespace Sia.Physics.Tests;

public sealed class PipelineAllocationTests
{
    [Fact]
    public void RigidPipelineKeepsSteadyStateGatherAllocationsBounded()
    {
        using var world = new World();
        world.GetPhysicsConfiguration().Gravity = float3.zero;
        var sphere = world.GetPhysicsShapes().Add(new SphereShape(0.4f));
        for (var i = 0; i < 256; i++)
        {
            world.CreateDynamicBody(
                RigidTransform.Translate(new float3(i * 2f, 0f, 0f)),
                sphere,
                1f,
                damping: new PhysicsDamping(0f, 0f));
        }
        using var stage = PhysicsPipeline.Default.CreateStage(world);
        for (var i = 0; i < 16; i++)
        {
            stage.Tick();
        }

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 16; i++)
        {
            stage.Tick();
        }
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.InRange(allocated, 0, 16 * 1024);
    }
}
