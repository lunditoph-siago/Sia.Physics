namespace Sia.Physics;

internal readonly record struct BvhNode(int Index, int Count)
{
    public bool IsLeaf => Count != 0;
}
