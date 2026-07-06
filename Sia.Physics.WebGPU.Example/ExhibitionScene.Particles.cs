namespace Sia.Physics.WebGPU.Example;

internal sealed partial class ExhibitionScene
{
    private void BuildParticleRegion()
    {
        const int width = 15;
        const int height = 11;
        const float spacing = 0.48f;
        var region = ExhibitionRegion.Particles;
        var center = GetCenter(region);
        var particles = new global::Sia.Entity[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = y * width + x;
                var position = center + new float3(
                    (x - (width - 1) * 0.5f) * spacing,
                    7.1f - y * spacing,
                    math.sin(x * 0.55f) * 0.22f);
                var inverseMass = y == 0 && (x % 2 == 0 || x == width - 1) ? 0f : 1f;
                var velocity = new ParticleVelocity(new float3(
                    0f,
                    0f,
                    math.sin(x * 0.7f + y * 0.31f) * 0.35f));
                var entity = World.CreateParticle(
                    position,
                    inverseMass,
                    radius: 0.11f,
                    velocity,
                    damping: new ParticleDamping(0.22f));
                particles[index] = entity;
                TrackParticle(region, entity, 0.11f, DebugPalette.Shade(GetColor(region), x + y));
            }
        }

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = y * width + x;
                if (x + 1 < width)
                {
                    AddParticleLink(region, particles[index], particles[index + 1], spacing, 0.000004f);
                }
                if (y + 1 < height)
                {
                    AddParticleLink(region, particles[index], particles[index + width], spacing, 0.000004f);
                }
                if (x + 1 < width && y + 1 < height)
                {
                    AddParticleLink(
                        region,
                        particles[index],
                        particles[index + width + 1],
                        spacing * math.sqrt(2f),
                        0.00002f);
                }
                if (x > 0 && y + 1 < height)
                {
                    AddParticleLink(
                        region,
                        particles[index],
                        particles[index + width - 1],
                        spacing * math.sqrt(2f),
                        0.00002f);
                }
            }
        }
    }

    private void AddParticleLink(
        ExhibitionRegion region,
        global::Sia.Entity first,
        global::Sia.Entity second,
        float restLength,
        float compliance)
    {
        var constraint = World.CreateParticleDistanceConstraint(
            first,
            second,
            restLength,
            compliance);
        TrackEntity(region, constraint);
        _particleLinks.Add(new(first, second, region));
    }

    private void TrackParticle(
        ExhibitionRegion region,
        global::Sia.Entity entity,
        float radius,
        System.Numerics.Vector4 color)
    {
        TrackEntity(region, entity);
        _renderParticles.Add(entity.Id, new(entity, region, radius, color));
    }

    private void AddParticleGeometry(DebugMeshBuilder builder)
    {
        foreach (var particle in _renderParticles.Values)
        {
            var position = particle.Entity.Get<ParticlePosition>().Value;
            builder.AddSphere(
                RigidTransform.Translate(position),
                particle.Radius,
                particle.Color,
                longitudeSegments: 6,
                latitudeSegments: 4);
        }

        foreach (var link in _particleLinks)
        {
            var first = link.First.Get<ParticlePosition>().Value;
            var second = link.Second.Get<ParticlePosition>().Value;
            builder.AddLine(first, second, 0.018f, DebugPalette.Particles);
        }
    }
}
