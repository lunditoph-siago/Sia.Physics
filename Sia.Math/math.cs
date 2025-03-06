using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable 8981 

namespace Sia.Math;

public static partial class math
{
    /// <summary>Extrinsic rotation order. Specifies in which order rotations around the principal axes (x, y and z) are to be applied.</summary>
    public enum RotationOrder : byte { XYZ, XZY, YXZ, YZX, ZXY, ZYX, Default = ZXY }

    /// <summary>Specifies a shuffle component.</summary>
    public enum ShuffleComponent : byte { LeftX,LeftY, LeftZ, LeftW, RightX, RightY, RightZ, RightW }

    /// <summary>The mathematical constant e also known as Euler's number. Approximately 2.72. This is a f64/double precision constant.</summary>
    public const double E_DBL = 2.71828182845904523536;

    /// <summary>The base 2 logarithm of e. Approximately 1.44. This is a f64/double precision constant.</summary>
    public const double LOG2E_DBL = 1.44269504088896340736;

    /// <summary>The base 10 logarithm of e. Approximately 0.43. This is a f64/double precision constant.</summary>
    public const double LOG10E_DBL = 0.434294481903251827651;

    /// <summary>The natural logarithm of 2. Approximately 0.69. This is a f64/double precision constant.</summary>
    public const double LN2_DBL = 0.693147180559945309417;

    /// <summary>The natural logarithm of 10. Approximately 2.30. This is a f64/double precision constant.</summary>
    public const double LN10_DBL = 2.30258509299404568402;

    /// <summary>The mathematical constant pi. Approximately 3.14. This is a f64/double precision constant.</summary>
    public const double PI_DBL = 3.14159265358979323846;

    /// <summary>The conversion constant 180 divide pi. used to convert radians to degree</summary>
    public const double TODEGREES_DBL = 57.29577951308232;

    /// <summary>The conversion constant pi divide 180. used to convert degrees to radians.</summary>
    public const double TORADIANS_DBL = 0.017453292519943296;

    /// <summary>The square root 2. Approximately 1.41. This is a f64/double precision constant.</summary>
    public const double SQRT2_DBL = 1.41421356237309504880;

    /// <summary>The difference between 1.0 and the next representable f64/double precision number.</summary>
    public const double EPSILON_DBL = 2.22044604925031308085e-16;

    /// <summary>The smallest positive normal number representable in a float.</summary>
    public const float FLT_MIN_NORMAL = 1.175494351e-38F;

    /// <summary>The smallest positive normal number representable in a double. This is a f64/double precision constant.</summary>
    public const double DBL_MIN_NORMAL = 2.2250738585072014e-308;

    /// <summary>The mathematical constant e also known as Euler's number. Approximately 2.72.</summary>
    public const float E = (float)E_DBL;

    /// <summary>The base 2 logarithm of e. Approximately 1.44.</summary>
    public const float LOG2E = (float)LOG2E_DBL;

    /// <summary>The base 10 logarithm of e. Approximately 0.43.</summary>
    public const float LOG10E = (float)LOG10E_DBL;

    /// <summary>The natural logarithm of 2. Approximately 0.69.</summary>
    public const float LN2 = (float)LN2_DBL;

    /// <summary>The natural logarithm of 10. Approximately 2.30.</summary>
    public const float LN10 = (float)LN10_DBL;

    /// <summary>The mathematical constant pi. Approximately 3.14.</summary>
    public const float PI = (float)PI_DBL;

    /// <summary>The conversion constant 180 divide pi.</summary>
    public const float TODEGREES = (float)TODEGREES_DBL;

    /// <summary>The conversion constant pi divide 180.</summary>
    public const float TORADIANS = (float)TORADIANS_DBL;

    /// <summary>The square root 2. Approximately 1.41.</summary>
    public const float SQRT2 = (float)SQRT2_DBL;

    /// <summary>The difference between 1.0f and the next representable f32/single precision number.</summary>
    public const float EPSILON = 1.1920928955078125e-7f;

