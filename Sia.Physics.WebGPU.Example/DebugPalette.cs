namespace Sia.Physics.WebGPU.Example;

internal static class DebugPalette
{
    public static readonly float4 Ground = new(0.12f, 0.15f, 0.19f, 1f);
    public static readonly float4 Static = new(0.28f, 0.32f, 0.38f, 1f);
    public static readonly float4 Contact = new(1f, 0.22f, 0.12f, 1f);
    public static readonly float4 Ray = new(1f, 0.9f, 0.25f, 1f);
    public static readonly float4 Stack = new(0.25f, 0.62f, 1f, 1f);
    public static readonly float4 Shapes = new(1f, 0.48f, 0.2f, 1f);
    public static readonly float4 Continuous = new(0.88f, 0.34f, 0.88f, 1f);
    public static readonly float4 Joints = new(0.3f, 0.86f, 0.48f, 1f);
    public static readonly float4 Particles = new(0.18f, 0.78f, 0.92f, 1f);
    public static readonly float4 Queries = new(0.95f, 0.76f, 0.18f, 1f);

    public static float4 Shade(float4 color, int index)
    {
        var scale = 0.78f + index % 4 * 0.07f;
        return new(color.x * scale, color.y * scale, color.z * scale, color.w);
    }
}
