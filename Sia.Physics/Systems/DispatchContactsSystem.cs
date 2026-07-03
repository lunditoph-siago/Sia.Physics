namespace Sia.Physics;

[SiaSystem]
[SiaAfter<NarrowphaseSystem>]
public sealed class DispatchContactsSystem : SystemBase
{
    private World _world = null!;
    private PhysicsFrame _frame = null!;

    public DispatchContactsSystem() : base(Matchers.Any)
    {
    }

    public override void Initialize(World world)
    {
        _world = world;
        _frame = world.AcquireAddon<PhysicsFrame>();
    }

    public override void Execute(WorldContext context, IEntityQuery query)
    {
        foreach (ref readonly var contact in _frame.Contacts)
        {
            var first = _frame.Entities[contact.Pair.First];
            var second = _frame.Entities[contact.Pair.Second];
            _world.Dispatcher.Send(first, new PhysicsContactEvent(
                second,
                contact.Point,
                contact.Normal,
                contact.Penetration));
            _world.Dispatcher.Send(second, new PhysicsContactEvent(
                first,
                contact.Point,
                -contact.Normal,
                contact.Penetration));
        }
    }
}

