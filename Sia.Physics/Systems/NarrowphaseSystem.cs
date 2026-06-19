namespace Sia.Physics;

[SiaSystem]
[SiaAfter<BroadphaseSystem>]
public sealed class NarrowphaseSystem : SystemBase
{
    private PhysicsFrame _frame = null!;
    private PhysicsShapes _shapes = null!;

    public NarrowphaseSystem() : base(Matchers.Any)
    {
    }

    public override void Initialize(World world)
    {
        _frame = world.AcquireAddon<PhysicsFrame>();
        _shapes = world.GetPhysicsShapes();
    }

    public override void Execute(WorldContext context, IEntityQuery query) =>
        Narrowphase.Build(_frame, _shapes);
}

