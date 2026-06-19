namespace Sia.Physics.Tests;

public sealed class NarrowphaseTests
{
    [Fact]
    public void BuildsSphereContactFromBroadphasePair()
    {
        using var world = new World();
        var sphere = world.GetPhysicsShapes().Add(new SphereShape(1f));
        world.CreateStaticBody(RigidTransform.Identity, sphere);
        world.CreateDynamicBody(RigidTransform.Translate(new float3(1.5f, 0f, 0f)), sphere, 1f);

        using var stage = SystemChain.Empty
            .Add<NarrowphaseSystem>()
            .Add<BroadphaseSystem>()
            .Add<BuildPhysicsFrameSystem>()
            .CreateStage(world);
        stage.Tick();

        var contacts = world.GetAddon<PhysicsFrame>().Contacts;
        Assert.Single(contacts.ToArray());
        Assert.Equal(new float3(1f, 0f, 0f), contacts[0].Normal);
        Assert.Equal(0.5f, contacts[0].Penetration, 4);
    }
}

