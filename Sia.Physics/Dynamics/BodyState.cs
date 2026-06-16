namespace Sia.Physics;

public struct BodyState
{
    public RigidTransform Pose;
    public PhysicsVelocity Velocity;
    public PhysicsBody Body;
    public PhysicsCollider Collider;

    public BodyState(
        in RigidTransform pose,
        in PhysicsVelocity velocity,
        in PhysicsBody body,
        in PhysicsCollider collider)
    {
        Pose = pose;
        Velocity = velocity;
        Body = body;
        Collider = collider;
    }
}

