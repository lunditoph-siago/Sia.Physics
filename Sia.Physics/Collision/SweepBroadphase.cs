namespace Sia.Physics;

public sealed class SweepBroadphase : IAddon, IDisposable
{
    internal SweepAxis ProjectionAxis { get; private set; }

    internal int LastSortSwapCount { get; private set; }

    public void Build(PhysicsFrame frame)
    {
        frame.ClearPairs();
        var index = frame.QueryIndex;
        if (!index.IsCurrentFor(frame.Bodies.Length))
        {
            index.Build(frame);
        }
        var bounds = frame.Bounds;
        ProjectionAxis = index.ProjectionAxis;
        LastSortSwapCount = index.SweepSortSwapCount;
        var bodies = frame.Bodies;
        var order = index.MinimumOrder;
        for (var orderIndex = 0; orderIndex < order.Length; orderIndex++)
        {
            var candidateIndex = order[orderIndex];
            ref readonly var candidateBounds = ref bounds[candidateIndex];
            var candidateMaximum = PhysicsQueryIndex.GetSweepValue(candidateBounds.Max, ProjectionAxis);
            for (var otherOrderIndex = orderIndex + 1; otherOrderIndex < order.Length; otherOrderIndex++)
            {
                var otherIndex = order[otherOrderIndex];
                ref readonly var otherBounds = ref bounds[otherIndex];
                if (PhysicsQueryIndex.GetSweepValue(otherBounds.Min, ProjectionAxis) > candidateMaximum)
                {
                    break;
                }
                if (!OverlapsOtherAxes(candidateBounds, otherBounds))
                {
                    continue;
                }

                ref var candidateBody = ref bodies[candidateIndex];
                ref var otherBody = ref bodies[otherIndex];
                if ((candidateBody.Body.MotionType == BodyMotionType.Static &&
                     otherBody.Body.MotionType == BodyMotionType.Static) ||
                    !CollisionFilter.Allows(candidateBody.Collider.Filter, otherBody.Collider.Filter))
                {
                    continue;
                }

                var first = System.Math.Min(candidateIndex, otherIndex);
                var second = System.Math.Max(candidateIndex, otherIndex);
                frame.AddPair(new BodyPair(first, second));
            }
        }
    }

    private bool OverlapsOtherAxes(in Aabb left, in Aabb right) => ProjectionAxis switch
    {
        SweepAxis.X => Overlaps(left.Min.yz, left.Max.yz, right.Min.yz, right.Max.yz),
        SweepAxis.Y => Overlaps(left.Min.xz, left.Max.xz, right.Min.xz, right.Max.xz),
        _ => Overlaps(left.Min.xy, left.Max.xy, right.Min.xy, right.Max.xy)
    };

    private static bool Overlaps(float2 leftMin, float2 leftMax, float2 rightMin, float2 rightMax) =>
        math.all(leftMax >= rightMin & leftMin <= rightMax);

    internal static SweepAxis SelectAxis(ReadOnlySpan<Aabb> bounds)
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
    }
}
