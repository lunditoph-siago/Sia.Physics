namespace Sia.Physics;

public static class WorldPhysicsExtensions
{
    public static PhysicsShapes GetPhysicsShapes(this World world) =>
        world.AcquireAddon<PhysicsShapes>();

    public static PhysicsConfiguration GetPhysicsConfiguration(this World world) =>
        world.AcquireAddon<PhysicsConfiguration>();

    public static Entity CreateDynamicBody(
        this World world,
        in RigidTransform pose,
        ShapeHandle shape,
        float density,
        PhysicsVelocity velocity = default,
        PhysicsCollider? collider = null,
        PhysicsDamping? damping = null,
        PhysicsGravityScale? gravityScale = null)
    {
        var mass = world.GetPhysicsShapes().ComputeMass(shape, density);
        return world.CreateUnmanaged(HList.From(
            pose,
            velocity,
            PhysicsBody.Dynamic(shape, mass),
            collider ?? PhysicsCollider.Default,
            damping ?? PhysicsDamping.Default,
            gravityScale ?? PhysicsGravityScale.Default,
            new PhysicsPreviousPose(pose)));
    }

    public static Entity CreateContinuousBody(
        this World world,
        in RigidTransform pose,
        ShapeHandle shape,
        float density,
        PhysicsVelocity velocity = default,
        PhysicsCollider? collider = null,
        PhysicsDamping? damping = null,
        PhysicsGravityScale? gravityScale = null)
    {
        var mass = world.GetPhysicsShapes().ComputeMass(shape, density);
        return world.CreateUnmanaged(HList.From(
            pose,
            velocity,
            PhysicsBody.Dynamic(shape, mass),
            collider ?? PhysicsCollider.Default,
            damping ?? PhysicsDamping.Default,
            gravityScale ?? PhysicsGravityScale.Default,
            new PhysicsPreviousPose(pose),
            new ContinuousCollision()));
    }

    public static Entity CreateStaticBody(
        this World world,
        in RigidTransform pose,
        ShapeHandle shape,
        PhysicsCollider? collider = null) => world.CreateUnmanaged(HList.From(
            pose,
            PhysicsBody.Static(shape),
            collider ?? PhysicsCollider.Default));

    public static Entity CreateDistanceJoint(
        this World world,
        Entity first,
        Entity second,
        float restLength,
        float3 localAnchorA = default,
        float3 localAnchorB = default,
        float compliance = 0f) => world.CreateUnmanaged(HList.From(new DistanceJoint(
            first.Id,
            second.Id,
            restLength,
            localAnchorA,
            localAnchorB,
            compliance)));

    public static Entity CreateParticle(
        this World world,
        float3 position,
        float inverseMass,
        float radius,
        ParticleVelocity velocity = default,
        ParticleDamping? damping = null,
        ParticleGravityScale? gravityScale = null) => world.CreateUnmanaged(HList.From(
            new ParticlePosition(position),
            velocity,
            new PhysicsParticle(inverseMass, radius),
            damping ?? ParticleDamping.Default,
            gravityScale ?? ParticleGravityScale.Default));
}
