namespace Sia.Physics;

internal readonly record struct SweepEndpoint(float Value, int BodyIndex, bool IsMaximum)
    : IComparable<SweepEndpoint>
{
    public int CompareTo(SweepEndpoint other)
    {
        var valueOrder = Value.CompareTo(other.Value);
        if (valueOrder != 0)
        {
            return valueOrder;
        }

        var boundaryOrder = IsMaximum.CompareTo(other.IsMaximum);
        return boundaryOrder != 0 ? boundaryOrder : BodyIndex.CompareTo(other.BodyIndex);
    }
}

