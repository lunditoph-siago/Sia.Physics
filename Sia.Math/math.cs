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

    #region min

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int min(int x, int y) => x < y ? x : y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int2 min(int2 x, int2 y) => new(min(x.x, y.x), min(x.y, y.y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int3 min(int3 x, int3 y) => new(min(x.x, y.x), min(x.y, y.y), min(x.z, y.z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int4 min(int4 x, int4 y) => new(min(x.x, y.x), min(x.y, y.y), min(x.z, y.z), min(x.w, y.w));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint min(uint x, uint y) => x < y ? x : y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint2 min(uint2 x, uint2 y) => new(min(x.x, y.x), min(x.y, y.y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint3 min(uint3 x, uint3 y) => new(min(x.x, y.x), min(x.y, y.y), min(x.z, y.z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint4 min(uint4 x, uint4 y) => new(min(x.x, y.x), min(x.y, y.y), min(x.z, y.z), min(x.w, y.w));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long min(long x, long y) => x < y ? x : y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong min(ulong x, ulong y) => x < y ? x : y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float min(float x, float y) => float.IsNaN(y) || x < y ? x : y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 min(float2 x, float2 y) => new(min(x.x, y.x), min(x.y, y.y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 min(float3 x, float3 y) => new(min(x.x, y.x), min(x.y, y.y), min(x.z, y.z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4 min(float4 x, float4 y) => new(min(x.x, y.x), min(x.y, y.y), min(x.z, y.z), min(x.w, y.w));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double min(double x, double y) => double.IsNaN(y) || x < y ? x : y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double2 min(double2 x, double2 y) => new(min(x.x, y.x), min(x.y, y.y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 min(double3 x, double3 y) => new(min(x.x, y.x), min(x.y, y.y), min(x.z, y.z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double4 min(double4 x, double4 y) => new(min(x.x, y.x), min(x.y, y.y), min(x.z, y.z), min(x.w, y.w));

    #endregion

    #region max

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int max(int x, int y) => x > y ? x : y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int2 max(int2 x, int2 y) => new(max(x.x, y.x), max(x.y, y.y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int3 max(int3 x, int3 y) => new(max(x.x, y.x), max(x.y, y.y), max(x.z, y.z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int4 max(int4 x, int4 y) => new(max(x.x, y.x), max(x.y, y.y), max(x.z, y.z), max(x.w, y.w));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint max(uint x, uint y) => x > y ? x : y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint2 max(uint2 x, uint2 y) => new(max(x.x, y.x), max(x.y, y.y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint3 max(uint3 x, uint3 y) => new(max(x.x, y.x), max(x.y, y.y), max(x.z, y.z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint4 max(uint4 x, uint4 y) => new(max(x.x, y.x), max(x.y, y.y), max(x.z, y.z), max(x.w, y.w));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long max(long x, long y) => x > y ? x : y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong max(ulong x, ulong y) => x > y ? x : y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float max(float x, float y) => float.IsNaN(y) || x > y ? x : y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 max(float2 x, float2 y) => new(max(x.x, y.x), max(x.y, y.y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 max(float3 x, float3 y) => new(max(x.x, y.x), max(x.y, y.y), max(x.z, y.z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4 max(float4 x, float4 y) => new(max(x.x, y.x), max(x.y, y.y), max(x.z, y.z), max(x.w, y.w));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double max(double x, double y) => double.IsNaN(y) || x > y ? x : y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double2 max(double2 x, double2 y) => new(max(x.x, y.x), max(x.y, y.y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 max(double3 x, double3 y) => new(max(x.x, y.x), max(x.y, y.y), max(x.z, y.z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double4 max(double4 x, double4 y) => new(max(x.x, y.x), max(x.y, y.y), max(x.z, y.z), max(x.w, y.w));

    #endregion

    #region any

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool any(bool2 x) => x.x || x.y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool any(bool3 x) => x.x || x.y || x.z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool any(bool4 x) => x.x || x.y || x.z || x.w;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool any(int2 x) => x.x != 0 || x.y != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool any(int3 x) => x.x != 0 || x.y != 0 || x.z != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool any(int4 x) => x.x != 0 || x.y != 0 || x.z != 0 || x.w != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool any(uint2 x) => x.x != 0 || x.y != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool any(uint3 x) => x.x != 0 || x.y != 0 || x.z != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool any(uint4 x) => x.x != 0 || x.y != 0 || x.z != 0 || x.w != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool any(float2 x) => x.x != 0.0f || x.y != 0.0f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool any(float3 x) => x.x != 0.0f || x.y != 0.0f || x.z != 0.0f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool any(float4 x) => x.x != 0.0f || x.y != 0.0f || x.z != 0.0f || x.w != 0.0f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool any(double2 x) => x.x != 0.0 || x.y != 0.0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool any(double3 x) => x.x != 0.0 || x.y != 0.0 || x.z != 0.0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool any(double4 x) => x.x != 0.0 || x.y != 0.0 || x.z != 0.0 || x.w != 0.0;

    #endregion

    #region all

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool all(bool2 x) => x.x && x.y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool all(bool3 x) => x.x && x.y && x.z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool all(bool4 x) => x.x && x.y && x.z && x.w;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool all(int2 x) => x.x != 0 && x.y != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool all(int3 x) => x.x != 0 && x.y != 0 && x.z != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool all(int4 x) => x.x != 0 && x.y != 0 && x.z != 0 && x.w != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool all(uint2 x) => x.x != 0 && x.y != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool all(uint3 x) => x.x != 0 && x.y != 0 && x.z != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool all(uint4 x) => x.x != 0 && x.y != 0 && x.z != 0 && x.w != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool all(float2 x) => x.x != 0.0f && x.y != 0.0f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool all(float3 x) => x.x != 0.0f && x.y != 0.0f && x.z != 0.0f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool all(float4 x) => x.x != 0.0f && x.y != 0.0f && x.z != 0.0f && x.w != 0.0f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool all(double2 x) => x.x != 0.0 && x.y != 0.0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool all(double3 x) => x.x != 0.0 && x.y != 0.0 && x.z != 0.0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool all(double4 x) => x.x != 0.0 && x.y != 0.0 && x.z != 0.0 && x.w != 0.0;

    #endregion

    #region select

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int select(int a, int b, bool c) => c ? b : a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int2 select(int2 a, int2 b, bool c) => c ? b : a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int3 select(int3 a, int3 b, bool c) => c ? b : a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int4 select(int4 a, int4 b, bool c) => c ? b : a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int2 select(int2 a, int2 b, bool2 c) => new(c.x ? b.x : a.x, c.y ? b.y : a.y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int3 select(int3 a, int3 b, bool3 c) => new(c.x ? b.x : a.x, c.y ? b.y : a.y, c.z ? b.z : a.z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int4 select(int4 a, int4 b, bool4 c) => new(c.x ? b.x : a.x, c.y ? b.y : a.y, c.z ? b.z : a.z, c.w ? b.w : a.w);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint select(uint a, uint b, bool c) => c ? b : a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint2 select(uint2 a, uint2 b, bool c) => c ? b : a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint3 select(uint3 a, uint3 b, bool c) => c ? b : a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint4 select(uint4 a, uint4 b, bool c) => c ? b : a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint2 select(uint2 a, uint2 b, bool2 c) => new(c.x ? b.x : a.x, c.y ? b.y : a.y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint3 select(uint3 a, uint3 b, bool3 c) => new(c.x ? b.x : a.x, c.y ? b.y : a.y, c.z ? b.z : a.z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint4 select(uint4 a, uint4 b, bool4 c) => new(c.x ? b.x : a.x, c.y ? b.y : a.y, c.z ? b.z : a.z, c.w ? b.w : a.w);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long select(long a, long b, bool c) => c ? b : a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong select(ulong a, ulong b, bool c) => c ? b : a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float select(float a, float b, bool c) => c ? b : a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 select(float2 a, float2 b, bool c) => c ? b : a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 select(float3 a, float3 b, bool c) => c ? b : a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4 select(float4 a, float4 b, bool c) => c ? b : a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float2 select(float2 a, float2 b, bool2 c) => new(c.x ? b.x : a.x, c.y ? b.y : a.y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 select(float3 a, float3 b, bool3 c) => new(c.x ? b.x : a.x, c.y ? b.y : a.y, c.z ? b.z : a.z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4 select(float4 a, float4 b, bool4 c) => new(c.x ? b.x : a.x, c.y ? b.y : a.y, c.z ? b.z : a.z, c.w ? b.w : a.w);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double select(double a, double b, bool c) => c ? b : a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double2 select(double2 a, double2 b, bool c) => c ? b : a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 select(double3 a, double3 b, bool c) => c ? b : a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double4 select(double4 a, double4 b, bool c) => c ? b : a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double2 select(double2 a, double2 b, bool2 c) => new(c.x ? b.x : a.x, c.y ? b.y : a.y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 select(double3 a, double3 b, bool3 c) => new(c.x ? b.x : a.x, c.y ? b.y : a.y, c.z ? b.z : a.z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double4 select(double4 a, double4 b, bool4 c) => new(c.x ? b.x : a.x, c.y ? b.y : a.y, c.z ? b.z : a.z, c.w ? b.w : a.w);

    #endregion

    #region csum

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int csum(int2 x) => x.x + x.y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int csum(int3 x) => x.x + x.y + x.z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int csum(int4 x) => x.x + x.y + x.z + x.w;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint csum(uint2 x) => x.x + x.y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint csum(uint3 x) => x.x + x.y + x.z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint csum(uint4 x) => x.x + x.y + x.z + x.w;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float csum(float2 x) => x.x + x.y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float csum(float3 x) => x.x + x.y + x.z;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public static float csum(float4 x) => (x.x + x.y) + (x.z + x.w);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public static double csum(double2 x) => x.x + x.y;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public static double csum(double3 x) => x.x + x.y + x.z;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public static double csum(double4 x) => (x.x + x.y) + (x.z + x.w);

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
