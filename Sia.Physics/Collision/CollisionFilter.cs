namespace Sia.Physics;

public readonly record struct CollisionFilter(uint BelongsTo, uint CollidesWith, int Group = 0)
{
    public static readonly CollisionFilter All = new(uint.MaxValue, uint.MaxValue);

    public static bool Allows(in CollisionFilter left, in CollisionFilter right)
    {
        if (left.Group != 0 && left.Group == right.Group)
        {
            return left.Group > 0;
        }

        return (left.BelongsTo & right.CollidesWith) != 0 &&
            (right.BelongsTo & left.CollidesWith) != 0;
    }
}

