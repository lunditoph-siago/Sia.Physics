namespace Sia.Physics.WebGPU.Example;

internal enum DemoShapeKind
{
    Sphere,
    Box,
    Capsule,
}

internal readonly record struct DemoShape(DemoShapeKind Kind, float3 Size)
{
    public static DemoShape Sphere(float radius) => new(DemoShapeKind.Sphere, new float3(radius));

    public static DemoShape Box(float3 halfExtents) => new(DemoShapeKind.Box, halfExtents);

    public static DemoShape Capsule(float radius, float halfLength) =>
        new(DemoShapeKind.Capsule, new float3(radius, halfLength, 0f));
}
