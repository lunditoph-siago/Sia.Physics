namespace Sia.Physics;

internal static class SequentialImpulseSolver
{
    public static void Solve(PhysicsFrame frame, int iterationCount)
    {
        var constraints = frame.Constraints;
        var bodies = frame.Bodies;
        for (var iteration = 0; iteration < iterationCount; iteration++)
        {
            foreach (ref var constraint in constraints)
            {
                ref var first = ref bodies[constraint.Pair.First];
                ref var second = ref bodies[constraint.Pair.Second];
                var relativeVelocity = VelocityAt(second, constraint.OffsetB) - VelocityAt(first, constraint.OffsetA);
                var normalImpulse = -constraint.NormalMass *
                    (math.dot(relativeVelocity, constraint.Normal) + constraint.VelocityBias);
                var previousNormalImpulse = constraint.AccumulatedNormalImpulse;
                constraint.AccumulatedNormalImpulse = MathF.Max(previousNormalImpulse + normalImpulse, 0f);
                normalImpulse = constraint.AccumulatedNormalImpulse - previousNormalImpulse;
                ApplyPairImpulse(ref first, ref second, constraint, constraint.Normal * normalImpulse);

                relativeVelocity = VelocityAt(second, constraint.OffsetB) - VelocityAt(first, constraint.OffsetA);
                var tangentImpulse = -constraint.TangentMass * math.dot(relativeVelocity, constraint.Tangent);
                var maximumFriction = constraint.Friction * constraint.AccumulatedNormalImpulse;
                var previousTangentImpulse = constraint.AccumulatedTangentImpulse;
                constraint.AccumulatedTangentImpulse = System.Math.Clamp(
                    previousTangentImpulse + tangentImpulse,
                    -maximumFriction,
                    maximumFriction);
                tangentImpulse = constraint.AccumulatedTangentImpulse - previousTangentImpulse;
                ApplyPairImpulse(ref first, ref second, constraint, constraint.Tangent * tangentImpulse);
            }
        }
    }

    private static void ApplyPairImpulse(
        ref BodyState first,
        ref BodyState second,
        in ContactConstraint constraint,
        float3 impulse)
    {
        if (first.Body.IsDynamic)
        {
            first.Velocity.Linear -= impulse * first.Body.InverseMass;
            first.Velocity.Angular -= InertiaMath.ApplyInverse(first, math.cross(constraint.OffsetA, impulse));
        }
        if (second.Body.IsDynamic)
        {
            second.Velocity.Linear += impulse * second.Body.InverseMass;
            second.Velocity.Angular += InertiaMath.ApplyInverse(second, math.cross(constraint.OffsetB, impulse));
        }
    }

    private static float3 VelocityAt(in BodyState body, float3 offset) =>
        body.Velocity.Linear + math.cross(body.Velocity.Angular, offset);
}

