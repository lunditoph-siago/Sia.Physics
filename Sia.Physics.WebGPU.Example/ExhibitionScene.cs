using System.Numerics;

namespace Sia.Physics.WebGPU.Example;

internal sealed partial class ExhibitionScene : IDisposable
{
    private const float k_FixedDeltaTime = 1f / 60f;

    private readonly Dictionary<DemoShape, ShapeHandle> _shapeHandles = [];
    private readonly Dictionary<global::Sia.EntityId, RenderableBody> _renderBodies = [];
    private readonly Dictionary<global::Sia.EntityId, RenderableParticle> _renderParticles = [];
    private readonly Dictionary<global::Sia.EntityId, ExhibitionRegion> _bodyRegions = [];
    private readonly Dictionary<ExhibitionRegion, List<global::Sia.Entity>> _regionEntities = [];
    private readonly List<ParticleLink> _particleLinks = [];
    private readonly global::Sia.SystemStage _physicsStage;
    private readonly global::Sia.SystemStage _particleStage;
    private double _accumulator;
    private float _elapsedTime;
    private bool _disposed;
    private float _rayDisplayTime;
    private float3 _rayStart;
    private float3 _rayEnd;
    private float3? _rayHit;

    public ExhibitionScene()
    {
        World = new global::Sia.World();
        Shapes = World.GetPhysicsShapes();
        var configuration = World.GetPhysicsConfiguration();
        configuration.FixedDeltaTime = k_FixedDeltaTime;
        configuration.Gravity = new float3(0f, -9.81f, 0f);
        configuration.SolverIterations = 10;

        foreach (var region in Enum.GetValues<ExhibitionRegion>()) {
            _regionEntities.Add(region, []);
            BuildRegion(region);
        }

        _physicsStage = PhysicsPipeline.Default.CreateStage(World);
        _particleStage = ParticlePipeline.Default.CreateStage(World);
        _physicsStage.Tick();
        _particleStage.Tick();
    }

    public global::Sia.World World { get; }

    public PhysicsShapes Shapes { get; }

    public int BodyCount => _renderBodies.Count;

    public int ParticleCount => _renderParticles.Count;

    public int ContactCount => World.GetAddon<PhysicsFrame>().Contacts.Length;

    public ExhibitionRegion? LastResetRegion { get; private set; }

    public string LastResetName => LastResetRegion is { } region ? GetName(region) : "none";

    public void Advance(float frameDeltaTime)
    {
        var clampedDelta = System.Math.Clamp(frameDeltaTime, 0f, 0.1f);
        _elapsedTime += clampedDelta;
        _rayDisplayTime = System.Math.Max(0f, _rayDisplayTime - clampedDelta);
        _accumulator += clampedDelta;
        var stepCount = 0;
        while (_accumulator >= k_FixedDeltaTime && stepCount < 6) {
            StepFixed();
            _accumulator -= k_FixedDeltaTime;
            stepCount++;
        }

        if (stepCount == 6) {
            _accumulator = 0;
        }
    }

    public void StepFixed()
    {
        _physicsStage.Tick();
        _particleStage.Tick();
    }

    public bool RaycastReset(float3 origin, float3 direction, float maximumDistance = 120f)
    {
        var ray = new PhysicsRay(origin, direction, maximumDistance);
        var collector = new ClosestHitCollector(maximumDistance);
        PhysicsQueries.Raycast(World.GetAddon<PhysicsFrame>(), Shapes, ray, ref collector);
        _rayStart = origin;
        _rayEnd = ray.GetPoint(collector.HasHit ? collector.Hit.Distance : maximumDistance);
        _rayHit = collector.HasHit ? collector.Hit.Position : null;
        _rayDisplayTime = 1.25f;
        if (!collector.HasHit || !_bodyRegions.TryGetValue(collector.Hit.Entity.Id, out var region)) {
            return false;
        }

        ResetRegion(region);
        LastResetRegion = region;
        return true;
    }

    public void ResetAll()
    {
        foreach (var region in Enum.GetValues<ExhibitionRegion>()) {
            ResetRegion(region, tickAfterReset: false);
        }
        _physicsStage.Tick();
        _particleStage.Tick();
        LastResetRegion = null;
    }

    public void ValidateState()
    {
        foreach (var body in _renderBodies.Values) {
            var pose = body.Entity.Get<RigidTransform>();
            if (!math.all(math.isfinite(pose.Translation)) || !math.isfinite(pose.Rotation)) {
                throw new InvalidOperationException($"Body {body.Entity} has a non-finite pose.");
            }
        }
        foreach (var particle in _renderParticles.Values) {
            var position = particle.Entity.Get<ParticlePosition>().Value;
            if (!math.all(math.isfinite(position))) {
                throw new InvalidOperationException($"Particle {particle.Entity} has a non-finite position.");
            }
        }
    }

