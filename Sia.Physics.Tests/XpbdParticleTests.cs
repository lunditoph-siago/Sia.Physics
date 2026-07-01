namespace Sia.Physics.Tests;

public sealed class XpbdParticleTests
{
    [Fact]
    public void DistanceConstraintRestoresParticleSeparation()
    {
        using var world = new World();
        world.GetPhysicsConfiguration().Gravity = float3.zero;
        var first = world.CreateParticle(float3.zero, 0f, 0.1f);
        var second = world.CreateParticle(new float3(2f, 0f, 0f), 1f, 0.1f);
        world.CreateParticleDistanceConstraint(first, second, 1f);
        using var stage = ParticlePipeline.Default.CreateStage(world);

        stage.Tick();

        Assert.Equal(1f, second.Get<ParticlePosition>().Value.x, 4);
        Assert.True(second.Get<ParticleVelocity>().Value.x < 0f);
    }
}
