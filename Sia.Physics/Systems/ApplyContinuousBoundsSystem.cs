namespace Sia.Physics;

[SiaSystem]
[SiaAfter<BuildPhysicsFrameSystem>]
public sealed class ApplyContinuousBoundsSystem : SystemBase
{
    private PhysicsFrame _frame = null!;
    private PhysicsShapes _shapes = null!;

    public ApplyContinuousBoundsSystem() : base(Matchers.Of<
        ContinuousCollision,
        PhysicsPreviousPose,
        PhysicsBody,
        RigidTransform>())
    {
    }

    public override void Initialize(World world)
    {
        _frame = world.AcquireAddon<PhysicsFrame>();
        _shapes = world.GetPhysicsShapes();
    }

    public override void Execute(WorldContext context, IEntityQuery query)
    {
        foreach (var entity in query)
        {
            if (!_frame.TryGetBodyIndex(entity.Id, out var bodyIndex))
            {
                continue;
            }
            ref readonly var body = ref entity.Get<PhysicsBody>();
            ref readonly var pose = ref entity.Get<RigidTransform>();
            var previousPose = entity.Get<PhysicsPreviousPose>().Value;
            var sweptBounds = Aabb.Union(
                _shapes.ComputeBounds(body.Shape, pose),
                _shapes.ComputeBounds(body.Shape, previousPose));
            _frame.MarkContinuous(bodyIndex, previousPose, sweptBounds);
        }
    }
}
