namespace Sia.Physics.Tests;

public sealed class NativeListTests
{
    [Fact]
    public void PreservesValuesAcrossGrowth()
    {
        using var values = new NativeList<int>(1);

        for (var i = 0; i < 129; i++)
        {
            values.Add(i * 3);
        }

        Assert.Equal(129, values.Count);
        Assert.True(values.Capacity >= values.Count);
        for (var i = 0; i < values.Count; i++)
        {
            Assert.Equal(i * 3, values[i]);
        }
    }
}

