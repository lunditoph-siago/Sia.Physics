namespace Sia.Physics;

public sealed class SweepBroadphase : IAddon, IDisposable
{
    private readonly NativeList<SweepEndpoint> _endpoints = new(256);
    private readonly NativeList<int> _active = new(64);

    public void Build(PhysicsFrame frame)
    {
        _endpoints.Clear();
        _active.Clear();
        frame.ClearPairs();

        var bounds = frame.Bounds;
        for (var i = 0; i < bounds.Length; i++)
        {
            _endpoints.Add(new SweepEndpoint(bounds[i].Min.x, i, IsMaximum: false));
            _endpoints.Add(new SweepEndpoint(bounds[i].Max.x, i, IsMaximum: true));
        }
        _endpoints.Span.Sort();

        var bodies = frame.Bodies;
        foreach (ref readonly var endpoint in _endpoints.ReadOnlySpan)
        {
            if (!endpoint.IsMaximum)
            {
                var candidateBounds = bounds[endpoint.BodyIndex];
                for (var i = 0; i < _active.Count; i++)
                {
                    var other = _active[i];
                    if (!OverlapsYZ(candidateBounds, bounds[other]))
                    {
                        continue;
                    }

                    ref var candidateBody = ref bodies[endpoint.BodyIndex];
                    ref var otherBody = ref bodies[other];
                    if ((candidateBody.Body.MotionType == BodyMotionType.Static &&
                         otherBody.Body.MotionType == BodyMotionType.Static) ||
                        !CollisionFilter.Allows(candidateBody.Collider.Filter, otherBody.Collider.Filter))
                    {
                        continue;
                    }

                    var first = System.Math.Min(endpoint.BodyIndex, other);
                    var second = System.Math.Max(endpoint.BodyIndex, other);
                    frame.AddPair(new BodyPair(first, second));
                }
                _active.Add(endpoint.BodyIndex);
            }
            else
            {
                for (var i = 0; i < _active.Count; i++)
                {
                    if (_active[i] == endpoint.BodyIndex)
                    {
                        _active.RemoveAtSwapBack(i);
                        break;
                    }
                }
            }
        }
    }

    private static bool OverlapsYZ(in Aabb left, in Aabb right) =>
        left.Max.y >= right.Min.y && left.Min.y <= right.Max.y &&
        left.Max.z >= right.Min.z && left.Min.z <= right.Max.z;

    public void OnUninitialize(World world) => Dispose();

    public void Dispose()
    {
        _endpoints.Dispose();
        _active.Dispose();
    }
}

