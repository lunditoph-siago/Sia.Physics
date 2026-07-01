namespace Sia.Physics;

public sealed class ParticleFrame : IAddon, IDisposable
{
    private readonly List<Entity> _entities = [];
    private readonly NativeList<ParticleState> _particles = new(256);
    private readonly NativeList<ParticleConstraintState> _constraints = new(512);

    internal IReadOnlyList<Entity> Entities => _entities;

    internal Span<ParticleState> Particles => _particles.Span;

    internal Span<ParticleConstraintState> Constraints => _constraints.Span;

    internal void BeginBuild()
    {
        _entities.Clear();
        _particles.Clear();
        _constraints.Clear();
    }

    internal void Add(Entity entity, in ParticleState particle)
    {
        _entities.Add(entity);
        _particles.Add(particle);
    }

    internal void AddConstraint(in ParticleConstraintState constraint) => _constraints.Add(constraint);

    internal bool TryGetParticleIndex(EntityId id, out int index)
    {
        for (var i = 0; i < _entities.Count; i++)
        {
            if (_entities[i].Id == id)
            {
                index = i;
                return true;
            }
        }
        index = -1;
        return false;
    }

    public void OnUninitialize(World world) => Dispose();

    public void Dispose()
    {
        _particles.Dispose();
        _constraints.Dispose();
    }
}

