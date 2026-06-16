namespace Sia.Physics;

[SiaSystem]
public sealed class IntegratePhysicsSystem : ParallelSystemBase<
    RigidTransform,
    PhysicsVelocity,
    PhysicsBody,
    PhysicsDamping,
    PhysicsGravityScale>
{
    private PhysicsConfiguration _configuration = null!;

    public override void Initialize(World world)
    {
        _configuration = world.GetPhysicsConfiguration();
    }

    protected override void HandleSlice(
        in WorldContext context,
        ref RigidTransform pose,
        ref PhysicsVelocity velocity,
        ref PhysicsBody body,
        ref PhysicsDamping damping,
        ref PhysicsGravityScale gravityScale)
    {
        if (!body.IsDynamic)
        {
            return;
        }

        var deltaTime = _configuration.FixedDeltaTime;
        velocity.Linear += _configuration.Gravity * gravityScale.Value * deltaTime;
        velocity.Linear *= MathF.Exp(-damping.Linear * deltaTime);
        velocity.Angular *= MathF.Exp(-damping.Angular * deltaTime);
        pose.Translation += velocity.Linear * deltaTime;

        var angularSpeed = math.length(velocity.Angular);
        if (angularSpeed > 1e-8f)
        {
            var rotationDelta = quaternion.AxisAngle(velocity.Angular / angularSpeed, angularSpeed * deltaTime);
            pose.Rotation = math.normalize(math.mul(rotationDelta, pose.Rotation));
        }
    }
}

