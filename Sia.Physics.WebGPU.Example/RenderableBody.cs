namespace Sia.Physics.WebGPU.Example;

internal readonly record struct RenderableBody(
    global::Sia.Entity Entity,
    DemoShape Shape,
    ExhibitionRegion Region,
    float4 Color);
