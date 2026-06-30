namespace Sia.Physics;

public readonly record struct ParticleGravityScale(float Value)
{
    public static readonly ParticleGravityScale Default = new(1f);
}

