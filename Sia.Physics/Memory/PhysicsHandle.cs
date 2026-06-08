namespace Sia.Physics;

public readonly record struct PhysicsHandle(int Index, uint Generation)
{
    public static readonly PhysicsHandle Invalid = new(-1, 0);

    public bool IsValid => Index >= 0 && Generation != 0;
}

