namespace Sia.Physics;

[SiaSystem]
[SiaAfter<SolveParticlesSystem>]
[SiaAfter<SolveParticleCollisionsSystem>]
public sealed class ExportParticleFrameSystem : SystemBase
{
    private PhysicsConfiguration _configuration = null!;
    private ParticleFrame _frame = null!;

    public ExportParticleFrameSystem() : base(Matchers.Any)
    {
    }

    public override void Initialize(World world)
    {
        _configuration = world.GetPhysicsConfiguration();
        _frame = world.AcquireAddon<ParticleFrame>();
    }

    public override void Execute(WorldContext context, IEntityQuery query)
    {
        var particles = _frame.Particles;
        var inverseDeltaTime = 1f / _configuration.FixedDeltaTime;
        for (var i = 0; i < particles.Length; i++)
        {
            var entity = _frame.Entities[i];
            ref readonly var particle = ref particles[i];
            entity.Get<ParticlePosition>().Value = particle.Position;
            entity.Get<ParticleVelocity>().Value +=
                (particle.Position - particle.PositionBeforeSolve) * inverseDeltaTime;
        }
    }
}
