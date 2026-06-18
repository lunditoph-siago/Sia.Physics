namespace Sia.Physics;

public sealed class PhysicsFrame : IAddon, IDisposable
{
    private readonly List<Entity> _entities = [];
    private readonly NativeList<BodyState> _bodies = new(128);
    private readonly NativeList<Aabb> _bounds = new(128);
    private readonly NativeList<BodyPair> _pairs = new(256);

    public IReadOnlyList<Entity> Entities => _entities;

    public Span<BodyState> Bodies => _bodies.Span;

    public ReadOnlySpan<Aabb> Bounds => _bounds.ReadOnlySpan;

    public ReadOnlySpan<BodyPair> Pairs => _pairs.ReadOnlySpan;

    internal void BeginBuild()
    {
        _entities.Clear();
        _bodies.Clear();
        _bounds.Clear();
        _pairs.Clear();
    }

    internal void Add(Entity entity, in BodyState body, in Aabb bounds)
    {
        _entities.Add(entity);
        _bodies.Add(body);
        _bounds.Add(bounds);
    }

    internal void AddPair(in BodyPair pair) => _pairs.Add(pair);

    internal void ClearPairs() => _pairs.Clear();

    public void OnUninitialize(World world) => Dispose();

    public void Dispose()
    {
        _bodies.Dispose();
        _bounds.Dispose();
        _pairs.Dispose();
    }
}
