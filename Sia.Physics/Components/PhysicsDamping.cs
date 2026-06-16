namespace Sia.Physics;

public readonly record struct PhysicsDamping(float Linear, float Angular)
{
    public static readonly PhysicsDamping Default = new(0.05f, 0.05f);
}

