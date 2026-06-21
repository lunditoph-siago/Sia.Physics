namespace Sia.Physics;

public struct AnyHitCollector : IQueryCollector
{
    public AnyHitCollector(float maximumDistance)
    {
        MaximumDistance = maximumDistance;
    }

    public float MaximumDistance { get; }

    public bool HasHit { get; private set; }

    public RaycastHit Hit { get; private set; }

    public bool AddHit(in RaycastHit hit)
    {
        Hit = hit;
        HasHit = true;
        return false;
    }
}

