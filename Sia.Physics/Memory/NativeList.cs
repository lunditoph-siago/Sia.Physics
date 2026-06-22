using System.Numerics;

namespace Sia.Physics;

public sealed class NativeList<T> : IDisposable where T : unmanaged
{
    private NativeBuffer<T> _buffer;

    public NativeList(int capacity = 16)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        _buffer = new NativeBuffer<T>(capacity);
    }

    public int Count { get; private set; }

    public int Capacity => _buffer.Length;

    public Span<T> Span => _buffer.Span[..Count];

    public ReadOnlySpan<T> ReadOnlySpan => _buffer.Span[..Count];

    public ref T this[int index] => ref Span[index];

    public ref T Add()
    {
        EnsureCapacity(Count + 1);
        return ref _buffer[Count++];
    }

    public void Add(in T value) => Add() = value;

    public int AddRange(ReadOnlySpan<T> values)
    {
        var start = Count;
        EnsureCapacity(Count + values.Length);
        values.CopyTo(_buffer.Span[Count..]);
        Count += values.Length;
        return start;
    }

    public void RemoveAtSwapBack(int index)
    {
        if ((uint)index >= (uint)Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        var last = --Count;
        if (index != last)
        {
            _buffer[index] = _buffer[last];
        }
    }

    public void Clear(bool clearMemory = false)
    {
        if (clearMemory)
        {
            Span.Clear();
        }

        Count = 0;
    }

    public void EnsureCapacity(int capacity)
    {
        if (capacity <= Capacity)
        {
            return;
        }

        var newCapacity = (int)BitOperations.RoundUpToPowerOf2((uint)capacity);
        var replacement = new NativeBuffer<T>(newCapacity, clear: false);
        _buffer.Span[..Count].CopyTo(replacement.Span);
        _buffer.Dispose();
        _buffer = replacement;
    }

    public void Dispose() => _buffer.Dispose();
}
