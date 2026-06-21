namespace Sia.Physics;

public interface IQueryCollector
{
    float MaximumDistance { get; }

    bool AddHit(in RaycastHit hit);
}

