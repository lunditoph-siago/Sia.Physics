using System.Runtime.CompilerServices;

namespace Sia.Physics;

[SiaSystem]
[SiaAfter<IntegratePhysicsSystem>]
public sealed class BuildPhysicsFrameSystem : SystemBase
{
    private PhysicsFrame _frame = null!;
    private PhysicsShapes _shapes = null!;

    public BuildPhysicsFrameSystem() : base(Matchers.Of<RigidTransform, PhysicsBody, PhysicsCollider>())
    {
    }

    public override void Initialize(World world)
    {
        _frame = world.AcquireAddon<PhysicsFrame>();
        _shapes = world.GetPhysicsShapes();
    }

    public override void Execute(WorldContext context, IEntityQuery query)
    {
        _frame.BeginBuild();
        foreach (var entity in query)
        {
            ref var pose = ref entity.Get<RigidTransform>();
            ref var body = ref entity.Get<PhysicsBody>();
            ref var collider = ref entity.Get<PhysicsCollider>();
            ref var velocity = ref entity.GetOrNullRef<PhysicsVelocity>();
            ref var previousPose = ref entity.GetOrNullRef<PhysicsPreviousPose>();
            ref var continuous = ref entity.GetOrNullRef<ContinuousCollision>();
            var priorPose = Unsafe.IsNullRef(ref previousPose) ? pose : previousPose.Value;
            var state = new BodyState(
                pose,
                Unsafe.IsNullRef(ref velocity) ? PhysicsVelocity.Zero : velocity,
                body,
                collider,
                priorPose,
                !Unsafe.IsNullRef(ref continuous));
            var bounds = _shapes.ComputeBounds(body.Shape, pose);
            if (state.IsContinuous)
            {
                bounds = Aabb.Union(bounds, _shapes.ComputeBounds(body.Shape, priorPose));
            }
            _frame.Add(entity, state, bounds);
        }
    }
}
