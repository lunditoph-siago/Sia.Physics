namespace Sia.Physics.Tests;

public sealed class NativeBufferTests
{
    [Fact]
    public unsafe void AllocatesCacheLineAlignedStorage()
    {
        using var buffer = new NativeBuffer<int>(37);

        fixed (int* pointer = buffer.Span)
        {
            Assert.Equal(0UL, (ulong)pointer & 63UL);
        }
    }

    [Fact]
    public void ClearsStorageByDefault()
    {
        using var buffer = new NativeBuffer<int>(8);

        Assert.All(buffer.Span.ToArray(), value => Assert.Equal(0, value));
    }
}

