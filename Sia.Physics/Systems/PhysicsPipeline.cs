namespace Sia.Physics;

public static class PhysicsPipeline
{
    public static SystemChain Default => SystemChain.Empty
        .Add<IntegratePhysicsSystem>()
        .Add<BuildPhysicsFrameSystem>()
        .Add<BroadphaseSystem>()
        .Add<NarrowphaseSystem>()
        .Add<SolveContactsSystem>()
        .Add<ExportPhysicsFrameSystem>();
}

