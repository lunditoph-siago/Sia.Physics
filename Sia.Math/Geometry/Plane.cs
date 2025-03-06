using System.Diagnostics;

namespace Sia.Math;

[DebuggerDisplay("{Normal}, {Distance}")]
[Serializable]
public record struct Plane(float3 Normal, float Distance)
{
    public Plane Flipped => new(-Normal, -Distance);
}