namespace Sia.Physics;

public struct AnyOverlapCollector : IOverlapCollector
{
    public bool HasHit { get; private set; }

    public OverlapHit Hit { get; private set; }

    public bool AddHit(in OverlapHit hit)
    {
        Hit = hit;
        HasHit = true;
        return false;
    }
}

