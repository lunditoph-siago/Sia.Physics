namespace Sia.Physics;

public interface IOverlapCollector
{
    bool AddHit(in OverlapHit hit);
}

