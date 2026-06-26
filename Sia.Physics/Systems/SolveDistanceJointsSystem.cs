namespace Sia.Physics;

[SiaSystem]
[SiaAfter<BuildDistanceJointsSystem>]
[SiaAfter<SolveContactsSystem>]
public sealed class SolveDistanceJointsSystem : SystemBase
{
    private PhysicsConfiguration _configuration = null!;
    private PhysicsFrame _frame = null!;

    public SolveDistanceJointsSystem() : base(Matchers.Any)
    {
    }

    public override void Initialize(World world)
    {
        _configuration = world.GetPhysicsConfiguration();
        _frame = world.AcquireAddon<PhysicsFrame>();
    }

    public override void Execute(WorldContext context, IEntityQuery query)
    {
        DistanceJointSolver.Solve(
            _frame,
            _configuration.FixedDeltaTime,
            _configuration.SolverIterations);
    }
}

