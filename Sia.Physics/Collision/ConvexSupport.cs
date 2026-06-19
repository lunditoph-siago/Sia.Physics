namespace Sia.Physics;

internal static class ConvexSupport
{
    public static SupportPoint Get(
        PhysicsShapes shapes,
        in BodyState first,
        in BodyState second,
        float3 direction)
    {
        var firstLocalDirection = math.mul(math.conjugate(first.Pose.Rotation), direction);
        var secondLocalDirection = math.mul(math.conjugate(second.Pose.Rotation), -direction);
        var pointA = math.transform(first.Pose, shapes.Support(first.Body.Shape, firstLocalDirection));
        var pointB = math.transform(second.Pose, shapes.Support(second.Body.Shape, secondLocalDirection));
        return new SupportPoint(pointA - pointB, pointA, pointB);
    }
}

