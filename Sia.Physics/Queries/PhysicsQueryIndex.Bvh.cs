using System.Runtime.CompilerServices;

namespace Sia.Physics;

public sealed partial class PhysicsQueryIndex
{
    private const int k_BvhBinCount = 8;
    private const int k_BvhLeafCapacity = 4;
    private const float k_MaximumQualityGrowth = 1.45f;
    private float _rebuildQuality;

    public float BvhQuality { get; private set; }

    public int BvhRebuildCount { get; private set; }

    internal ReadOnlySpan<Aabb> NodeBounds => _nodeBounds.ReadOnlySpan;

    internal ReadOnlySpan<BvhNode> Nodes => _nodes.ReadOnlySpan;

    internal ReadOnlySpan<int> BodyIndices => _bodyIndices.ReadOnlySpan;

    internal void BuildBvh(ReadOnlySpan<Aabb> bounds)
    {
        if (bounds.IsEmpty) {
            _nodeBounds.Clear();
            _nodes.Clear();
            _bodyIndices.Clear();
            BvhQuality = 0f;
            _rebuildQuality = 0f;
            return;
        }

        if (_bodyIndices.Count != bounds.Length || _nodes.Count == 0) {
            RebuildBvh(bounds);
            return;
        }

        RefitBvh(bounds);
        BvhQuality = ComputeBvhQuality();
        if (BvhQuality > _rebuildQuality * k_MaximumQualityGrowth) {
            RebuildBvh(bounds);
        }
    }

    private void RebuildBvh(ReadOnlySpan<Aabb> bounds)
    {
        _nodeBounds.Clear();
        _nodes.Clear();
        _bodyIndices.Clear();
        _bodyIndices.EnsureCapacity(bounds.Length);
        for (var i = 0; i < bounds.Length; i++) {
            _bodyIndices.Add(i);
        }

        BuildBvhNode(bounds, 0, bounds.Length);
        BvhQuality = ComputeBvhQuality();
        _rebuildQuality = BvhQuality;
        BvhRebuildCount++;
    }

    private int BuildBvhNode(ReadOnlySpan<Aabb> bounds, int start, int count)
    {
        var nodeIndex = _nodes.Count;
        _nodes.Add();
        _nodeBounds.Add();

        var nodeBounds = bounds[_bodyIndices[start]];
        var centroidMinimum = nodeBounds.Center;
        var centroidMaximum = centroidMinimum;
        for (var i = 1; i < count; i++) {
            ref readonly var bodyBounds = ref bounds[_bodyIndices[start + i]];
            nodeBounds.Include(bodyBounds);
            centroidMinimum = math.min(centroidMinimum, bodyBounds.Center);
            centroidMaximum = math.max(centroidMaximum, bodyBounds.Center);
        }
        _nodeBounds[nodeIndex] = nodeBounds;

        if (count <= k_BvhLeafCapacity) {
            _nodes[nodeIndex] = new BvhNode(start, count);
            return nodeIndex;
        }

        var centroidExtents = centroidMaximum - centroidMinimum;
        var axis = LargestAxis(centroidExtents);
        var axisMinimum = GetAxis(centroidMinimum, axis);
        var axisExtent = GetAxis(centroidExtents, axis);
        var leftCount = axisExtent > 1e-6f
            ? PartitionBySah(bounds, start, count, axis, axisMinimum, axisExtent)
            : count / 2;
        if (leftCount < count / 4 || leftCount > count - count / 4) {
            leftCount = count / 2;
        }

        BuildBvhNode(bounds, start, leftCount);
        var rightChild = BuildBvhNode(bounds, start + leftCount, count - leftCount);
        _nodes[nodeIndex] = new BvhNode(rightChild, 0);
        return nodeIndex;
    }

