namespace Sia.Physics;

public sealed class PhysicsFrame : IAddon, IDisposable
{
    private readonly List<Entity> _entities = [];
    private readonly NativeList<BodyState> _bodies = new(128);
    private readonly NativeList<Aabb> _bounds = new(128);
    private readonly NativeList<BodyPair> _pairs = new(256);
    private readonly NativeList<ContactManifold> _contacts = new(256);
    private readonly NativeList<ContactConstraint> _constraints = new(256);
    private readonly NativeList<DistanceJointConstraint> _distanceJoints = new(64);

    public IReadOnlyList<Entity> Entities => _entities;

    public Span<BodyState> Bodies => _bodies.Span;

    public ReadOnlySpan<Aabb> Bounds => _bounds.ReadOnlySpan;

    public ReadOnlySpan<BodyPair> Pairs => _pairs.ReadOnlySpan;

    public ReadOnlySpan<ContactManifold> Contacts => _contacts.ReadOnlySpan;

    internal Span<ContactConstraint> Constraints => _constraints.Span;

    internal Span<DistanceJointConstraint> DistanceJoints => _distanceJoints.Span;

    internal void BeginBuild()
    {
        _entities.Clear();
        _bodies.Clear();
        _bounds.Clear();
        _pairs.Clear();
        _contacts.Clear();
        _constraints.Clear();
        _distanceJoints.Clear();
    }

    internal void Add(Entity entity, in BodyState body, in Aabb bounds)
    {
        _entities.Add(entity);
        _bodies.Add(body);
        _bounds.Add(bounds);
    }

    internal void AddPair(in BodyPair pair) => _pairs.Add(pair);

    internal void ClearPairs() => _pairs.Clear();

    internal void AddContact(in ContactManifold contact) => _contacts.Add(contact);

    internal void ClearContacts() => _contacts.Clear();

    internal void AddConstraint(in ContactConstraint constraint) => _constraints.Add(constraint);

    internal void ClearConstraints() => _constraints.Clear();

    internal void AddDistanceJoint(in DistanceJointConstraint constraint) => _distanceJoints.Add(constraint);

    internal bool TryGetBodyIndex(EntityId id, out int bodyIndex)
    {
        for (var i = 0; i < _entities.Count; i++)
        {
            if (_entities[i].Id == id)
            {
                bodyIndex = i;
                return true;
            }
        }

        bodyIndex = -1;
        return false;
    }

    public void OnUninitialize(World world) => Dispose();

    public void Dispose()
    {
        _bodies.Dispose();
        _bounds.Dispose();
        _pairs.Dispose();
        _contacts.Dispose();
        _constraints.Dispose();
        _distanceJoints.Dispose();
    }
}
