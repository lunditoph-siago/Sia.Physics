using System.Runtime.CompilerServices;

namespace Sia.Physics;

internal static class InertiaMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ApplyInverse(in BodyState body, float3 value)
    {
        if (!body.Body.IsDynamic)
        {
            return float3.zero;
        }

        var local = math.mul(math.conjugate(body.Pose.Rotation), value);
        return math.mul(body.Pose.Rotation, local * body.Body.InverseInertia);
    }
}