    private int PartitionBySah(
        ReadOnlySpan<Aabb> bounds,
        int start,
        int count,
        int axis,
        float axisMinimum,
        float axisExtent)
    {
        Span<BvhBin> bins = stackalloc BvhBin[k_BvhBinCount];
        var scale = k_BvhBinCount / axisExtent;
        for (var i = 0; i < count; i++) {
            ref readonly var bodyBounds = ref bounds[_bodyIndices[start + i]];
            var binIndex = System.Math.Min(
                (int)((GetAxis(bodyBounds.Center, axis) - axisMinimum) * scale),
                k_BvhBinCount - 1);
            bins[binIndex].Include(bodyBounds);
        }

        Span<float> leftAreas = stackalloc float[k_BvhBinCount - 1];
        Span<int> leftCounts = stackalloc int[k_BvhBinCount - 1];
        var leftBounds = default(Aabb);
        var leftCount = 0;
        for (var i = 0; i < k_BvhBinCount - 1; i++) {
            IncludeBin(ref leftBounds, ref leftCount, bins[i]);
            leftAreas[i] = leftBounds.SurfaceArea;
            leftCounts[i] = leftCount;
        }

        var bestCost = float.PositiveInfinity;
        var bestSplit = 0;
        var rightBounds = default(Aabb);
        var rightCount = 0;
        for (var i = k_BvhBinCount - 1; i > 0; i--) {
            IncludeBin(ref rightBounds, ref rightCount, bins[i]);
            var cost = leftAreas[i - 1] * leftCounts[i - 1] + rightBounds.SurfaceArea * rightCount;
            if (cost < bestCost) {
                bestCost = cost;
                bestSplit = i - 1;
            }
        }

        var first = start;
        var last = start + count - 1;
        while (first <= last) {
            ref readonly var bodyBounds = ref bounds[_bodyIndices[first]];
            var binIndex = System.Math.Min(
                (int)((GetAxis(bodyBounds.Center, axis) - axisMinimum) * scale),
                k_BvhBinCount - 1);
            if (binIndex <= bestSplit) {
                first++;
            }
            else {
                (_bodyIndices[first], _bodyIndices[last]) = (_bodyIndices[last], _bodyIndices[first]);
                last--;
            }
        }
        return first - start;
    }

    private void RefitBvh(ReadOnlySpan<Aabb> bounds)
    {
        for (var nodeIndex = _nodes.Count - 1; nodeIndex >= 0; nodeIndex--) {
            ref readonly var node = ref _nodes[nodeIndex];
            if (!node.IsLeaf) {
                _nodeBounds[nodeIndex] = Aabb.Union(
                    _nodeBounds[nodeIndex + 1],
                    _nodeBounds[node.Index]);
                continue;
            }

            var leafBounds = bounds[_bodyIndices[node.Index]];
            for (var i = 1; i < node.Count; i++) {
                leafBounds.Include(bounds[_bodyIndices[node.Index + i]]);
            }
            _nodeBounds[nodeIndex] = leafBounds;
        }
    }

    private float ComputeBvhQuality()
    {
        var rootArea = _nodeBounds[0].SurfaceArea;
        if (!(rootArea > 0f)) {
            return 1f;
        }

        var totalArea = 0f;
        foreach (ref readonly var bounds in _nodeBounds.ReadOnlySpan) {
            totalArea += bounds.SurfaceArea;
        }
        return totalArea / rootArea;
    }

    private static void IncludeBin(ref Aabb bounds, ref int count, in BvhBin bin)
    {
        if (bin.Count == 0) {
            return;
        }

        if (count == 0) {
            bounds = bin.Bounds;
        }
        else {
            bounds.Include(bin.Bounds);
        }
        count += bin.Count;
    }

    private static int LargestAxis(float3 extents) =>
        extents.x >= extents.y && extents.x >= extents.z ? 0 : extents.y >= extents.z ? 1 : 2;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float GetAxis(float3 value, int axis) => axis switch {
        0 => value.x,
        1 => value.y,
        _ => value.z
    };

    private struct BvhBin
    {
        public Aabb Bounds;
        public int Count;

        public void Include(in Aabb bounds)
        {
            if (Count == 0) {
                Bounds = bounds;
            }
            else {
                Bounds.Include(bounds);
            }
            Count++;
        }
    }
}
