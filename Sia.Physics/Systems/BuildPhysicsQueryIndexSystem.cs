namespace Sia.Physics;

[SiaSystem]
[SiaAfter<BuildPhysicsFrameSystem>]
[SiaAfter<ApplyContinuousBoundsSystem>]
public sealed class BuildPhysicsQueryIndexSystem : SystemBase
{
    private PhysicsFrame _frame = null!;

    public BuildPhysicsQueryIndexSystem() : base(Matchers.Any)
    {
    }

    public override void Initialize(World world) =>
        _frame = world.AcquireAddon<PhysicsFrame>();

    public override void Execute(WorldContext context, IEntityQuery query) =>
        _frame.QueryIndex.Build(_frame);
}
