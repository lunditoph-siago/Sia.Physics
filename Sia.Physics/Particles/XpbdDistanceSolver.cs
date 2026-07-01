namespace Sia.Physics;

internal static class XpbdDistanceSolver
{
    public static void Solve(ParticleFrame frame, float deltaTime, int iterationCount)
    {
        var particles = frame.Particles;
        var constraints = frame.Constraints;
        var inverseDeltaTimeSquared = 1f / (deltaTime * deltaTime);
        for (var iteration = 0; iteration < iterationCount; iteration++)
        {
            foreach (ref var constraint in constraints)
            {
                ref var first = ref particles[constraint.First];
                ref var second = ref particles[constraint.Second];
                var separation = second.Position - first.Position;
                var length = math.length(separation);
                if (length <= 1e-10f)
                {
                    continue;
                }

                var alpha = constraint.Compliance * inverseDeltaTimeSquared;
                var denominator = first.InverseMass + second.InverseMass + alpha;
                if (denominator <= 1e-12f)
                {
                    continue;
                }

                var normal = separation / length;
                var deltaLambda = (-(length - constraint.RestLength) - alpha * constraint.Lambda) / denominator;
                constraint.Lambda += deltaLambda;
                first.Position -= normal * first.InverseMass * deltaLambda;
                second.Position += normal * second.InverseMass * deltaLambda;
            }
        }
    }
}

