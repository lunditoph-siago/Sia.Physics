namespace Sia.Physics;

[SiaSystem]
[SiaAfter<NarrowphaseSystem>]
public sealed class SolveContactsSystem : SystemBase
{
    private PhysicsConfiguration _configuration = null!;
    private PhysicsFrame _frame = null!;

    public SolveContactsSystem() : base(Matchers.Any)
    {
    }

    public override void Initialize(World world)
    {
        _configuration = world.GetPhysicsConfiguration();
        _frame = world.AcquireAddon<PhysicsFrame>();
    }

    public override void Execute(WorldContext context, IEntityQuery query)
    {
        ConstraintBuilder.Build(_frame, _configuration.FixedDeltaTime);
        SequentialImpulseSolver.Solve(_frame, _configuration.SolverIterations);
    }
}

