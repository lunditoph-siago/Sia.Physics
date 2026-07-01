namespace Sia.Physics;

internal readonly record struct SpatialHashEntry(ulong Key, int ParticleIndex) : IComparable<SpatialHashEntry>
{
    public int CompareTo(SpatialHashEntry other)
    {
        var keyOrder = Key.CompareTo(other.Key);
        return keyOrder != 0 ? keyOrder : ParticleIndex.CompareTo(other.ParticleIndex);
    }
}

