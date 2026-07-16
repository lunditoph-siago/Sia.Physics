namespace Sia.Physics;

public static class PhysicsQueries
{
    public static void Raycast<TCollector>(
        PhysicsFrame frame,
        PhysicsShapes shapes,
        in PhysicsRay ray,
        ref TCollector collector)
        where TCollector : struct, IQueryCollector
    {
        Raycast(frame, shapes, ray, PhysicsQueryFilter.All, ref collector);
    }

    public static void Raycast<TCollector>(
        PhysicsFrame frame,
        PhysicsShapes shapes,
        in PhysicsRay ray,
        in PhysicsQueryFilter filter,
        ref TCollector collector)
        where TCollector : struct, IQueryCollector
    {
        var index = frame.QueryIndex;
        if (!index.IsCurrentFor(frame.Bodies.Length) || index.NodeCount == 0)
        {
            RaycastLinear(frame, shapes, ray, filter, ref collector);
            return;
        }

        Span<BvhTraversalEntry> stack = stackalloc BvhTraversalEntry[128];
        if (!GeometryMath.IntersectRay(index.NodeBounds[0], ray, out var rootDistance))
        {
            return;
        }
        stack[0] = new BvhTraversalEntry(0, rootDistance);
        var stackCount = 1;
        while (stackCount != 0)
        {
            var entry = stack[--stackCount];
            if (entry.Distance > collector.MaximumDistance)
            {
                continue;
            }

            ref readonly var node = ref index.Nodes[entry.NodeIndex];
            if (node.IsLeaf)
            {
                for (var i = 0; i < node.Count; i++)
                {
                    var bodyIndex = index.BodyIndices[node.Index + i];
                    if (!RaycastBody(frame, shapes, ray, filter, bodyIndex, ref collector))
                    {
                        return;
                    }
                }
                continue;
            }

            var leftChild = entry.NodeIndex + 1;
            var rightChild = node.Index;
            var hitLeft = GeometryMath.IntersectRay(index.NodeBounds[leftChild], ray, out var leftDistance) &&
                leftDistance <= collector.MaximumDistance;
            var hitRight = GeometryMath.IntersectRay(index.NodeBounds[rightChild], ray, out var rightDistance) &&
                rightDistance <= collector.MaximumDistance;
            if (hitLeft && hitRight)
            {
                if (leftDistance <= rightDistance)
                {
                    stack[stackCount++] = new BvhTraversalEntry(rightChild, rightDistance);
                    stack[stackCount++] = new BvhTraversalEntry(leftChild, leftDistance);
                }
                else
                {
                    stack[stackCount++] = new BvhTraversalEntry(leftChild, leftDistance);
                    stack[stackCount++] = new BvhTraversalEntry(rightChild, rightDistance);
                }
            }
            else if (hitLeft)
            {
                stack[stackCount++] = new BvhTraversalEntry(leftChild, leftDistance);
            }
            else if (hitRight)
            {
                stack[stackCount++] = new BvhTraversalEntry(rightChild, rightDistance);
            }
        }
    }

    private static void RaycastLinear<TCollector>(
        PhysicsFrame frame,
        PhysicsShapes shapes,
        in PhysicsRay ray,
        in PhysicsQueryFilter filter,
        ref TCollector collector)
        where TCollector : struct, IQueryCollector
    {
        var bodies = frame.Bodies;
        var bounds = frame.Bounds;
        for (var i = 0; i < bodies.Length; i++)
        {
            if (!GeometryMath.IntersectRay(bounds[i], ray, out var broadphaseDistance) ||
                broadphaseDistance > collector.MaximumDistance)
            {
                continue;
            }

            if (!RaycastBody(frame, shapes, ray, filter, i, ref collector))
            {
                return;
            }
        }
    }

    private static bool RaycastBody<TCollector>(
        PhysicsFrame frame,
        PhysicsShapes shapes,
        in PhysicsRay ray,
        in PhysicsQueryFilter filter,
        int bodyIndex,
        ref TCollector collector)
        where TCollector : struct, IQueryCollector
    {
        ref var body = ref frame.Bodies[bodyIndex];
        if (!filter.Allows(body.Collider) ||
            !shapes.Raycast(body.Body.Shape, body.Pose, ray, out var distance, out var normal) ||
            distance > collector.MaximumDistance)
        {
            return true;
        }

        var hit = new RaycastHit(frame.Entities[bodyIndex], bodyIndex, distance, ray.GetPoint(distance), normal);
        return collector.AddHit(hit);
    }

    public static void OverlapAabb<TCollector>(
        PhysicsFrame frame,
        in Aabb bounds,
        ref TCollector collector)
        where TCollector : struct, IOverlapCollector
    {
        OverlapAabb(frame, bounds, PhysicsQueryFilter.All, ref collector);
    }

    public static void OverlapAabb<TCollector>(
        PhysicsFrame frame,
        in Aabb bounds,
        in PhysicsQueryFilter filter,
        ref TCollector collector)
        where TCollector : struct, IOverlapCollector
    {
        var bodies = frame.Bodies;
        var bodyBounds = frame.Bounds;
        for (var i = 0; i < bodies.Length; i++)
        {
            if (!filter.Allows(bodies[i].Collider) || !bounds.Overlaps(bodyBounds[i]))
            {
                continue;
            }

            var hit = new OverlapHit(frame.Entities[i], i);
            if (!collector.AddHit(hit))
            {
                return;
            }
        }
    }

    private readonly record struct BvhTraversalEntry(int NodeIndex, float Distance);
}
