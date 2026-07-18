namespace Sia.Physics;

internal static class SweepBodySort
{
    public static void Sort(
        Span<int> order,
        ReadOnlySpan<Aabb> bounds,
        SweepAxis axis,
        bool maximum)
    {
        for (var start = order.Length / 2 - 1; start >= 0; start--)
        {
            SiftDown(order, bounds, axis, maximum, start, order.Length);
        }

        for (var end = order.Length - 1; end > 0; end--)
        {
            (order[0], order[end]) = (order[end], order[0]);
            SiftDown(order, bounds, axis, maximum, 0, end);
        }
    }

    public static int InsertionSort(
        Span<int> order,
        ReadOnlySpan<Aabb> bounds,
        SweepAxis axis,
        bool maximum)
    {
        var swapCount = 0;
        for (var i = 1; i < order.Length; i++)
        {
            var bodyIndex = order[i];
            var destination = i;
            while (destination > 0 && Compare(bodyIndex, order[destination - 1], bounds, axis, maximum) < 0)
            {
                order[destination] = order[destination - 1];
                destination--;
                swapCount++;
            }
            order[destination] = bodyIndex;
        }
        return swapCount;
    }

    private static void SiftDown(
        Span<int> order,
        ReadOnlySpan<Aabb> bounds,
        SweepAxis axis,
        bool maximum,
        int root,
        int count)
    {
        while (true)
        {
            var child = root * 2 + 1;
            if (child >= count)
            {
                return;
            }

            if (child + 1 < count && Compare(order[child], order[child + 1], bounds, axis, maximum) < 0)
            {
                child++;
            }
            if (Compare(order[root], order[child], bounds, axis, maximum) >= 0)
            {
                return;
            }

            (order[root], order[child]) = (order[child], order[root]);
            root = child;
        }
    }

    private static int Compare(
        int left,
        int right,
        ReadOnlySpan<Aabb> bounds,
        SweepAxis axis,
        bool maximum)
    {
        var leftPoint = maximum ? bounds[left].Max : bounds[left].Min;
        var rightPoint = maximum ? bounds[right].Max : bounds[right].Min;
        var order = PhysicsQueryIndex.GetSweepValue(leftPoint, axis)
            .CompareTo(PhysicsQueryIndex.GetSweepValue(rightPoint, axis));
        return order != 0 ? order : left.CompareTo(right);
    }
}
