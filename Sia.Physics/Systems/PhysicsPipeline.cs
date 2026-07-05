namespace Sia.Physics;

public static class PhysicsPipeline
{
    public static SystemChain Default => SystemChain.Empty
        .Add<IntegratePhysicsSystem>()
        .Add<BuildPhysicsFrameSystem>()
        .Add<ApplyContinuousBoundsSystem>()
        .Add<BuildDistanceJointsSystem>()
        .Add<BroadphaseSystem>()
        .Add<NarrowphaseSystem>()
        .Add<DispatchContactsSystem>()
        .Add<SolveContactsSystem>()
        .Add<SolveDistanceJointsSystem>()
        .Add<ExportPhysicsFrameSystem>();
}
