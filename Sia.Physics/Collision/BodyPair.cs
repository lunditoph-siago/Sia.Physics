namespace Sia.Physics;

public readonly record struct BodyPair
{
    public BodyPair(int first, int second)
    {
        if (first < 0 || second <= first)
        {
            throw new ArgumentOutOfRangeException(nameof(first));
        }

        First = first;
        Second = second;
    }

    public int First { get; }

    public int Second { get; }
}
