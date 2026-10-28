namespace Sia.Physics.WebGPU.Example;

internal readonly record struct RenderableParticle(
    global::Sia.Entity Entity,
    ExhibitionRegion Region,
    float Radius,
    float4 Color);

internal readonly record struct ParticleLink(
    global::Sia.Entity First,
    global::Sia.Entity Second,
    ExhibitionRegion Region);
