namespace Sia.Physics;

internal static class PrimitiveContact
{
    public static bool TryBuild(
        PhysicsShapes shapes,
        in BodyState first,
        in BodyState second,
        out float3 point,
        out float3 normal,
        out float penetration)
    {
        if (first.Body.Shape.Type != ShapeType.Sphere || second.Body.Shape.Type != ShapeType.Sphere)
        {
            point = default;
            normal = default;
            penetration = default;
            return false;
        }

        var firstSphere = shapes.GetSphere(first.Body.Shape);
        var secondSphere = shapes.GetSphere(second.Body.Shape);
        var offset = second.Pose.Translation - first.Pose.Translation;
        var distanceSquared = math.lengthsq(offset);
        var distance = MathF.Sqrt(distanceSquared);
        normal = distanceSquared > 1e-20f ? offset / distance : new float3(1f, 0f, 0f);
        penetration = MathF.Max(0f, firstSphere.Radius + secondSphere.Radius - distance);
        var pointA = first.Pose.Translation + normal * firstSphere.Radius;
        var pointB = second.Pose.Translation - normal * secondSphere.Radius;
        point = (pointA + pointB) * 0.5f;
        return true;
    }
}

