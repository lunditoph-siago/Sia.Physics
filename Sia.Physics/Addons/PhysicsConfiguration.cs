namespace Sia.Physics;

public sealed class PhysicsConfiguration : IAddon
{
    private float _fixedDeltaTime = 1f / 60f;
    private int _solverIterations = 8;

    public float FixedDeltaTime
    {
        get => _fixedDeltaTime;
        set
        {
            if (!(value > 0f) || !float.IsFinite(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            _fixedDeltaTime = value;
        }
    }

    public int SolverIterations
    {
        get => _solverIterations;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            _solverIterations = value;
        }
    }

    public float3 Gravity { get; set; } = new(0f, -9.81f, 0f);
}

