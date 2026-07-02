namespace Sia.Physics;

[SiaSystem]
[SiaAfter<SolveParticlesSystem>]
public sealed class SolveParticleCollisionsSystem : SystemBase
{
    private ParticleFrame _frame = null!;
    private ParticleSpatialHash _spatialHash = null!;

    public SolveParticleCollisionsSystem() : base(Matchers.Any)
    {
    }

    public override void Initialize(World world)
    {
        _frame = world.AcquireAddon<ParticleFrame>();
        _spatialHash = world.AcquireAddon<ParticleSpatialHash>();
    }

    public override void Execute(WorldContext context, IEntityQuery query)
    {
        ParticleCollisionSolver.Solve(_frame, _spatialHash);
    }
}

