using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using static Sia.Math.math;

#pragma warning disable 8981

namespace Sia.Math;

[Serializable]
public struct quaternion : IEquatable<quaternion>, IFormattable
{
    /// <summary>The quaternion component values.</summary>
    public float4 value;

    /// <summary>A quaternion representing the identity transform.</summary>
    public static readonly quaternion identity = new(0.0f, 0.0f, 0.0f, 1.0f);

    /// <summary>Constructs a <see cref="quaternion" /> from four <see cref="float" /> values.</summary>
    /// <param name="x">The value to assign to the <see cref="x" /> field.</param>
    /// <param name="y">The value to assign to the <see cref="y" /> field.</param>
    /// <param name="z">The value to assign to the <see cref="z" /> field.</param>
    /// <param name="w">The value to assign to the <see cref="w" /> field.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public quaternion(float x, float y, float z, float w) { value.x = x; value.y = y; value.z = z; value.w = w; }

    /// <summary>Constructs a <see cref="quaternion" /> from a <see cref="float4" /> vector.</summary>
    /// <param name="v">The value to assign to the <see cref="v" /> field.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public quaternion(float4 v) => value = v;

    /// <summary>Implicitly converts a <see cref="float4" /> vector to a <see cref="quaternion" />.</summary>
    /// <param name="v">The <see cref="float4" /> to convert to <see cref="quaternion" />.</param>
    /// <returns>The <see cref="quaternion" /> converted from arguments.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator quaternion(float4 v) => new(v);

    /// <summary>Constructs a unit <see cref="quaternion" /> from a <see cref="float3x3" /> rotation matrix. The matrix must be orthonormal.</summary>
    /// <param name="m">The <see cref="float3x3" /> orthonormal rotation matrix.</param>
    public quaternion(float3x3 m)
    {
        var u = m.c0;
        var v = m.c1;
        var w = m.c2;

        var u_sign = (asuint(u.x) & 0x80000000);
        var t = v.y + asfloat(asuint(w.z) ^ u_sign);
        var u_mask = uint4((int)u_sign >> 31);
        var t_mask = uint4(asint(t) >> 31);

        var tr = 1.0f + abs(u.x);

        var sign_flips = uint4(0x00000000, 0x80000000, 0x80000000, 0x80000000) ^
                         (u_mask & uint4(0x00000000, 0x80000000, 0x00000000, 0x80000000)) ^
                         (t_mask & uint4(0x80000000, 0x80000000, 0x80000000, 0x00000000));

        value = float4(tr, u.y, w.x, v.z) + asfloat(asuint(float4(t, v.x, u.z, w.y)) ^ sign_flips);   // +---, +++-, ++-+, +-++

        value = asfloat((asuint(value) & ~u_mask) | (asuint(value.zwxy) & u_mask));
        value = asfloat((asuint(value.wzyx) & ~t_mask) | (asuint(value) & t_mask));
        value = normalize(value);
    }

