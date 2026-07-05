namespace Sia.Physics.Tests;

public sealed class PhysicsRayTests
{
    [Fact]
    public void RejectsNonFiniteInputs()
    {
        Assert.Throws<ArgumentException>(() => new PhysicsRay(
            new float3(float.NaN, 0f, 0f),
            new float3(1f, 0f, 0f)));
        Assert.Throws<ArgumentException>(() => new PhysicsRay(
            float3.zero,
            new float3(float.PositiveInfinity, 0f, 0f)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new PhysicsRay(
            float3.zero,
            new float3(1f, 0f, 0f),
            float.NaN));
    }
}
