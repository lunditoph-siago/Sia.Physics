using System.Runtime.CompilerServices;

namespace Sia.Physics;

public sealed partial class PhysicsQueryIndex
{
    internal SweepAxis ProjectionAxis { get; private set; }

    internal int SweepSortSwapCount { get; private set; }

    internal ReadOnlySpan<int> MinimumOrder => _minimumOrder.ReadOnlySpan;

    internal ReadOnlySpan<int> MaximumOrder => _maximumOrder.ReadOnlySpan;

    internal void BuildSweep(ReadOnlySpan<Aabb> bounds)
    {
        if (bounds.IsEmpty)
        {
            _minimumOrder.Clear();
            _maximumOrder.Clear();
            SweepSortSwapCount = 0;
            return;
        }

        var axis = SweepBroadphase.SelectAxis(bounds);
        if (_minimumOrder.Count != bounds.Length || ProjectionAxis != axis)
        {
            ProjectionAxis = axis;
            InitializeSweepOrder(_minimumOrder, bounds.Length);
            InitializeSweepOrder(_maximumOrder, bounds.Length);
            SweepBodySort.Sort(_minimumOrder.Span, bounds, axis, maximum: false);
            SweepBodySort.Sort(_maximumOrder.Span, bounds, axis, maximum: true);
            SweepSortSwapCount = 0;
            return;
        }

        SweepSortSwapCount = SweepBodySort.InsertionSort(
            _minimumOrder.Span,
            bounds,
            axis,
            maximum: false);
        SweepSortSwapCount += SweepBodySort.InsertionSort(
            _maximumOrder.Span,
            bounds,
            axis,
            maximum: true);
    }

    internal int UpperBoundMinimum(ReadOnlySpan<Aabb> bounds, float value)
    {
        var low = 0;
        var high = _minimumOrder.Count;
        while (low < high)
        {
            var middle = low + ((high - low) >> 1);
            if (GetSweepValue(bounds[_minimumOrder[middle]].Min, ProjectionAxis) <= value)
            {
                low = middle + 1;
            }
            else
            {
                high = middle;
            }
        }
        return low;
    }

    internal int LowerBoundMaximum(ReadOnlySpan<Aabb> bounds, float value)
    {
        var low = 0;
        var high = _maximumOrder.Count;
        while (low < high)
        {
            var middle = low + ((high - low) >> 1);
            if (GetSweepValue(bounds[_maximumOrder[middle]].Max, ProjectionAxis) < value)
            {
                low = middle + 1;
            }
            else
            {
                high = middle;
            }
        }
        return low;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float GetSweepValue(float3 value, SweepAxis axis) => axis switch
    {
        SweepAxis.X => value.x,
        SweepAxis.Y => value.y,
        _ => value.z
    };

    private static void InitializeSweepOrder(NativeList<int> order, int count)
    {
        order.Clear();
        order.EnsureCapacity(count);
        for (var i = 0; i < count; i++)
        {
            order.Add(i);
        }
    }
}
