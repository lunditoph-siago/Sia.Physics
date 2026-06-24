namespace Sia.Physics;

public static class Narrowphase
{
    public static void Build(PhysicsFrame frame, PhysicsShapes shapes)
    {
        frame.ClearContacts();
        var bodies = frame.Bodies;
        foreach (ref readonly var pair in frame.Pairs)
        {
            ref var first = ref bodies[pair.First];
            ref var second = ref bodies[pair.Second];
            if (!Gjk.TryGetSimplex(shapes, first, second, out var simplex))
            {
                continue;
            }

            float3 point;
            float3 normal;
            float penetration;
            if (!PrimitiveContact.TryBuild(shapes, first, second, out point, out normal, out penetration) &&
                !Epa.TrySolve(shapes, first, second, simplex, out point, out normal, out penetration))
            {
                normal = GeometryMath.NormalizeOr(
                    second.Pose.Translation - first.Pose.Translation,
                    new float3(1f, 0f, 0f));
                var support = ConvexSupport.Get(shapes, first, second, normal);
                penetration = MathF.Max(0f, math.dot(support.Difference, normal));
                point = (support.PointA + support.PointB) * 0.5f;
            }
            var firstMaterial = first.Collider.Material;
            var secondMaterial = second.Collider.Material;
            var frictionMode = Max(firstMaterial.FrictionCombine, secondMaterial.FrictionCombine);
            var restitutionMode = Max(firstMaterial.RestitutionCombine, secondMaterial.RestitutionCombine);
            frame.AddContact(new ContactManifold(
                pair,
                point,
                normal,
                penetration,
                PhysicsMaterial.Combine(firstMaterial.Friction, secondMaterial.Friction, frictionMode),
                PhysicsMaterial.Combine(firstMaterial.Restitution, secondMaterial.Restitution, restitutionMode)));
        }
    }

    private static MaterialCombineMode Max(MaterialCombineMode left, MaterialCombineMode right) =>
        (MaterialCombineMode)System.Math.Max((int)left, (int)right);
}
