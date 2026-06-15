namespace Sia.Physics;

public record struct PhysicsVelocity(float3 Linear, float3 Angular)
{
    public static readonly PhysicsVelocity Zero = new(float3.zero, float3.zero);
}

