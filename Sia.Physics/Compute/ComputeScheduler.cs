using System.Runtime.CompilerServices;

namespace Sia.Physics;

public sealed class ComputeScheduler
{
    public ComputeScheduler(ComputePolicy policy)
    {
        Policy = policy;
    }

    public ComputePolicy Policy { get; }

    public void Execute<TKernel, TContext>(in TContext context, int count)
        where TKernel : IComputeKernel<TContext>
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (count == 0)
        {
            return;
        }

        var batchCount = System.Math.Min(Policy.WorkerCount, DivideRoundUp(count, Policy.MinimumBatchSize));
        if (batchCount == 1)
        {
            TKernel.Execute(context, new WorkRange(0, count));
            return;
        }

        var sharedContext = context;
        Parallel.For(0, batchCount, new ParallelOptions { MaxDegreeOfParallelism = Policy.WorkerCount }, batchIndex =>
        {
            var range = Partition(count, batchCount, batchIndex);
            TKernel.Execute(sharedContext, range);
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static WorkRange Partition(int count, int partitionCount, int partitionIndex)
    {
        var start = (int)((long)count * partitionIndex / partitionCount);
        var end = (int)((long)count * (partitionIndex + 1) / partitionCount);
        return new WorkRange(start, end - start);
    }

    private static int DivideRoundUp(int value, int divisor) => (value + divisor - 1) / divisor;
}
