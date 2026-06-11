using System.Runtime.CompilerServices;

namespace Sia.Physics;

public readonly record struct BoundingBox(float3 Min, float3 Max)
{
    public float3 Center => (Min + Max) * 0.5f;

    public float3 HalfExtents => (Max - Min) * 0.5f;

    public float SurfaceArea
    {
        get
        {
            var size = Max - Min;
            return 2f * (size.x * size.y + size.y * size.z + size.z * size.x);
        }
    }

    public bool IsValid => Min.x <= Max.x && Min.y <= Max.y && Min.z <= Max.z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Overlaps(in BoundingBox other) =>
        Max.x >= other.Min.x && Min.x <= other.Max.x &&
        Max.y >= other.Min.y && Min.y <= other.Max.y &&
        Max.z >= other.Min.z && Min.z <= other.Max.z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(float3 point) =>
        point.x >= Min.x && point.x <= Max.x &&
        point.y >= Min.y && point.y <= Max.y &&
        point.z >= Min.z && point.z <= Max.z;

    public BoundingBox Expanded(float amount) => new(Min - amount, Max + amount);

    public static BoundingBox Merge(in BoundingBox left, in BoundingBox right) =>
        new(math.min(left.Min, right.Min), math.max(left.Max, right.Max));
}