    #region as

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int bitmask(bool4 value)
    {
        int mask = 0;
        if (value.x) mask |= 0x01;
        if (value.y) mask |= 0x02;
        if (value.z) mask |= 0x04;
        if (value.w) mask |= 0x08;
        return mask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int asint(uint x) => *(int*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int2 asint(uint2 x) => *(int2*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int3 asint(uint3 x) => *(int3*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int4 asint(uint4 x) => *(int4*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int asint(float x) => *(int*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int2 asint(float2 x) => *(int2*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int3 asint(float3 x) => *(int3*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe int4 asint(float4 x) => *(int4*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint asuint(int x) => (uint)x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe uint2 asuint(int2 x) => *(uint2*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe uint3 asuint(int3 x) => *(uint3*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe uint4 asuint(int4 x) => *(uint4*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe uint asuint(float x) => *(uint*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe uint2 asuint(float2 x) => *(uint2*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe uint3 asuint(float3 x) => *(uint3*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe uint4 asuint(float4 x) => *(uint4*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long aslong(ulong x) => (long)x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe long aslong(double x) => *(long*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong asulong(long x) => (ulong)x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ulong asulong(double x) => *(ulong*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe float asfloat(int x) => *(float*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe float2 asfloat(int2 x) => *(float2*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe float3 asfloat(int3 x) => *(float3*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe float4 asfloat(int4 x) => *(float4*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe float asfloat(uint x) => *(float*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe float2 asfloat(uint2 x) => *(float2*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe float3 asfloat(uint3 x) => *(float3*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe float4 asfloat(uint4 x) => *(float4*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe double asdouble(long x) => *(double*)&x;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe double asdouble(ulong x) => *(double*)&x;

    #endregion

    #region is

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool isfinite(float x) => abs(x) < float.PositiveInfinity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool2 isfinite(float2 x) => abs(x) < float.PositiveInfinity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool3 isfinite(float3 x) => abs(x) < float.PositiveInfinity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool4 isfinite(float4 x) => abs(x) < float.PositiveInfinity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool isfinite(double x) => abs(x) < double.PositiveInfinity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool2 isfinite(double2 x) => abs(x) < double.PositiveInfinity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool3 isfinite(double3 x) => abs(x) < double.PositiveInfinity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool4 isfinite(double4 x) => abs(x) < double.PositiveInfinity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool isinf(float x) => abs(x) == float.PositiveInfinity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool2 isinf(float2 x) => abs(x) == float.PositiveInfinity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool3 isinf(float3 x) => abs(x) == float.PositiveInfinity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool4 isinf(float4 x) => abs(x) == float.PositiveInfinity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool isinf(double x) => abs(x) == double.PositiveInfinity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool2 isinf(double2 x) => abs(x) == double.PositiveInfinity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool3 isinf(double3 x) => abs(x) == double.PositiveInfinity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool4 isinf(double4 x) => abs(x) == double.PositiveInfinity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool isnan(float x) => (asuint(x) & 0x7FFFFFFF) > 0x7F800000;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool2 isnan(float2 x) => (asuint(x) & 0x7FFFFFFF) > 0x7F800000;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool3 isnan(float3 x) => (asuint(x) & 0x7FFFFFFF) > 0x7F800000;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool4 isnan(float4 x) => (asuint(x) & 0x7FFFFFFF) > 0x7F800000;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool isnan(double x) => (asulong(x) & 0x7FFFFFFFFFFFFFFF) > 0x7FF0000000000000;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool2 isnan(double2 x) => bool2((asulong(x.x) & 0x7FFFFFFFFFFFFFFF) > 0x7FF0000000000000, (asulong(x.y) & 0x7FFFFFFFFFFFFFFF) > 0x7FF0000000000000);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool3 isnan(double3 x) => bool3((asulong(x.x) & 0x7FFFFFFFFFFFFFFF) > 0x7FF0000000000000, (asulong(x.y) & 0x7FFFFFFFFFFFFFFF) > 0x7FF0000000000000, (asulong(x.z) & 0x7FFFFFFFFFFFFFFF) > 0x7FF0000000000000);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool4 isnan(double4 x) => bool4((asulong(x.x) & 0x7FFFFFFFFFFFFFFF) > 0x7FF0000000000000, (asulong(x.y) & 0x7FFFFFFFFFFFFFFF) > 0x7FF0000000000000, (asulong(x.z) & 0x7FFFFFFFFFFFFFFF) > 0x7FF0000000000000, (asulong(x.w) & 0x7FFFFFFFFFFFFFFF) > 0x7FF0000000000000);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ispow2(int x) => x > 0 && ((x & (x - 1)) == 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool2 ispow2(int2 x) => new(ispow2(x.x), ispow2(x.y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool3 ispow2(int3 x) => new(ispow2(x.x), ispow2(x.y), ispow2(x.z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool4 ispow2(int4 x) => new(ispow2(x.x), ispow2(x.y), ispow2(x.z), ispow2(x.w));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ispow2(uint x) => x > 0 && ((x & (x - 1)) == 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool2 ispow2(uint2 x) => new(ispow2(x.x), ispow2(x.y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool3 ispow2(uint3 x) => new(ispow2(x.x), ispow2(x.y), ispow2(x.z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool4 ispow2(uint4 x) => new(ispow2(x.x), ispow2(x.y), ispow2(x.z), ispow2(x.w));

    #endregion

    #region cross

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 cross(float3 x, float3 y) => (x * y.yzx - x.yzx * y).yzx;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 cross(double3 x, double3 y) => (x * y.yzx - x.yzx * y).yzx;

    #endregion

    #region f16 & f32

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float f16tof32(uint x)
    {
        const uint shifted_exp = (0x7c00 << 13);
        var uf = (x & 0x7fff) << 13;
        var e = uf & shifted_exp;
        uf += (127 - 15) << 23;
        uf += select(0, (128u - 16u) << 23, e == shifted_exp);
        uf = select(uf, asuint(asfloat(uf + (1 << 23)) - 6.10351563e-05f), e == 0);
        uf |= (x & 0x8000) << 16;
        return asfloat(uf);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 f16tof32(uint2 x)
    {
        const uint shifted_exp = (0x7c00 << 13);
        var uf = (x & 0x7fff) << 13;
        var e = uf & shifted_exp;
        uf += (127 - 15) << 23;
        uf += select(0, (128u - 16u) << 23, e == shifted_exp);
        uf = select(uf, asuint(asfloat(uf + (1 << 23)) - 6.10351563e-05f), e == 0);
        uf |= (x & 0x8000) << 16;
        return asfloat(uf);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 f16tof32(uint3 x)
    {
        const uint shifted_exp = (0x7c00 << 13);
        var uf = (x & 0x7fff) << 13;
        var e = uf & shifted_exp;
        uf += (127 - 15) << 23;
        uf += select(0, (128u - 16u) << 23, e == shifted_exp);
        uf = select(uf, asuint(asfloat(uf + (1 << 23)) - 6.10351563e-05f), e == 0);
        uf |= (x & 0x8000) << 16;
        return asfloat(uf);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4 f16tof32(uint4 x)
    {
        const uint shifted_exp = (0x7c00 << 13);
        var uf = (x & 0x7fff) << 13;
        var e = uf & shifted_exp;
        uf += (127 - 15) << 23;
        uf += select(0, (128u - 16u) << 23, e == shifted_exp);
        uf = select(uf, asuint(asfloat(uf + (1 << 23)) - 6.10351563e-05f), e == 0);
        uf |= (x & 0x8000) << 16;
        return asfloat(uf);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint f32tof16(float x)
    {
        const int infinity_32 = 255 << 23;
        const uint msk = 0x7FFFF000u;

        var ux = asuint(x);
        var uux = ux & msk;
        var h = (uint)(asuint(min(asfloat(uux) * 1.92592994e-34f, 260042752.0f)) + 0x1000) >> 13; // Clamp to signed infinity if overflowed
        h = select(h, select(0x7c00u, 0x7e00u, (int)uux > infinity_32), (int)uux >= infinity_32); // NaN->qNaN and Inf->Inf
        return h | (ux & ~msk) >> 16;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint2 f32tof16(float2 x)
    {
        const int infinity_32 = 255 << 23;
        const uint msk = 0x7FFFF000u;

        var ux = asuint(x);
        var uux = ux & msk;
        var h = (uint2)(asint(min(asfloat(uux) * 1.92592994e-34f, 260042752.0f)) + 0x1000) >> 13; // Clamp to signed infinity if overflowed
        h = select(h, select(0x7c00u, 0x7e00u, (int2)uux > infinity_32), (int2)uux >= infinity_32); // NaN->qNaN and Inf->Inf
        return h | (ux & ~msk) >> 16;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint3 f32tof16(float3 x)
    {
        const int infinity_32 = 255 << 23;
        const uint msk = 0x7FFFF000u;

        var ux = asuint(x);
        var uux = ux & msk;
        var h = (uint3)(asint(min(asfloat(uux) * 1.92592994e-34f, 260042752.0f)) + 0x1000) >> 13; // Clamp to signed infinity if overflowed
        h = select(h, select(0x7c00u, 0x7e00u, (int3)uux > infinity_32), (int3)uux >= infinity_32); // NaN->qNaN and Inf->Inf
        return h | (ux & ~msk) >> 16;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint4 f32tof16(float4 x)
    {
        const int infinity_32 = 255 << 23;
        const uint msk = 0x7FFFF000u;

        var ux = asuint(x);
        var uux = ux & msk;
        var h = (uint4)(asint(min(asfloat(uux) * 1.92592994e-34f, 260042752.0f)) + 0x1000) >> 13; // Clamp to signed infinity if overflowed
        h = select(h, select(0x7c00u, 0x7e00u, (int4)uux > infinity_32), (int4)uux >= infinity_32); // NaN->qNaN and Inf->Inf
        return h | (ux & ~msk) >> 16;
    }

    #endregion

    // Internal

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float4 movelh(float4 a, float4 b) => shuffle(a, b, ShuffleComponent.LeftX, ShuffleComponent.LeftY, ShuffleComponent.RightX, ShuffleComponent.RightY);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double4 movelh(double4 a, double4 b) => shuffle(a, b, ShuffleComponent.LeftX, ShuffleComponent.LeftY, ShuffleComponent.RightX, ShuffleComponent.RightY);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static float4 movehl(float4 a, float4 b) => shuffle(b, a, ShuffleComponent.LeftZ, ShuffleComponent.LeftW, ShuffleComponent.RightZ, ShuffleComponent.RightW);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double4 movehl(double4 a, double4 b) => shuffle(b, a, ShuffleComponent.LeftZ, ShuffleComponent.LeftW, ShuffleComponent.RightZ, ShuffleComponent.RightW);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint fold_to_uint(double x)  // utility for double hashing
    {
        LongDoubleUnion u;
        u.longValue = 0;
        u.doubleValue = x;
        return (uint)(u.longValue >> 32) ^ (uint)u.longValue;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint2 fold_to_uint(double2 x) => uint2(fold_to_uint(x.x), fold_to_uint(x.y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint3 fold_to_uint(double3 x) => uint3(fold_to_uint(x.x), fold_to_uint(x.y), fold_to_uint(x.z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static uint4 fold_to_uint(double4 x) => uint4(fold_to_uint(x.x), fold_to_uint(x.y), fold_to_uint(x.z), fold_to_uint(x.w));

    [StructLayout(LayoutKind.Explicit)]
    internal struct LongDoubleUnion
    {
        [FieldOffset(0)]
        public long longValue;
        [FieldOffset(0)]
        public double doubleValue;
    }
}
