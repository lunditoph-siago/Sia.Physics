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
            if (!Gjk.Intersects(shapes, first, second))
            {
                continue;
            }

            var normal = GeometryMath.NormalizeOr(
                second.Pose.Translation - first.Pose.Translation,
                new float3(1f, 0f, 0f));
            var support = ConvexSupport.Get(shapes, first, second, normal);
            var penetration = MathF.Max(0f, math.dot(support.Difference, normal));
            var firstMaterial = first.Collider.Material;
            var secondMaterial = second.Collider.Material;
            var frictionMode = Max(firstMaterial.FrictionCombine, secondMaterial.FrictionCombine);
            var restitutionMode = Max(firstMaterial.RestitutionCombine, secondMaterial.RestitutionCombine);
            frame.AddContact(new ContactManifold(
                pair,
                (support.PointA + support.PointB) * 0.5f,
                normal,
                penetration,
                PhysicsMaterial.Combine(firstMaterial.Friction, secondMaterial.Friction, frictionMode),
                PhysicsMaterial.Combine(firstMaterial.Restitution, secondMaterial.Restitution, restitutionMode)));
        }
    }

    private static MaterialCombineMode Max(MaterialCombineMode left, MaterialCombineMode right) =>
        (MaterialCombineMode)System.Math.Max((int)left, (int)right);
}