    /// <summary>Constructs a unit quaternion from an orthonormal <see cref="float4x4" /> matrix.</summary>
    /// <param name="m">The <see cref="float4x4" /> orthonormal rotation matrix.</param>
    public quaternion(float4x4 m)
    {
        var u = m.c0;
        var v = m.c1;
        var w = m.c2;

        var u_sign = (asuint(u.x) & 0x80000000);
        var t = v.y + asfloat(asuint(w.z) ^ u_sign);
        var u_mask = uint4((int)u_sign >> 31);
        var t_mask = uint4(asint(t) >> 31);

        var tr = 1.0f + abs(u.x);

        var sign_flips = uint4(0x00000000, 0x80000000, 0x80000000, 0x80000000) ^
                         (u_mask & uint4(0x00000000, 0x80000000, 0x00000000, 0x80000000)) ^
                         (t_mask & uint4(0x80000000, 0x80000000, 0x80000000, 0x00000000));

        value = float4(tr, u.y, w.x, v.z) + asfloat(asuint(float4(t, v.x, u.z, w.y)) ^ sign_flips);   // +---, +++-, ++-+, +-++

        value = asfloat((asuint(value) & ~u_mask) | (asuint(value.zwxy) & u_mask));
        value = asfloat((asuint(value.wzyx) & ~t_mask) | (asuint(value) & t_mask));
        value = normalize(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion AxisAngle(float3 axis, float angle)
    {
        sincos(0.5f * angle, out var sina, out var cosa);
        return new quaternion(float4(axis * sina, cosa));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion EulerXYZ(float3 xyz)
    {
        // return mul(rotateZ(xyz.z), mul(rotateY(xyz.y), rotateX(xyz.x)));
        sincos(0.5f * xyz, out var s, out var c);
        return new quaternion(
            // s.x * c.y * c.z - s.y * s.z * c.x,
            // s.y * c.x * c.z + s.x * s.z * c.y,
            // s.z * c.x * c.y - s.x * s.y * c.z,
            // c.x * c.y * c.z + s.y * s.z * s.x
            float4(s.xyz, c.x) * c.yxxy * c.zzyz + s.yxxy * s.zzyz * float4(c.xyz, s.x) * float4(-1.0f, 1.0f, -1.0f, 1.0f)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion EulerXZY(float3 xyz)
    {
        // return mul(rotateY(xyz.y), mul(rotateZ(xyz.z), rotateX(xyz.x)));
        sincos(0.5f * xyz, out var s, out var c);
        return new quaternion(
            // s.x * c.y * c.z + s.y * s.z * c.x,
            // s.y * c.x * c.z + s.x * s.z * c.y,
            // s.z * c.x * c.y - s.x * s.y * c.z,
            // c.x * c.y * c.z - s.y * s.z * s.x
            float4(s.xyz, c.x) * c.yxxy * c.zzyz + s.yxxy * s.zzyz * float4(c.xyz, s.x) * float4(1.0f, 1.0f, -1.0f, -1.0f)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion EulerYXZ(float3 xyz)
    {
        // return mul(rotateZ(xyz.z), mul(rotateX(xyz.x), rotateY(xyz.y)));
        sincos(0.5f * xyz, out var s, out var c);
        return new quaternion(
            // s.x * c.y * c.z - s.y * s.z * c.x,
            // s.y * c.x * c.z + s.x * s.z * c.y,
            // s.z * c.x * c.y + s.x * s.y * c.z,
            // c.x * c.y * c.z - s.y * s.z * s.x
            float4(s.xyz, c.x) * c.yxxy * c.zzyz +
            s.yxxy * s.zzyz * float4(c.xyz, s.x) * float4(-1.0f, 1.0f, 1.0f, -1.0f)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion EulerYZX(float3 xyz)
    {
        // return mul(rotateX(xyz.x), mul(rotateZ(xyz.z), rotateY(xyz.y)));
        sincos(0.5f * xyz, out var s, out var c);
        return new quaternion(
            // s.x * c.y * c.z - s.y * s.z * c.x,
            // s.y * c.x * c.z - s.x * s.z * c.y,
            // s.z * c.x * c.y + s.x * s.y * c.z,
            // c.x * c.y * c.z + s.y * s.z * s.x
            float4(s.xyz, c.x) * c.yxxy * c.zzyz + s.yxxy * s.zzyz * float4(c.xyz, s.x) * float4(-1.0f, -1.0f, 1.0f, 1.0f)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion EulerZXY(float3 xyz)
    {
        // return mul(rotateY(xyz.y), mul(rotateX(xyz.x), rotateZ(xyz.z)));
        sincos(0.5f * xyz, out var s, out var c);
        return new quaternion(
            // s.x * c.y * c.z + s.y * s.z * c.x,
            // s.y * c.x * c.z - s.x * s.z * c.y,
            // s.z * c.x * c.y - s.x * s.y * c.z,
            // c.x * c.y * c.z + s.y * s.z * s.x
            float4(s.xyz, c.x) * c.yxxy * c.zzyz + s.yxxy * s.zzyz * float4(c.xyz, s.x) * float4(1.0f, -1.0f, -1.0f, 1.0f)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion EulerZYX(float3 xyz)
    {
        // return mul(rotateX(xyz.x), mul(rotateY(xyz.y), rotateZ(xyz.z)));
        sincos(0.5f * xyz, out var s, out var c);
        return new quaternion(
            // s.x * c.y * c.z + s.y * s.z * c.x,
            // s.y * c.x * c.z - s.x * s.z * c.y,
            // s.z * c.x * c.y + s.x * s.y * c.z,
            // c.x * c.y * c.z - s.y * s.x * s.z
            float4(s.xyz, c.x) * c.yxxy * c.zzyz + s.yxxy * s.zzyz * float4(c.xyz, s.x) * float4(1.0f, -1.0f, 1.0f, -1.0f)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion EulerXYZ(float x, float y, float z) => EulerXYZ(float3(x, y, z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion EulerXZY(float x, float y, float z) => EulerXZY(float3(x, y, z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion EulerYXZ(float x, float y, float z) => EulerYXZ(float3(x, y, z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion EulerYZX(float x, float y, float z) => EulerYZX(float3(x, y, z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion EulerZXY(float x, float y, float z) => EulerZXY(float3(x, y, z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion EulerZYX(float x, float y, float z) => EulerZYX(float3(x, y, z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion Euler(float3 xyz, RotationOrder order = RotationOrder.ZXY) => order switch
    {
        RotationOrder.XYZ => EulerXYZ(xyz),
        RotationOrder.XZY => EulerXZY(xyz),
        RotationOrder.YXZ => EulerYXZ(xyz),
        RotationOrder.YZX => EulerYZX(xyz),
        RotationOrder.ZXY => EulerZXY(xyz),
        RotationOrder.ZYX => EulerZYX(xyz),
        _ => identity
    };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion Euler(float x, float y, float z, RotationOrder order = RotationOrder.Default) => Euler(float3(x, y, z), order);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion RotateX(float angle)
    {
        sincos(0.5f * angle, out var sina, out var cosa);
        return new quaternion(sina, 0.0f, 0.0f, cosa);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion RotateY(float angle)
    {
        sincos(0.5f * angle, out var sina, out var cosa);
        return new quaternion(0.0f, sina, 0.0f, cosa);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion RotateZ(float angle)
    {
        sincos(0.5f * angle, out var sina, out var cosa);
        return new quaternion(0.0f, 0.0f, sina, cosa);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion LookRotation(float3 forward, float3 up)
    {
        var t = normalize(cross(up, forward));
        return new quaternion(float3x3(t, cross(forward, t), forward));
    }

    public static quaternion LookRotationSafe(float3 forward, float3 up)
    {
        var forwardLengthSq = dot(forward, forward);
        var upLengthSq = dot(up, up);

        forward *= rsqrt(forwardLengthSq);
        up *= rsqrt(upLengthSq);

        var t = cross(up, forward);
        var tLengthSq = dot(t, t);
        t *= rsqrt(tLengthSq);

        var mn = min(min(forwardLengthSq, upLengthSq), tLengthSq);
        var mx = max(max(forwardLengthSq, upLengthSq), tLengthSq);

        var accept = mn > 1e-35f && mx < 1e35f && isfinite(forwardLengthSq) && isfinite(upLengthSq) && isfinite(tLengthSq);
        return new quaternion(select(float4(0.0f, 0.0f, 0.0f, 1.0f), new quaternion(float3x3(t, cross(forward, t), forward)).value, accept));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(quaternion x) =>
        value.x == x.value.x && value.y == x.value.y && value.z == x.value.z && value.w == x.value.w;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object? x) => x is quaternion converted && Equals(converted);

    /// <summary>Returns a hash code for the <see cref="quaternion" />.</summary>
    /// <returns>The hash code of the <see cref="quaternion" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => (int)hash(value);

    /// <summary>Returns the string representation of the <see cref="quaternion" /> using default formatting.</summary>
    /// <returns>The string representation of the <see cref="quaternion" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return ToString("G", CultureInfo.CurrentCulture);
    }

    /// <summary>Returns a string representation of the <see cref="quaternion" /> using the specified format string to format individual elements and the specified format provider to define culture-specific formatting.</summary>
    /// <param name="format">A standard or custom numeric format string that defines the format of individual elements.</param>
    /// <param name="formatProvider">A format provider that supplies culture-specific formatting information.</param>
    /// <returns>The formatted string representation of the <see cref="quaternion" />.</returns>
    public readonly string ToString([StringSyntax(StringSyntaxAttribute.NumericFormat)] string? format, IFormatProvider? formatProvider)
    {
        var separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
        return $"quaternion({value.x.ToString(format, formatProvider)}f{separator} {value.y.ToString(format, formatProvider)}f{separator} {value.z.ToString(format, formatProvider)}f{separator} {value.w.ToString(format, formatProvider)}f)";
    }
}

public static partial class math
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion quaternion(float x, float y, float z, float w) => new(x, y, z, w);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion quaternion(float4 v) => new(v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion quaternion(float3x3 m) => new(m);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion quaternion(float4x4 m) => new(m);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion conjugate(quaternion q)
    {
        return quaternion(q.value * float4(-1.0f, -1.0f, -1.0f, 1.0f));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion inverse(quaternion q)
    {
        var x = q.value;
        return quaternion(rcp(dot(x, x)) * x * float4(-1.0f, -1.0f, -1.0f, 1.0f));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float dot(quaternion a, quaternion b)
    {
        return dot(a.value, b.value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float length(quaternion q)
    {
        return sqrt(dot(q.value, q.value));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float lengthsq(quaternion q)
    {
        return dot(q.value, q.value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion normalize(quaternion q)
    {
        var x = q.value;
        return quaternion(rsqrt(dot(x, x)) * x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion normalizesafe(quaternion q)
    {
        var x = q.value;
        var len = dot(x, x);
        return quaternion(select(Math.quaternion.identity.value, x * rsqrt(len), len > FLT_MIN_NORMAL));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion normalizesafe(quaternion q, quaternion defaultvalue)
    {
        var x = q.value;
        var len = dot(x, x);
        return quaternion(select(defaultvalue.value, x * rsqrt(len), len > FLT_MIN_NORMAL));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion unitexp(quaternion q)
    {
        var v_rcp_len = rsqrt(dot(q.value.xyz, q.value.xyz));
        var v_len = rcp(v_rcp_len);
        sincos(v_len, out var sin_v_len, out var cos_v_len);
        return quaternion(float4(q.value.xyz * v_rcp_len * sin_v_len, cos_v_len));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion exp(quaternion q)
    {
        var v_rcp_len = rsqrt(dot(q.value.xyz, q.value.xyz));
        var v_len = rcp(v_rcp_len);
        sincos(v_len, out var sin_v_len, out var cos_v_len);
        return quaternion(float4(q.value.xyz * v_rcp_len * sin_v_len, cos_v_len) * exp(q.value.w));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion unitlog(quaternion q)
    {
        var w = clamp(q.value.w, -1.0f, 1.0f);
        var s = acos(w) * rsqrt(1.0f - w * w);
        return quaternion(float4(q.value.xyz * s, 0.0f));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion log(quaternion q)
    {
        var v_len_sq = dot(q.value.xyz, q.value.xyz);
        var q_len_sq = v_len_sq + q.value.w*q.value.w;

        var s = acos(clamp(q.value.w * rsqrt(q_len_sq), -1.0f, 1.0f)) * rsqrt(v_len_sq);
        return quaternion(float4(q.value.xyz * s, 0.5f * log(q_len_sq)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion mul(quaternion a, quaternion b)
    {
        return quaternion(a.value.wwww * b.value + (a.value.xyzx * b.value.wwwx + a.value.yzxy * b.value.zxyy) * float4(1.0f, 1.0f, 1.0f, -1.0f) - a.value.zxyz * b.value.yzxz);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 mul(quaternion q, float3 v)
    {
        var t = 2 * cross(q.value.xyz, v);
        return v + q.value.w * t + cross(q.value.xyz, t);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 rotate(quaternion q, float3 v)
    {
        var t = 2 * cross(q.value.xyz, v);
        return v + q.value.w * t + cross(q.value.xyz, t);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion nlerp(quaternion q1, quaternion q2, float t)
    {
        var dt = dot(q1, q2);
        if(dt < 0.0f) q2.value = -q2.value;

        return normalize(quaternion(lerp(q1.value, q2.value, t)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static quaternion slerp(quaternion q1, quaternion q2, float t)
    {
        var dt = dot(q1, q2);
        if (dt < 0.0f)
        {
            dt = -dt;
            q2.value = -q2.value;
        }

        if (dt < 0.9995f)
        {
            var angle = acos(dt);
            var s = rsqrt(1.0f - dt * dt); // 1.0f / sin(angle)
            var w1 = sin(angle * (1.0f - t)) * s;
            var w2 = sin(angle * t) * s;
            return quaternion(q1.value * w1 + q2.value * w2);
        }

        // if the angle is small, use linear interpolation
        return nlerp(q1, q2, t);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint hash(quaternion q)
    {
        return hash(q.value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint4 hashwide(quaternion q)
    {
        return hashwide(q.value);
    }
}