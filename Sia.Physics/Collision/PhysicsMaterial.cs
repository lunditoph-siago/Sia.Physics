namespace Sia.Physics;

public readonly record struct PhysicsMaterial
{
    public static readonly PhysicsMaterial Default = new(0.5f);

    public PhysicsMaterial(
        float friction = 0.5f,
        float restitution = 0f,
        MaterialCombineMode frictionCombine = MaterialCombineMode.Average,
        MaterialCombineMode restitutionCombine = MaterialCombineMode.Maximum)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(friction);
        if (restitution is < 0f or > 1f)
        {
            throw new ArgumentOutOfRangeException(nameof(restitution));
        }

        Friction = friction;
        Restitution = restitution;
        FrictionCombine = frictionCombine;
        RestitutionCombine = restitutionCombine;
    }

    public float Friction { get; }

    public float Restitution { get; }

    public MaterialCombineMode FrictionCombine { get; }

    public MaterialCombineMode RestitutionCombine { get; }

    public static float Combine(float left, float right, MaterialCombineMode mode) => mode switch
    {
        MaterialCombineMode.Average => (left + right) * 0.5f,
        MaterialCombineMode.Minimum => MathF.Min(left, right),
        MaterialCombineMode.Maximum => MathF.Max(left, right),
        MaterialCombineMode.Multiply => left * right,
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };
}
