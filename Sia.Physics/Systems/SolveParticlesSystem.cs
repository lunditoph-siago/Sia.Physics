namespace Sia.Physics;

[SiaSystem]
[SiaAfter<BuildParticleConstraintsSystem>]
public sealed class SolveParticlesSystem : SystemBase
{
    private PhysicsConfiguration _configuration = null!;
    private ParticleFrame _frame = null!;

    public SolveParticlesSystem() : base(Matchers.Any)
    {
    }

    public override void Initialize(World world)
    {
        _configuration = world.GetPhysicsConfiguration();
        _frame = world.AcquireAddon<ParticleFrame>();
    }

    public override void Execute(WorldContext context, IEntityQuery query)
    {
        XpbdDistanceSolver.Solve(_frame, _configuration.FixedDeltaTime, _configuration.SolverIterations);
    }
}

