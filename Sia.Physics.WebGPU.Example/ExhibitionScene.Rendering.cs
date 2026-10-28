namespace Sia.Physics.WebGPU.Example;

internal sealed partial class ExhibitionScene
{
    public void BuildDebugMesh(DebugDrawList builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        foreach (var renderBody in _renderBodies.Values) {
            var pose = renderBody.Entity.Get<RigidTransform>();
            switch (renderBody.Shape.Kind) {
                case DemoShapeKind.Sphere:
                    builder.AddSphere(pose, renderBody.Shape.Size.x, renderBody.Color);
                    break;
                case DemoShapeKind.Box:
                    builder.AddBox(pose, renderBody.Shape.Size, renderBody.Color);
                    break;
                case DemoShapeKind.Capsule:
                    builder.AddCapsule(
                        pose,
                        renderBody.Shape.Size.x,
                        renderBody.Shape.Size.y,
                        renderBody.Color);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(renderBody.Shape));
            }
        }

        var frame = World.GetAddon<PhysicsFrame>();
        foreach (ref readonly var contact in frame.Contacts) {
            builder.AddMarker(contact.Point, 0.09f, DebugPalette.Contact);
        }

        AddParticleGeometry(builder);
        AddRegionGuides(builder);
        AddQuerySweep(builder);
        if (_rayDisplayTime > 0f) {
            builder.AddLine(_rayStart, _rayEnd, 0.035f, DebugPalette.Ray);
            if (_rayHit is { } hit) {
                builder.AddMarker(hit, 0.18f, DebugPalette.Contact);
            }
        }
    }

    private static void AddRegionGuides(DebugDrawList builder)
    {
        foreach (var region in Enum.GetValues<ExhibitionRegion>()) {
            var center = GetCenter(region);
            var color = GetColor(region);
            const float half = 6.65f;
            const float height = 0.035f;
            builder.AddLine(center + new float3(-half, height, -half), center + new float3(half, height, -half), 0.035f, color);
            builder.AddLine(center + new float3(half, height, -half), center + new float3(half, height, half), 0.035f, color);
            builder.AddLine(center + new float3(half, height, half), center + new float3(-half, height, half), 0.035f, color);
            builder.AddLine(center + new float3(-half, height, half), center + new float3(-half, height, -half), 0.035f, color);
        }
    }

    private void AddQuerySweep(DebugDrawList builder)
    {
        var center = GetCenter(ExhibitionRegion.Queries);
        var phase = _elapsedTime * 0.65f;
        var start = center + new float3(-5.5f, 1.1f + math.sin(phase) * 0.3f, -5.2f);
        var direction = math.normalize(new float3(1f, -0.04f, 0.72f + math.sin(phase * 0.7f) * 0.2f));
        var ray = new PhysicsRay(start, direction, 13f);
        var collector = new ClosestHitCollector(13f);
        PhysicsQueries.Raycast(World.GetAddon<PhysicsFrame>(), Shapes, ray, ref collector);
        var end = ray.GetPoint(collector.HasHit ? collector.Hit.Distance : 13f);
        builder.AddLine(start, end, 0.025f, DebugPalette.Queries);
        if (collector.HasHit) {
            builder.AddMarker(collector.Hit.Position, 0.13f, DebugPalette.Contact);
        }
    }
}
