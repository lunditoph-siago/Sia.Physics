namespace Sia.Physics;

internal static class Gjk
{
    public static bool Intersects(PhysicsShapes shapes, in BodyState first, in BodyState second) =>
        TryGetSimplex(shapes, first, second, out _);

    public static bool TryGetSimplex(
        PhysicsShapes shapes,
        in BodyState first,
        in BodyState second,
        out GjkSimplex simplex)
    {
        var direction = second.Pose.Translation - first.Pose.Translation;
        direction = GeometryMath.NormalizeOr(direction, new float3(1f, 0f, 0f));
        simplex = new GjkSimplex();
        simplex.PushFront(ConvexSupport.Get(shapes, first, second, direction));
        direction = -simplex.A.Difference;

        for (var iteration = 0; iteration < 24; iteration++)
        {
            if (math.lengthsq(direction) < 1e-16f)
            {
                return true;
            }

            var support = ConvexSupport.Get(shapes, first, second, direction);
            if (math.dot(support.Difference, direction) < 0f)
            {
                return false;
            }

            simplex.PushFront(support);
            if (ContainsOrigin(ref simplex, ref direction))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsOrigin(ref GjkSimplex simplex, ref float3 direction) => simplex.Count switch
    {
        2 => SolveLine(ref simplex, ref direction),
        3 => SolveTriangle(ref simplex, ref direction),
        4 => SolveTetrahedron(ref simplex, ref direction),
        _ => false
    };

    private static bool SolveLine(ref GjkSimplex simplex, ref float3 direction)
    {
        var a = simplex.A.Difference;
        var b = simplex.B.Difference;
        var ab = b - a;
        var ao = -a;
        if (math.dot(ab, ao) > 0f)
        {
            direction = TripleCross(ab, ao, ab);
        }
        else
        {
            simplex.Count = 1;
            direction = ao;
        }
        return false;
    }

    private static bool SolveTriangle(ref GjkSimplex simplex, ref float3 direction)
    {
        var a = simplex.A.Difference;
        var b = simplex.B.Difference;
        var c = simplex.C.Difference;
        var ab = b - a;
        var ac = c - a;
        var ao = -a;
        var normal = math.cross(ab, ac);

        if (math.dot(math.cross(normal, ac), ao) > 0f)
        {
            if (math.dot(ac, ao) > 0f)
            {
                simplex.B = simplex.C;
                simplex.Count = 2;
                direction = TripleCross(ac, ao, ac);
            }
            else
            {
                simplex.Count = 2;
                return SolveLine(ref simplex, ref direction);
            }
        }
        else if (math.dot(math.cross(ab, normal), ao) > 0f)
        {
            simplex.Count = 2;
            return SolveLine(ref simplex, ref direction);
        }
        else if (math.dot(normal, ao) > 0f)
        {
            direction = normal;
        }
        else
        {
            (simplex.B, simplex.C) = (simplex.C, simplex.B);
            direction = -normal;
        }
        return false;
    }

    private static bool SolveTetrahedron(ref GjkSimplex simplex, ref float3 direction)
    {
        var a = simplex.A.Difference;
        var ao = -a;
        var ab = simplex.B.Difference - a;
        var ac = simplex.C.Difference - a;
        var ad = simplex.D.Difference - a;

        var abc = math.cross(ab, ac);
        if (math.dot(abc, ao) > 0f)
        {
            simplex.Count = 3;
            direction = abc;
            return false;
        }

        var acd = math.cross(ac, ad);
        if (math.dot(acd, ao) > 0f)
        {
            simplex.B = simplex.C;
            simplex.C = simplex.D;
            simplex.Count = 3;
            direction = acd;
            return false;
        }

        var adb = math.cross(ad, ab);
        if (math.dot(adb, ao) > 0f)
        {
            simplex.C = simplex.B;
            simplex.B = simplex.D;
            simplex.Count = 3;
            direction = adb;
            return false;
        }

        return true;
    }

    private static float3 TripleCross(float3 left, float3 middle, float3 right)
    {
        var result = math.cross(math.cross(left, middle), right);
        if (math.lengthsq(result) > 1e-16f)
        {
            return result;
        }

        var axis = MathF.Abs(left.x) < MathF.Abs(left.y)
            ? new float3(1f, 0f, 0f)
            : new float3(0f, 1f, 0f);
        return math.cross(left, axis);
    }
}
