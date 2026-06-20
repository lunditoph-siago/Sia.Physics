namespace Sia.Physics;

internal static class ConstraintBuilder
{
    public static void Build(PhysicsFrame frame, float deltaTime)
    {
        frame.ClearConstraints();
        var bodies = frame.Bodies;
        foreach (ref readonly var contact in frame.Contacts)
        {
            ref var first = ref bodies[contact.Pair.First];
            ref var second = ref bodies[contact.Pair.Second];
            var offsetA = contact.Point - first.Pose.Translation;
            var offsetB = contact.Point - second.Pose.Translation;
            var relativeVelocity = VelocityAt(second, offsetB) - VelocityAt(first, offsetA);
            var normalVelocity = math.dot(relativeVelocity, contact.Normal);
            var tangent = GeometryMath.NormalizeOr(
                relativeVelocity - contact.Normal * normalVelocity,
                Perpendicular(contact.Normal));
            var bias = -0.2f / deltaTime * MathF.Max(contact.Penetration - 0.005f, 0f);
            if (normalVelocity < -1f)
            {
                bias += contact.Restitution * normalVelocity;
            }

            frame.AddConstraint(new ContactConstraint
            {
                Pair = contact.Pair,
                Normal = contact.Normal,
                Tangent = tangent,
                OffsetA = offsetA,
                OffsetB = offsetB,
                NormalMass = EffectiveMass(first, second, offsetA, offsetB, contact.Normal),
                TangentMass = EffectiveMass(first, second, offsetA, offsetB, tangent),
                VelocityBias = bias,
                Friction = contact.Friction
            });
        }
    }

    private static float EffectiveMass(
        in BodyState first,
        in BodyState second,
        float3 offsetA,
        float3 offsetB,
        float3 axis)
    {
        var angularA = math.cross(InertiaMath.ApplyInverse(first, math.cross(offsetA, axis)), offsetA);
        var angularB = math.cross(InertiaMath.ApplyInverse(second, math.cross(offsetB, axis)), offsetB);
        var inverseMass = first.Body.InverseMass + second.Body.InverseMass + math.dot(angularA + angularB, axis);
        return inverseMass > 1e-8f ? 1f / inverseMass : 0f;
    }

    private static float3 VelocityAt(in BodyState body, float3 offset) =>
        body.Velocity.Linear + math.cross(body.Velocity.Angular, offset);

    private static float3 Perpendicular(float3 normal)
    {
        var axis = MathF.Abs(normal.x) < 0.577f
            ? new float3(1f, 0f, 0f)
            : new float3(0f, 1f, 0f);
        return math.normalize(math.cross(normal, axis));
    }
}

