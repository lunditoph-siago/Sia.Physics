namespace Sia.Physics;

public sealed partial class PhysicsQueryIndex : IDisposable
{
    private readonly NativeList<Aabb> _nodeBounds = new(256);
    private readonly NativeList<BvhNode> _nodes = new(256);
    private readonly NativeList<int> _bodyIndices = new(256);
    private readonly NativeList<int> _minimumOrder = new(256);
    private readonly NativeList<int> _maximumOrder = new(256);
    private bool _isCurrent;

    public int BodyCount => _bodyIndices.Count;

    public int NodeCount => _nodes.Count;

    internal bool IsCurrentFor(int bodyCount) => _isCurrent && BodyCount == bodyCount;

    internal void Invalidate() => _isCurrent = false;

    internal void Build(PhysicsFrame frame)
    {
        BuildBvh(frame.Bounds);
        BuildSweep(frame.Bounds);
        _isCurrent = true;
    }

    public void Dispose()
    {
        _nodeBounds.Dispose();
        _nodes.Dispose();
        _bodyIndices.Dispose();
        _minimumOrder.Dispose();
        _maximumOrder.Dispose();
    }
}
