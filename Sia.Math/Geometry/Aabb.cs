using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Sia.Math;

[DebuggerDisplay("{Min} - {Max}")]
[Serializable]
public record struct Aabb(float3 Min, float3 Max)
{
    public float3 Extents => Max - Min;

    public float3 Center => (Max + Min) * 0.5f;

    public bool IsValid => math.all(Min <= Max);

    public float SurfaceArea
    {
        get
        {
            var diff = Max - Min;
            return 2 * math.dot(diff, diff.yzx);
        }
    }

    public static Aabb Union(Aabb a, Aabb b)
    {
        a.Include(b);
        return a;
    }

    public void Intersect(Aabb aabb)
    {
        Min = math.max(Min, aabb.Min);
        Max = math.min(Max, aabb.Max);
    }

    public void Include(float3 point)
    {
        Min = math.min(Min, point);
        Max = math.max(Max, point);
    }

    public void Include(Aabb aabb)
    {
        Min = math.min(Min, aabb.Min);
        Max = math.max(Max, aabb.Max);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(float3 point) => math.all(point >= Min & point <= Max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(Aabb aabb) => math.all((Min <= aabb.Min) & (Max >= aabb.Max));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Overlaps(Aabb aabb) => math.all(Max >= aabb.Min & Min <= aabb.Max);
}