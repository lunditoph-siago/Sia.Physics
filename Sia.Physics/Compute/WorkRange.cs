namespace Sia.Physics;

public readonly record struct WorkRange
{
    public WorkRange(int start, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        Start = start;
        Count = count;
    }

    public int Start { get; }

    public int Count { get; }

    public int End => checked(Start + Count);

    public bool IsEmpty => Count == 0;
}
