namespace Sia.Physics.Tests;

public sealed class ComputeSchedulerTests
{
    [Fact]
    public void ProducesIdenticalSequentialAndParallelResults()
    {
        var sequential = new int[4099];
        var parallel = new int[sequential.Length];

        new ComputeScheduler(ComputePolicy.Sequential)
            .Execute<FillKernel, FillContext>(new FillContext(sequential), sequential.Length);
        new ComputeScheduler(new ComputePolicy(4, 64))
            .Execute<FillKernel, FillContext>(new FillContext(parallel), parallel.Length);

        Assert.Equal(sequential, parallel);
    }

    [Theory]
    [InlineData(17, 4)]
    [InlineData(4, 17)]
    public void PartitionsCoverEveryItemExactlyOnce(int count, int partitionCount)
    {
        var covered = new int[count];
        for (var i = 0; i < partitionCount; i++)
        {
            var range = ComputeScheduler.Partition(count, partitionCount, i);
            for (var item = range.Start; item < range.End; item++)
            {
                covered[item]++;
            }
        }

        Assert.All(covered, value => Assert.Equal(1, value));
    }

    private readonly record struct FillContext(int[] Values);

    private readonly struct FillKernel : IComputeKernel<FillContext>
    {
        public static void Execute(in FillContext context, WorkRange range)
        {
            for (var i = range.Start; i < range.End; i++)
            {
                context.Values[i] = unchecked(i * 397) ^ 0x5a5a5a5a;
            }
        }
    }
}

