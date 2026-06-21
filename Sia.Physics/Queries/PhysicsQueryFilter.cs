namespace Sia.Physics;

public readonly record struct PhysicsQueryFilter(CollisionFilter CollisionFilter)
{
    public static readonly PhysicsQueryFilter All = new(CollisionFilter.All);

    public bool Allows(in PhysicsCollider collider) =>
        CollisionFilter.Allows(CollisionFilter, collider.Filter);
}

