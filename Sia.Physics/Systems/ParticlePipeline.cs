namespace Sia.Physics;

public static class ParticlePipeline
{
    public static SystemChain Default => SystemChain.Empty
        .Add<IntegrateParticlesSystem>()
        .Add<BuildParticleFrameSystem>()
        .Add<BuildParticleConstraintsSystem>()
        .Add<SolveParticlesSystem>()
        .Add<ExportParticleFrameSystem>();
}

