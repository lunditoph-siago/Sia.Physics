namespace Sia.Physics;

internal static class DistanceJointSolver
{
    private const float BiasFactor = 0.2f;

    public static void Solve(PhysicsFrame frame, float deltaTime, int iterationCount)
    {
        var bodies = frame.Bodies;
        var constraints = frame.DistanceJoints;
        var inverseDeltaTime = 1f / deltaTime;
        for (var iteration = 0; iteration < iterationCount; iteration++)
        {
            foreach (ref var constraint in constraints)
            {
                ref var first = ref bodies[constraint.First];
                ref var second = ref bodies[constraint.Second];
                var offsetA = math.rotate(first.Pose, constraint.LocalAnchorA);
                var offsetB = math.rotate(second.Pose, constraint.LocalAnchorB);
                var anchorA = first.Pose.Translation + offsetA;
                var anchorB = second.Pose.Translation + offsetB;
                var separation = anchorB - anchorA;
                var length = math.length(separation);
                var normal = GeometryMath.NormalizeOr(separation, new float3(1f, 0f, 0f));
                var angularA = math.cross(offsetA, normal);
                var angularB = math.cross(offsetB, normal);
                var inverseMass = first.Body.InverseMass + second.Body.InverseMass +
                    math.dot(angularA, InertiaMath.ApplyInverse(first, angularA)) +
                    math.dot(angularB, InertiaMath.ApplyInverse(second, angularB));
                var softness = constraint.Compliance * inverseDeltaTime * inverseDeltaTime;
                if (inverseMass + softness <= 1e-12f)
                {
                    continue;
                }

                var velocityA = first.Velocity.Linear + math.cross(first.Velocity.Angular, offsetA);
                var velocityB = second.Velocity.Linear + math.cross(second.Velocity.Angular, offsetB);
                var relativeSpeed = math.dot(velocityB - velocityA, normal);
                var bias = (length - constraint.RestLength) * BiasFactor * inverseDeltaTime;
                var impulse = -(relativeSpeed + bias + softness * constraint.AccumulatedImpulse) /
                    (inverseMass + softness);
                constraint.AccumulatedImpulse += impulse;
                ApplyImpulse(ref first, -normal * impulse, offsetA);
                ApplyImpulse(ref second, normal * impulse, offsetB);
            }
        }
    }

    private static void ApplyImpulse(ref BodyState body, float3 impulse, float3 offset)
    {
        if (!body.Body.IsDynamic)
        {
            return;
        }

        body.Velocity.Linear += impulse * body.Body.InverseMass;
        body.Velocity.Angular += InertiaMath.ApplyInverse(body, math.cross(offset, impulse));
    }
}

