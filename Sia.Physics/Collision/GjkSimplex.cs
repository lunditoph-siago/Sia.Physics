namespace Sia.Physics;

internal struct GjkSimplex
{
    public SupportPoint A;
    public SupportPoint B;
    public SupportPoint C;
    public SupportPoint D;
    public int Count;

    public readonly SupportPoint this[int index] => index switch
    {
        0 => A,
        1 => B,
        2 => C,
        3 => D,
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };

    public void PushFront(in SupportPoint point)
    {
        D = C;
        C = B;
        B = A;
        A = point;
        Count = System.Math.Min(Count + 1, 4);
    }
}

