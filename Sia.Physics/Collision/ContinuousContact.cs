namespace Sia.Physics;

internal static class ContinuousContact
{
    public static bool TryBuild(
        PhysicsShapes shapes,
        ref BodyState first,
        ref BodyState second,
        out float3 point,
        out float3 normal,
        out float penetration)
    {
        if ((!first.IsContinuous && !second.IsContinuous) ||
            first.Body.Shape.Type != ShapeType.Sphere ||
            second.Body.Shape.Type != ShapeType.Sphere)
        {
            point = default;
            normal = default;
            penetration = default;
            return false;
        }

        var firstSphere = shapes.GetSphere(first.Body.Shape);
        var secondSphere = shapes.GetSphere(second.Body.Shape);
        if (!TimeOfImpact.TrySphereSphere(
            first.PreviousPose.Translation,
            first.Pose.Translation,
            firstSphere.Radius,
            second.PreviousPose.Translation,
            second.Pose.Translation,
            secondSphere.Radius,
            out var fraction))
        {
            point = default;
            normal = default;
            penetration = default;
            return false;
        }

        ClampContinuousPose(ref first, fraction);
        ClampContinuousPose(ref second, fraction);
        normal = GeometryMath.NormalizeOr(
            second.Pose.Translation - first.Pose.Translation,
            new float3(1f, 0f, 0f));
        var pointA = first.Pose.Translation + normal * firstSphere.Radius;
        var pointB = second.Pose.Translation - normal * secondSphere.Radius;
        point = (pointA + pointB) * 0.5f;
        penetration = 0f;
        return true;
    }

    private static void ClampContinuousPose(ref BodyState body, float fraction)
    {
        if (body.IsContinuous && body.Body.IsDynamic)
        {
            body.Pose.Translation = math.lerp(
                body.PreviousPose.Translation,
                body.Pose.Translation,
                fraction);
        }
    }
}

