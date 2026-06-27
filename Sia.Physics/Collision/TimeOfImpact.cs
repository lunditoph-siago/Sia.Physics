namespace Sia.Physics;

public static class TimeOfImpact
{
    public static bool TrySphereSphere(
        float3 startA,
        float3 endA,
        float radiusA,
        float3 startB,
        float3 endB,
        float radiusB,
        out float fraction)
    {
        var separation = startB - startA;
        var relativeMotion = (endB - startB) - (endA - startA);
        var radius = radiusA + radiusB;
        var c = math.lengthsq(separation) - radius * radius;
        if (c <= 0f)
        {
            fraction = 0f;
            return true;
        }

        var a = math.lengthsq(relativeMotion);
        var b = math.dot(separation, relativeMotion);
        var discriminant = b * b - a * c;
        if (a <= 1e-16f || b >= 0f || discriminant < 0f)
        {
            fraction = default;
            return false;
        }

        fraction = (-b - MathF.Sqrt(discriminant)) / a;
        return fraction is >= 0f and <= 1f;
    }
}

