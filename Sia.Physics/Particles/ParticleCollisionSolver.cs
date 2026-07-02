namespace Sia.Physics;

internal static class ParticleCollisionSolver
{
    public static void Solve(ParticleFrame frame, ParticleSpatialHash spatialHash)
    {
        spatialHash.Build(frame);
        var particles = frame.Particles;
        var entries = spatialHash.Entries;
        for (var firstIndex = 0; firstIndex < particles.Length; firstIndex++)
        {
            var cell = spatialHash.CellOf(particles[firstIndex].Position);
            for (var z = -1; z <= 1; z++)
            {
                for (var y = -1; y <= 1; y++)
                {
                    for (var x = -1; x <= 1; x++)
                    {
                        var neighbor = cell + new int3(x, y, z);
                        spatialHash.FindRange(ParticleSpatialHash.Hash(neighbor), out var start, out var end);
                        for (var entryIndex = start; entryIndex < end; entryIndex++)
                        {
                            var secondIndex = entries[entryIndex].ParticleIndex;
                            if (secondIndex <= firstIndex)
                            {
                                continue;
                            }
                            Resolve(ref particles[firstIndex], ref particles[secondIndex]);
                        }
                    }
                }
            }
        }
    }

    private static void Resolve(ref ParticleState first, ref ParticleState second)
    {
        var separation = second.Position - first.Position;
        var distanceSquared = math.lengthsq(separation);
        var radius = first.Radius + second.Radius;
        if (distanceSquared >= radius * radius)
        {
            return;
        }

        var inverseMass = first.InverseMass + second.InverseMass;
        if (inverseMass <= 1e-12f)
        {
            return;
        }
        var distance = MathF.Sqrt(distanceSquared);
        var normal = distance > 1e-10f ? separation / distance : new float3(1f, 0f, 0f);
        var correction = normal * ((radius - distance) / inverseMass);
        first.Position -= correction * first.InverseMass;
        second.Position += correction * second.InverseMass;
    }
}

