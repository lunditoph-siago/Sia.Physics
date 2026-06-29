namespace Sia.Physics;

public sealed class SweepBroadphase : IAddon, IDisposable
{
    private readonly NativeList<SweepEndpoint> _endpoints = new(256);
    private readonly NativeList<int> _active = new(64);

    internal SweepAxis ProjectionAxis { get; private set; }

    internal int LastSortSwapCount { get; private set; }

    public void Build(PhysicsFrame frame)
    {
        _active.Clear();
        frame.ClearPairs();

        var bounds = frame.Bounds;
        ProjectionAxis = SelectAxis(bounds);
        if (_endpoints.Count != bounds.Length * 2)
        {
            _endpoints.Clear();
            for (var i = 0; i < bounds.Length; i++)
            {
                _endpoints.Add(CreateEndpoint(bounds[i], i, isMaximum: false));
                _endpoints.Add(CreateEndpoint(bounds[i], i, isMaximum: true));
            }
            _endpoints.Span.Sort();
            LastSortSwapCount = 0;
        }
        else
        {
            foreach (ref var endpoint in _endpoints.Span)
            {
                endpoint = CreateEndpoint(bounds[endpoint.BodyIndex], endpoint.BodyIndex, endpoint.IsMaximum);
            }
            LastSortSwapCount = SweepSort.InsertionSort(_endpoints.Span);
        }

        var bodies = frame.Bodies;
        foreach (ref readonly var endpoint in _endpoints.ReadOnlySpan)
        {
            if (!endpoint.IsMaximum)
            {
                var candidateBounds = bounds[endpoint.BodyIndex];
                for (var i = 0; i < _active.Count; i++)
                {
                    var other = _active[i];
                    if (!OverlapsOtherAxes(candidateBounds, bounds[other]))
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

    private SweepEndpoint CreateEndpoint(in Aabb bounds, int bodyIndex, bool isMaximum)
    {
        var point = isMaximum ? bounds.Max : bounds.Min;
        var value = ProjectionAxis switch
        {
            SweepAxis.X => point.x,
            SweepAxis.Y => point.y,
            _ => point.z
        };
        return new SweepEndpoint(value, bodyIndex, isMaximum);
    }

    private bool OverlapsOtherAxes(in Aabb left, in Aabb right) => ProjectionAxis switch
    {
        SweepAxis.X => Overlaps(left.Min.yz, left.Max.yz, right.Min.yz, right.Max.yz),
        SweepAxis.Y => Overlaps(left.Min.xz, left.Max.xz, right.Min.xz, right.Max.xz),
        _ => Overlaps(left.Min.xy, left.Max.xy, right.Min.xy, right.Max.xy)
    };

    private static bool Overlaps(float2 leftMin, float2 leftMax, float2 rightMin, float2 rightMax) =>
        math.all(leftMax >= rightMin & leftMin <= rightMax);

    private static SweepAxis SelectAxis(ReadOnlySpan<Aabb> bounds)
    {
        if (bounds.IsEmpty)
        {
            return SweepAxis.X;
        }

        var mean = float3.zero;
        foreach (ref readonly var bound in bounds)
        {
            mean += bound.Center;
        }
        mean /= bounds.Length;

        var variance = float3.zero;
        foreach (ref readonly var bound in bounds)
        {
            var offset = bound.Center - mean;
            variance += offset * offset;
        }
        return variance.x >= variance.y && variance.x >= variance.z
            ? SweepAxis.X
            : variance.y >= variance.z ? SweepAxis.Y : SweepAxis.Z;
    }

    public void OnUninitialize(World world) => Dispose();

    public void Dispose()
    {
        _endpoints.Dispose();
        _active.Dispose();
    }
}
