namespace Sia.Physics;

[SiaSystem]
[SiaAfter<BuildPhysicsFrameSystem>]
[SiaAfter<ApplyContinuousBoundsSystem>]
public sealed class BroadphaseSystem : SystemBase
{
    private PhysicsFrame _frame = null!;
    private SweepBroadphase _broadphase = null!;

    public BroadphaseSystem() : base(Matchers.Any)
    {
    }

    public override void Initialize(World world)
    {
        _frame = world.AcquireAddon<PhysicsFrame>();
        _broadphase = world.AcquireAddon<SweepBroadphase>();
    }

    public override void Execute(WorldContext context, IEntityQuery query) =>
        _broadphase.Build(_frame);
}
