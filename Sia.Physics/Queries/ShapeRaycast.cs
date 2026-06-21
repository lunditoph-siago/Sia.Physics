namespace Sia.Physics;

internal static class ShapeRaycast
{
    public static bool Cast(
        in SphereShape shape,
        in RigidTransform pose,
        in PhysicsRay ray,
        out float distance,
        out float3 normal)
    {
        var origin = math.transform(math.inverse(pose), ray.Origin);
        var direction = math.rotate(math.inverse(pose), ray.Direction);
        var projection = math.dot(origin, direction);
        var discriminant = projection * projection - (math.lengthsq(origin) - shape.Radius * shape.Radius);
        if (discriminant < 0f)
        {
            distance = default;
            normal = default;
            return false;
        }

        var root = MathF.Sqrt(discriminant);
        distance = -projection - root;
        if (distance < 0f)
        {
            distance = -projection + root;
        }
        if (distance < 0f || distance > ray.MaximumDistance)
        {
            normal = default;
            return false;
        }

        normal = math.rotate(pose, GeometryMath.NormalizeOr(origin + direction * distance, new float3(1f, 0f, 0f)));
        return true;
    }

    public static bool Cast(
        in BoxShape shape,
        in RigidTransform pose,
        in PhysicsRay ray,
        out float distance,
        out float3 normal)
    {
        var inverse = math.inverse(pose);
        var localRay = new PhysicsRay(
            math.transform(inverse, ray.Origin),
            math.rotate(inverse, ray.Direction),
            ray.MaximumDistance);
        if (!GeometryMath.IntersectRay(new Aabb(-shape.HalfExtents, shape.HalfExtents), localRay, out distance))
        {
            normal = default;
            return false;
        }

        var point = localRay.GetPoint(distance);
        var normalized = math.abs(point / shape.HalfExtents);
        var localNormal = normalized.x >= normalized.y && normalized.x >= normalized.z
            ? new float3(MathF.CopySign(1f, point.x), 0f, 0f)
            : normalized.y >= normalized.z
                ? new float3(0f, MathF.CopySign(1f, point.y), 0f)
                : new float3(0f, 0f, MathF.CopySign(1f, point.z));
        normal = math.rotate(pose, localNormal);
        return true;
    }

    public static bool Cast(
        in CapsuleShape shape,
        in RigidTransform pose,
        in PhysicsRay ray,
        out float distance,
        out float3 normal)
    {
        var inverse = math.inverse(pose);
        var origin = math.transform(inverse, ray.Origin);
        var direction = math.rotate(inverse, ray.Direction);
        distance = float.PositiveInfinity;
        var localNormal = float3.zero;

        var a = direction.x * direction.x + direction.z * direction.z;
        var b = origin.x * direction.x + origin.z * direction.z;
        var c = origin.x * origin.x + origin.z * origin.z - shape.Radius * shape.Radius;
        var discriminant = b * b - a * c;
        if (a > 1e-12f && discriminant >= 0f)
        {
            var cylinderDistance = (-b - MathF.Sqrt(discriminant)) / a;
            var y = origin.y + direction.y * cylinderDistance;
            if (cylinderDistance >= 0f && MathF.Abs(y) <= shape.HalfLength)
            {
                distance = cylinderDistance;
                var point = origin + direction * distance;
                localNormal = GeometryMath.NormalizeOr(new float3(point.x, 0f, point.z), new float3(1f, 0f, 0f));
            }
        }

        TestSphere(origin, direction, new float3(0f, shape.HalfLength, 0f), shape.Radius, ref distance, ref localNormal);
        TestSphere(origin, direction, new float3(0f, -shape.HalfLength, 0f), shape.Radius, ref distance, ref localNormal);
        if (!float.IsFinite(distance) || distance > ray.MaximumDistance)
        {
            normal = default;
            return false;
        }

        normal = math.rotate(pose, localNormal);
        return true;
    }

    private static void TestSphere(
        float3 origin,
        float3 direction,
        float3 center,
        float radius,
        ref float closestDistance,
        ref float3 normal)
    {
        var offset = origin - center;
        var projection = math.dot(offset, direction);
        var discriminant = projection * projection - (math.lengthsq(offset) - radius * radius);
        if (discriminant < 0f)
        {
            return;
        }

        var distance = -projection - MathF.Sqrt(discriminant);
        if (distance >= 0f && distance < closestDistance)
        {
            closestDistance = distance;
            normal = GeometryMath.NormalizeOr(offset + direction * distance, new float3(0f, 1f, 0f));
        }
    }
}

