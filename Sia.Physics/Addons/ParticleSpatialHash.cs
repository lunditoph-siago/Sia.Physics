namespace Sia.Physics;

public sealed class ParticleSpatialHash : IAddon, IDisposable
{
    private readonly NativeList<SpatialHashEntry> _entries = new(512);

    internal ReadOnlySpan<SpatialHashEntry> Entries => _entries.ReadOnlySpan;

    internal float CellSize { get; private set; } = 1f;

    internal void Build(ParticleFrame frame)
    {
        _entries.Clear();
        var particles = frame.Particles;
        var maximumRadius = 0.5f;
        foreach (ref readonly var particle in particles)
        {
            maximumRadius = MathF.Max(maximumRadius, particle.Radius);
        }
        CellSize = maximumRadius * 2f;

        for (var i = 0; i < particles.Length; i++)
        {
            _entries.Add(new SpatialHashEntry(Hash(CellOf(particles[i].Position)), i));
        }
        _entries.Span.Sort();
    }

    internal int3 CellOf(float3 position) => new(
        (int)MathF.Floor(position.x / CellSize),
        (int)MathF.Floor(position.y / CellSize),
        (int)MathF.Floor(position.z / CellSize));

    internal static ulong Hash(int3 cell)
    {
        var x = unchecked((uint)cell.x) * 73856093u;
        var y = unchecked((uint)cell.y) * 19349663u;
        var z = unchecked((uint)cell.z) * 83492791u;
        return ((ulong)(x ^ y) << 32) | z;
    }

    internal void FindRange(ulong key, out int start, out int end)
    {
        var entries = _entries.ReadOnlySpan;
        var low = 0;
        var high = entries.Length;
        while (low < high)
        {
            var middle = low + ((high - low) >> 1);
            if (entries[middle].Key < key)
            {
                low = middle + 1;
            }
            else
            {
                high = middle;
            }
        }
        start = low;
        while (low < entries.Length && entries[low].Key == key)
        {
            low++;
        }
        end = low;
    }

    public void OnUninitialize(World world) => Dispose();

    public void Dispose() => _entries.Dispose();
}

