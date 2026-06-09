namespace Sia.Physics;

public readonly record struct ComputePolicy
{
    public ComputePolicy(int workerCount, int minimumBatchSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(workerCount, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(minimumBatchSize, 1);
        WorkerCount = workerCount;
        MinimumBatchSize = minimumBatchSize;
    }

    public static ComputePolicy Sequential => new(1, int.MaxValue);

    public static ComputePolicy Default => new(Environment.ProcessorCount, 256);

    public int WorkerCount { get; }

    public int MinimumBatchSize { get; }
}
