using System.Runtime.CompilerServices;

namespace Sia.Physics;

public readonly record struct Pose(float3 Position, quaternion Orientation)
{
    public static readonly Pose Identity = new(float3.zero, quaternion.identity);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float3 TransformPoint(float3 point) => math.mul(Orientation, point) + Position;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float3 TransformDirection(float3 direction) => math.mul(Orientation, direction);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float3 InverseTransformPoint(float3 point) => math.mul(math.conjugate(Orientation), point - Position);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float3 InverseTransformDirection(float3 direction) => math.mul(math.conjugate(Orientation), direction);
}

