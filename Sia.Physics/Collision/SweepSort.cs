namespace Sia.Physics;

internal static class SweepSort
{
    public static int InsertionSort(Span<SweepEndpoint> endpoints)
    {
        var swapCount = 0;
        for (var i = 1; i < endpoints.Length; i++)
        {
            var current = endpoints[i];
            var destination = i;
            while (destination > 0 && current.CompareTo(endpoints[destination - 1]) < 0)
            {
                endpoints[destination] = endpoints[destination - 1];
                destination--;
                swapCount++;
            }
            endpoints[destination] = current;
        }
        return swapCount;
    }
}

