namespace Sia.Physics;

[SiaSystem]
[SiaAfter<BuildParticleFrameSystem>]
public sealed class BuildParticleConstraintsSystem : SystemBase
{
    private ParticleFrame _frame = null!;

    public BuildParticleConstraintsSystem() : base(Matchers.Of<ParticleDistanceConstraint>())
    {
    }

    public override void Initialize(World world)
    {
        _frame = world.AcquireAddon<ParticleFrame>();
    }

    public override void Execute(WorldContext context, IEntityQuery query)
    {
        foreach (var entity in query)
        {
            ref readonly var constraint = ref entity.Get<ParticleDistanceConstraint>();
            if (!_frame.TryGetParticleIndex(constraint.First, out var first) ||
                !_frame.TryGetParticleIndex(constraint.Second, out var second))
            {
                continue;
            }
            _frame.AddConstraint(new ParticleConstraintState
            {
                First = first,
                Second = second,
                RestLength = constraint.RestLength,
                Compliance = constraint.Compliance
            });
        }
    }
}

