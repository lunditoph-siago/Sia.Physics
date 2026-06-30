namespace Sia.Physics;

public record struct ParticleVelocity(float3 Value)
{
    public static readonly ParticleVelocity Zero = new(float3.zero);
}

