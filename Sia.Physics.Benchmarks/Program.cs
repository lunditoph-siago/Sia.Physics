using System.Diagnostics;

namespace Sia.Physics.Benchmarks;

public static class Program
{
    private const int k_WarmupSteps = 16;
    private const int k_MeasuredSteps = 120;

    public static void Main()
    {
        Run("rigid pipeline", CreateRigidScene);
        Run("particle pipeline", CreateParticleScene);
        RunQueries();
    }

    private static void Run(string name, Func<(World World, SystemStage Stage)> create)
    {
        var (world, stage) = create();
        using (world)
        using (stage) {
            for (var i = 0; i < k_WarmupSteps; i++) {
                stage.Tick();
            }

            var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < k_MeasuredSteps; i++) {
                stage.Tick();
            }
            stopwatch.Stop();
            var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
            Console.WriteLine($"{name,-20} {stopwatch.Elapsed.TotalMilliseconds / k_MeasuredSteps,8:F3} ms/step " +
                $"{allocated / (double)k_MeasuredSteps,8:F1} B/step");
            if (world.TryGetAddon<PhysicsFrame>(out var frame)) {
                Console.WriteLine($"{string.Empty,-20} {frame.QueryIndex.BvhRebuildCount,8} BVH rebuilds");
            }
        }
    }

    private static void RunQueries()
    {
        using var world = new World();
        var shapes = world.GetPhysicsShapes();
        var sphere = shapes.Add(new SphereShape(0.45f));
        for (var i = 0; i < 4096; i++) {
            var x = i % 64;
            var y = i / 64;
            world.CreateStaticBody(RigidTransform.Translate(new float3(x, y, 0f) * 2f), sphere);
        }
        using var stage = SystemChain.Empty
            .Add<BuildPhysicsFrameSystem>()
            .Add<BuildPhysicsQueryIndexSystem>()
            .CreateStage(world);
        stage.Tick();

        const int queryCount = 4096;
        var frame = world.GetAddon<PhysicsFrame>();
        var checksum = 0;
        MeasureRaycasts("BVH raycast", frame, shapes, queryCount, ref checksum);
        MeasureOverlaps("sweep overlap", frame, queryCount, ref checksum);

        using var linearStage = SystemChain.Empty.Add<BuildPhysicsFrameSystem>().CreateStage(world);
        linearStage.Tick();
        MeasureRaycasts("linear raycast", frame, shapes, queryCount, ref checksum);
        MeasureOverlaps("linear overlap", frame, queryCount, ref checksum);
    }

    private static void MeasureRaycasts(
        string name,
        PhysicsFrame frame,
        PhysicsShapes shapes,
        int queryCount,
        ref int checksum)
    {
        for (var i = 0; i < 256; i++) {
            Raycast(frame, shapes, i, ref checksum);
        }

        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < queryCount; i++) {
            Raycast(frame, shapes, i, ref checksum);
        }
        stopwatch.Stop();
        var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        Console.WriteLine($"{name,-20} {stopwatch.Elapsed.TotalMicroseconds / queryCount,8:F3} us/query " +
            $"{allocated / (double)queryCount,8:F1} B/query ({checksum})");
    }

    private static void MeasureOverlaps(string name, PhysicsFrame frame, int queryCount, ref int checksum)
    {
        var overlapBounds = Aabb.CreateFromCenterAndHalfExtents(new float3(124f, 124f, 0f), new float3(2f));
        for (var i = 0; i < 256; i++) {
            Overlap(frame, overlapBounds, ref checksum);
        }

        var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        var stopwatch = Stopwatch.StartNew();
        for (var i = 0; i < queryCount; i++) {
            Overlap(frame, overlapBounds, ref checksum);
        }
        stopwatch.Stop();
        var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
        Console.WriteLine($"{name,-20} {stopwatch.Elapsed.TotalMicroseconds / queryCount,8:F3} us/query " +
            $"{allocated / (double)queryCount,8:F1} B/query ({checksum})");
    }

    private static void Raycast(PhysicsFrame frame, PhysicsShapes shapes, int queryIndex, ref int checksum)
    {
        var y = queryIndex % 64 * 2f;
        var ray = new PhysicsRay(new float3(-2f, y, 0f), new float3(1f, 0f, 0f), 132f);
        var collector = new ClosestHitCollector(ray.MaximumDistance);
        PhysicsQueries.Raycast(frame, shapes, ray, ref collector);
        checksum += collector.HasHit ? collector.Hit.BodyIndex : -1;
    }

    private static void Overlap(PhysicsFrame frame, in Aabb bounds, ref int checksum)
    {
        var collector = new AnyOverlapCollector();
        PhysicsQueries.OverlapAabb(frame, bounds, ref collector);
        checksum += collector.HasHit ? collector.Hit.BodyIndex : -1;
    }

    private static (World, SystemStage) CreateRigidScene()
    {
        var world = new World();
        world.GetPhysicsConfiguration().Gravity = float3.zero;
        var sphere = world.GetPhysicsShapes().Add(new SphereShape(0.45f));
        for (var i = 0; i < 2048; i++) {
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
        for (var i = 0; i < 4096; i++) {
            var x = i % 64;
            var y = i / 64;
            world.CreateParticle(new float3(x, y, 0f) * 0.22f, 1f, 0.1f);
        }
        return (world, ParticlePipeline.Default.CreateStage(world));
    }
}
