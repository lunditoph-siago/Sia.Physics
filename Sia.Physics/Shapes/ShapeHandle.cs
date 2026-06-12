namespace Sia.Physics;

public readonly record struct ShapeHandle(ShapeType Type, int Index, uint Generation)
{
    public static readonly ShapeHandle Invalid = new(ShapeType.Sphere, -1, 0);

    public bool IsValid => Index >= 0 && Generation != 0;
}

