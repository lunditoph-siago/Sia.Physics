namespace Sia.Physics;

[SiaSystem]
public sealed class IntegrateParticlesSystem : ParallelSystemBase<
    ParticlePosition,
    ParticleVelocity,
    PhysicsParticle,
    ParticleDamping,
    ParticleGravityScale>
{
    private PhysicsConfiguration _configuration = null!;

    public override void Initialize(World world)
    {
        _configuration = world.GetPhysicsConfiguration();
    }

    protected override void HandleSlice(
        in WorldContext context,
        ref ParticlePosition position,
        ref ParticleVelocity velocity,
        ref PhysicsParticle particle,
        ref ParticleDamping damping,
        ref ParticleGravityScale gravityScale)
    {
        if (!particle.IsDynamic)
        {
            return;
        }

        var deltaTime = _configuration.FixedDeltaTime;
        velocity.Value += _configuration.Gravity * gravityScale.Value * deltaTime;
        velocity.Value *= MathF.Exp(-damping.Value * deltaTime);
        position.Value += velocity.Value * deltaTime;
    }
}

