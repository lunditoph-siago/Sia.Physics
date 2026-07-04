using System.Diagnostics;

namespace Sia.Physics.Benchmarks;

public static class Program
{
    private const int WarmupSteps = 16;
    private const int MeasuredSteps = 120;

    public static void Main()
    {
        Run("rigid pipeline", CreateRigidScene);
        Run("particle pipeline", CreateParticleScene);
    }

    private static void Run(string name, Func<(World World, SystemStage Stage)> create)
    {
        var (world, stage) = create();
        using (world)
        using (stage)
        {
            for (var i = 0; i < WarmupSteps; i++)
            {
                stage.Tick();
            }

            var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < MeasuredSteps; i++)
            {
                stage.Tick();
            }
            stopwatch.Stop();
            var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
            Console.WriteLine($"{name,-20} {stopwatch.Elapsed.TotalMilliseconds / MeasuredSteps,8:F3} ms/step " +
                $"{allocated / (double)MeasuredSteps,8:F1} B/step");
        }
    }

    private static (World, SystemStage) CreateRigidScene()
    {
        var world = new World();
        world.GetPhysicsConfiguration().Gravity = float3.zero;
        var sphere = world.GetPhysicsShapes().Add(new SphereShape(0.45f));
        for (var i = 0; i < 2048; i++)
        {
            var x = i % 32;
            var y = (i / 32) % 16;
            var z = i / 512;
            world.CreateDynamicBody(
                RigidTransform.Translate(new float3(x, y, z) * 1.05f),
                sphere,
                1f,
                damping: new PhysicsDamping(0f, 0f));
        }
        return (world, PhysicsPipeline.Default.CreateStage(world));
    }

    private static (World, SystemStage) CreateParticleScene()
    {
        var world = new World();
        world.GetPhysicsConfiguration().Gravity = float3.zero;
        for (var i = 0; i < 4096; i++)
        {
            var x = i % 64;
            var y = i / 64;
            world.CreateParticle(new float3(x, y, 0f) * 0.22f, 1f, 0.1f);
        }
        return (world, ParticlePipeline.Default.CreateStage(world));
    }
}

