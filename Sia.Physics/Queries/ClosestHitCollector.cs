namespace Sia.Physics;

public struct ClosestHitCollector : IQueryCollector
{
    public ClosestHitCollector(float maximumDistance)
    {
        MaximumDistance = maximumDistance;
    }

    public float MaximumDistance { get; private set; }

    public bool HasHit { get; private set; }

    public RaycastHit Hit { get; private set; }

    public bool AddHit(in RaycastHit hit)
    {
        if (hit.Distance > MaximumDistance)
        {
            return true;
        }

        MaximumDistance = hit.Distance;
        Hit = hit;
        HasHit = true;
        return true;
    }
}

