using System.Runtime.CompilerServices;

namespace Sia.Physics;

[SiaSystem]
[SiaAfter<SolveContactsSystem>]
[SiaAfter<SolveDistanceJointsSystem>]
public sealed class ExportPhysicsFrameSystem : SystemBase
{
    private PhysicsFrame _frame = null!;

    public ExportPhysicsFrameSystem() : base(Matchers.Any)
    {
    }

    public override void Initialize(World world)
    {
        _frame = world.AcquireAddon<PhysicsFrame>();
    }

    public override void Execute(WorldContext context, IEntityQuery query)
    {
        var bodies = _frame.Bodies;
        for (var i = 0; i < bodies.Length; i++)
        {
            var entity = _frame.Entities[i];
            entity.Get<RigidTransform>() = bodies[i].Pose;
            ref var velocity = ref entity.GetOrNullRef<PhysicsVelocity>();
            if (!Unsafe.IsNullRef(ref velocity))
            {
                velocity = bodies[i].Velocity;
            }
        }
    }
}
