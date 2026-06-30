namespace Sia.Physics;

public readonly record struct ParticleDamping(float Value)
{
    public static readonly ParticleDamping Default = new(0.01f);
}

