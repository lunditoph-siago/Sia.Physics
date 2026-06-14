namespace Sia.Physics;

public interface IConvexShape<TSelf> where TSelf : unmanaged, IConvexShape<TSelf>
{
    static abstract Aabb ComputeBounds(in TSelf shape, in RigidTransform pose);

    static abstract MassProperties ComputeMass(in TSelf shape, float density);

    static abstract float3 Support(in TSelf shape, float3 direction);
}
