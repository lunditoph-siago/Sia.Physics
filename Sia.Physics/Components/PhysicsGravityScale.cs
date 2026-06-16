namespace Sia.Physics;

public readonly record struct PhysicsGravityScale(float Value)
{
    public static readonly PhysicsGravityScale Default = new(1f);
}

