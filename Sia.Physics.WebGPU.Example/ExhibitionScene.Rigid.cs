namespace Sia.Physics.WebGPU.Example;

internal sealed partial class ExhibitionScene
{
    private void BuildRegion(ExhibitionRegion region)
    {
        AddPlatform(region);
        switch (region)
        {
            case ExhibitionRegion.Stack:
                BuildStackRegion();
                break;
            case ExhibitionRegion.Shapes:
                BuildShapeRegion();
                break;
            case ExhibitionRegion.Continuous:
                BuildContinuousRegion();
                break;
            case ExhibitionRegion.Joints:
                BuildJointRegion();
                break;
            case ExhibitionRegion.Particles:
                break;
            case ExhibitionRegion.Queries:
                BuildQueryRegion();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(region));
        }
    }

    private void AddPlatform(ExhibitionRegion region)
    {
        var center = GetCenter(region);
        var color = GetColor(region);
        AddStatic(
            region,
            DemoShape.Box(new float3(6.7f, 0.25f, 6.7f)),
            RigidTransform.Translate(center + new float3(0f, -0.25f, 0f)),
            DebugPalette.Ground);
        AddStatic(
            region,
            DemoShape.Box(new float3(0.55f, 0.8f, 0.55f)),
            RigidTransform.Translate(center + new float3(-5.65f, 0.8f, -5.65f)),
            color);
    }

    private void BuildStackRegion()
    {
        var region = ExhibitionRegion.Stack;
        var center = GetCenter(region);
        var shape = DemoShape.Box(new float3(0.48f, 0.42f, 0.48f));
        const int levels = 8;
        for (var level = 0; level < levels; level++)
        {
            var count = levels - level;
            for (var index = 0; index < count; index++)
            {
                var position = center + new float3(
                    (index - (count - 1) * 0.5f) * 1.02f,
                    0.44f + level * 0.86f,
                    0f);
                var rotation = quaternion.RotateY((level & 1) * 0.018f);
                AddDynamic(
                    region,
                    shape,
                    new RigidTransform(rotation, position),
                    DebugPalette.Shade(GetColor(region), level));
            }
        }
    }

    private void BuildShapeRegion()
    {
        var region = ExhibitionRegion.Shapes;
        var center = GetCenter(region);
        DemoShape[] shapes =
        [
            DemoShape.Sphere(0.48f),
            DemoShape.Box(new float3(0.46f)),
            DemoShape.Capsule(0.32f, 0.55f),
        ];
        for (var index = 0; index < 27; index++)
        {
            var x = index % 3;
            var z = index / 3 % 3;
            var y = index / 9;
            var position = center + new float3(
                (x - 1f) * 1.45f,
                1.2f + y * 1.35f + (index % 2) * 0.18f,
                (z - 1f) * 1.45f);
            var rotation = quaternion.EulerXYZ(new float3(index * 0.13f, index * 0.21f, index * 0.08f));
            AddDynamic(
                region,
                shapes[index % shapes.Length],
                new RigidTransform(rotation, position),
                DebugPalette.Shade(GetColor(region), index));
        }
    }

    private void BuildContinuousRegion()
    {
        var region = ExhibitionRegion.Continuous;
        var center = GetCenter(region);
        var projectile = DemoShape.Sphere(0.28f);
        var target = DemoShape.Sphere(0.52f);
        for (var lane = -3; lane <= 3; lane++)
        {
            var z = lane * 1.45f;
            AddStatic(
                region,
                target,
                RigidTransform.Translate(center + new float3(3.2f, 0.85f, z)),
                DebugPalette.Static);
            AddDynamic(
                region,
                projectile,
                RigidTransform.Translate(center + new float3(-4.8f - System.Math.Abs(lane) * 0.18f, 0.85f, z)),
                DebugPalette.Shade(GetColor(region), lane + 3),
                new PhysicsVelocity(new float3(34f, 0f, 0f), float3.zero),
                continuous: true,
                density: 2f);
        }
    }

    private void BuildJointRegion()
    {
        var region = ExhibitionRegion.Joints;
        var center = GetCenter(region);
        var anchorShape = DemoShape.Sphere(0.34f);
        var linkShape = DemoShape.Box(new float3(0.36f, 0.62f, 0.36f));
        var previous = AddStatic(
            region,
            anchorShape,
            RigidTransform.Translate(center + new float3(0f, 7.2f, 0f)),
            DebugPalette.Static);
        for (var index = 0; index < 8; index++)
        {
            var current = AddDynamic(
                region,
                linkShape,
                RigidTransform.Translate(center + new float3(0.08f * index, 5.95f - index * 1.18f, 0f)),
                DebugPalette.Shade(GetColor(region), index),
                index == 7
                    ? new PhysicsVelocity(new float3(3.5f, 0f, 1.5f), float3.zero)
                    : default,
                density: 1.6f);
            var joint = World.CreateDistanceJoint(
                previous,
                current,
                1.18f,
                new float3(0f, -0.58f, 0f),
                new float3(0f, 0.58f, 0f),
                compliance: 0.00002f);
            TrackEntity(region, joint);
            previous = current;
        }
    }

    private void BuildQueryRegion()
    {
        var region = ExhibitionRegion.Queries;
        var center = GetCenter(region);
        for (var z = -2; z <= 2; z++)
        {
            for (var x = -2; x <= 2; x++)
            {
                var shape = ((x + z) & 1) == 0
                    ? DemoShape.Sphere(0.48f)
                    : DemoShape.Box(new float3(0.43f));
                AddStatic(
                    region,
                    shape,
                    RigidTransform.Translate(center + new float3(x * 1.55f, 0.62f, z * 1.55f)),
                    DebugPalette.Shade(GetColor(region), x + z + 4));
            }
        }
    }
}
