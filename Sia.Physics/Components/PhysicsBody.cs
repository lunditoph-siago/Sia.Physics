namespace Sia.Physics;

public readonly record struct PhysicsBody(
    ShapeHandle Shape,
    BodyMotionType MotionType,
    float InverseMass,
    float3 InverseInertia)
{
    public bool IsDynamic => MotionType == BodyMotionType.Dynamic;

    public static PhysicsBody Dynamic(ShapeHandle shape, in MassProperties mass) =>
        new(shape, BodyMotionType.Dynamic, mass.InverseMass, mass.InverseInertiaDiagonal);

    public static PhysicsBody Kinematic(ShapeHandle shape) =>
        new(shape, BodyMotionType.Kinematic, 0f, float3.zero);

    public static PhysicsBody Static(ShapeHandle shape) =>
        new(shape, BodyMotionType.Static, 0f, float3.zero);
}

