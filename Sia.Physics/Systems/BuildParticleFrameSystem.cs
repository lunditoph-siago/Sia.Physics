namespace Sia.Physics;

[SiaSystem]
[SiaAfter<IntegrateParticlesSystem>]
public sealed class BuildParticleFrameSystem : SystemBase
{
    private ParticleFrame _frame = null!;

    public BuildParticleFrameSystem() : base(Matchers.Of<ParticlePosition, ParticleVelocity, PhysicsParticle>())
    {
    }

    public override void Initialize(World world)
    {
        _frame = world.AcquireAddon<ParticleFrame>();
    }

    public override void Execute(WorldContext context, IEntityQuery query)
    {
        _frame.BeginBuild();
        foreach (var entity in query)
        {
            var position = entity.Get<ParticlePosition>().Value;
            var particle = entity.Get<PhysicsParticle>();
            _frame.Add(entity, new ParticleState
            {
                Position = position,
                PositionBeforeSolve = position,
                Velocity = entity.Get<ParticleVelocity>().Value,
                InverseMass = particle.InverseMass,
                Radius = particle.Radius
            });
        }
    }
}