    public static float3 GetCenter(ExhibitionRegion region) => region switch {
        ExhibitionRegion.Stack => new float3(-15f, 0f, -12f),
        ExhibitionRegion.Shapes => new float3(0f, 0f, -12f),
        ExhibitionRegion.Continuous => new float3(15f, 0f, -12f),
        ExhibitionRegion.Joints => new float3(-15f, 0f, 12f),
        ExhibitionRegion.Particles => new float3(0f, 0f, 12f),
        ExhibitionRegion.Queries => new float3(15f, 0f, 12f),
        _ => throw new ArgumentOutOfRangeException(nameof(region)),
    };

    public static string GetName(ExhibitionRegion region) => region switch {
        ExhibitionRegion.Stack => "stack stability",
        ExhibitionRegion.Shapes => "mixed shapes",
        ExhibitionRegion.Continuous => "continuous collision",
        ExhibitionRegion.Joints => "distance joints",
        ExhibitionRegion.Particles => "XPBD particles",
        ExhibitionRegion.Queries => "spatial queries",
        _ => throw new ArgumentOutOfRangeException(nameof(region)),
    };

    public static Vector4 GetColor(ExhibitionRegion region) => region switch {
        ExhibitionRegion.Stack => DebugPalette.Stack,
        ExhibitionRegion.Shapes => DebugPalette.Shapes,
        ExhibitionRegion.Continuous => DebugPalette.Continuous,
        ExhibitionRegion.Joints => DebugPalette.Joints,
        ExhibitionRegion.Particles => DebugPalette.Particles,
        ExhibitionRegion.Queries => DebugPalette.Queries,
        _ => throw new ArgumentOutOfRangeException(nameof(region)),
    };

    public void Dispose()
    {
        if (_disposed) {
            return;
        }
        _disposed = true;
        _particleStage.Dispose();
        _physicsStage.Dispose();
        World.Dispose();
    }

    private void ResetRegion(ExhibitionRegion region, bool tickAfterReset = true)
    {
        var entities = _regionEntities[region];
        foreach (var entity in entities) {
            _renderBodies.Remove(entity.Id);
            _renderParticles.Remove(entity.Id);
            _bodyRegions.Remove(entity.Id);
            entity.Destroy();
        }
        _particleLinks.RemoveAll(link => link.Region == region);
        entities.Clear();
        BuildRegion(region);
        if (tickAfterReset) {
            _physicsStage.Tick();
            _particleStage.Tick();
        }
    }

    private global::Sia.Entity AddStatic(
        ExhibitionRegion region,
        DemoShape shape,
        in RigidTransform pose,
        Vector4 color)
    {
        var entity = World.CreateStaticBody(pose, GetShapeHandle(shape));
        TrackBody(region, entity, shape, color);
        return entity;
    }

    private global::Sia.Entity AddDynamic(
        ExhibitionRegion region,
        DemoShape shape,
        in RigidTransform pose,
        Vector4 color,
        PhysicsVelocity velocity = default,
        bool continuous = false,
        float density = 1f)
    {
        var handle = GetShapeHandle(shape);
        var damping = new PhysicsDamping(0.04f, 0.08f);
        var entity = continuous
            ? World.CreateContinuousBody(pose, handle, density, velocity, damping: damping)
            : World.CreateDynamicBody(pose, handle, density, velocity, damping: damping);
        TrackBody(region, entity, shape, color);
        return entity;
    }

    private void TrackBody(
        ExhibitionRegion region,
        global::Sia.Entity entity,
        DemoShape shape,
        Vector4 color)
    {
        _regionEntities[region].Add(entity);
        _renderBodies.Add(entity.Id, new(entity, shape, region, color));
        _bodyRegions.Add(entity.Id, region);
    }

    private void TrackEntity(ExhibitionRegion region, global::Sia.Entity entity) =>
        _regionEntities[region].Add(entity);

    private ShapeHandle GetShapeHandle(DemoShape shape)
    {
        if (_shapeHandles.TryGetValue(shape, out var handle)) {
            return handle;
        }

        handle = shape.Kind switch {
            DemoShapeKind.Sphere => Shapes.Add(new SphereShape(shape.Size.x)),
            DemoShapeKind.Box => Shapes.Add(new BoxShape(shape.Size)),
            DemoShapeKind.Capsule => Shapes.Add(new CapsuleShape(shape.Size.x, shape.Size.y)),
            _ => throw new ArgumentOutOfRangeException(nameof(shape)),
        };
        _shapeHandles.Add(shape, handle);
        return handle;
    }
}
