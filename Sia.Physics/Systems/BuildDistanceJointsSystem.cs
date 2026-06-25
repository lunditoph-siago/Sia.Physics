namespace Sia.Physics;

[SiaSystem]
[SiaAfter<BuildPhysicsFrameSystem>]
public sealed class BuildDistanceJointsSystem : SystemBase
{
    private PhysicsFrame _frame = null!;

    public BuildDistanceJointsSystem() : base(Matchers.Of<DistanceJoint>())
    {
    }

    public override void Initialize(World world)
    {
        _frame = world.AcquireAddon<PhysicsFrame>();
    }

    public override void Execute(WorldContext context, IEntityQuery query)
    {
        foreach (var entity in query)
        {
            ref readonly var joint = ref entity.Get<DistanceJoint>();
            if (!_frame.TryGetBodyIndex(joint.First, out var first) ||
                !_frame.TryGetBodyIndex(joint.Second, out var second))
            {
                continue;
            }

            _frame.AddDistanceJoint(new DistanceJointConstraint
            {
                First = first,
                Second = second,
                RestLength = joint.RestLength,
                LocalAnchorA = joint.LocalAnchorA,
                LocalAnchorB = joint.LocalAnchorB,
                Compliance = joint.Compliance
            });
        }
    }
}

