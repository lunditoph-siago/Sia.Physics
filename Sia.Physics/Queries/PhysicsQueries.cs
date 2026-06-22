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
        var bodies = frame.Bodies;
        var bounds = frame.Bounds;
        for (var i = 0; i < bodies.Length; i++)
        {
            if (!filter.Allows(bodies[i].Collider) ||
                !GeometryMath.IntersectRay(bounds[i], ray, out var broadphaseDistance) ||
                broadphaseDistance > collector.MaximumDistance ||
                !shapes.Raycast(bodies[i].Body.Shape, bodies[i].Pose, ray, out var distance, out var normal) ||
                distance > collector.MaximumDistance)
            {
                continue;
            }

            var hit = new RaycastHit(frame.Entities[i], i, distance, ray.GetPoint(distance), normal);
            if (!collector.AddHit(hit))
            {
                return;
            }
        }
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
}
