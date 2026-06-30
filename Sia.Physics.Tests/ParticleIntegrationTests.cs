namespace Sia.Physics.Tests;

public sealed class ParticleIntegrationTests
{
    [Fact]
    public void IntegratesParticlesThroughUnmanagedEcsSlices()
    {
        using var world = new World();
        var configuration = world.GetPhysicsConfiguration();
        configuration.FixedDeltaTime = 0.5f;
        configuration.Gravity = new float3(0f, -4f, 0f);
        var particle = world.CreateParticle(
            float3.zero,
            inverseMass: 1f,
            radius: 0.1f,
            velocity: new ParticleVelocity(new float3(2f, 0f, 0f)),
            damping: new ParticleDamping(0f));
        using var stage = SystemChain.Empty.Add<IntegrateParticlesSystem>().CreateStage(world);

        stage.Tick();

        Assert.Equal(new float3(1f, -1f, 0f), particle.Get<ParticlePosition>().Value);
        Assert.Equal(new float3(2f, -2f, 0f), particle.Get<ParticleVelocity>().Value);
        Assert.True(particle.Host.TryGetSequentialBytes(out _));
    }
}
