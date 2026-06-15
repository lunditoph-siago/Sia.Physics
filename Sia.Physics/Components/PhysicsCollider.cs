namespace Sia.Physics;

public readonly record struct PhysicsCollider(CollisionFilter Filter, PhysicsMaterial Material)
{
    public static readonly PhysicsCollider Default = new(CollisionFilter.All, PhysicsMaterial.Default);
}

