namespace Sia.Physics;

public sealed class ShapeBatch<T> : IDisposable where T : unmanaged
{
    private readonly ShapeType _type;
    private readonly NativeList<T> _shapes;

    public ShapeBatch(ShapeType type, int capacity = 16)
    {
        _type = type;
        _shapes = new NativeList<T>(capacity);
    }

    public int Count => _shapes.Count;

    public ShapeHandle Add(in T shape)
    {
        var index = _shapes.Count;
        _shapes.Add(shape);
        return new ShapeHandle(_type, index, 1);
    }

    public ref readonly T Get(ShapeHandle handle)
    {
        if (handle.Type != _type || handle.Generation != 1 || (uint)handle.Index >= (uint)_shapes.Count)
        {
            throw new ArgumentException("Shape handle does not belong to this batch.", nameof(handle));
        }

        return ref _shapes[handle.Index];
    }

    public void Dispose() => _shapes.Dispose();
}

