using System.Runtime.InteropServices;

namespace Sia.Physics;

public sealed unsafe class NativeBuffer<T> : IDisposable where T : unmanaged
{
    private const nuint Alignment = 64;
    private T* _pointer;

    public NativeBuffer(int length, bool clear = true)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        Length = length;
        if (length == 0)
        {
            return;
        }

        var byteCount = checked((nuint)length * (nuint)sizeof(T));
        _pointer = (T*)NativeMemory.AlignedAlloc(byteCount, Alignment);
        if (_pointer is null)
        {
            throw new OutOfMemoryException();
        }

        if (clear)
        {
            NativeMemory.Clear(_pointer, byteCount);
        }
    }

    public int Length { get; }

    public bool IsDisposed => _pointer is null && Length != 0;

    public Span<T> Span
    {
        get
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return new Span<T>(_pointer, Length);
        }
    }

    public ref T this[int index] => ref Span[index];

    public void Dispose()
    {
        if (_pointer is null)
        {
            return;
        }

        NativeMemory.AlignedFree(_pointer);
        _pointer = null;
        GC.SuppressFinalize(this);
    }

    ~NativeBuffer() => Dispose();
}

