# Sia.Physics

Sia.Physics is a data-oriented physics layer for Sia ECS and Sia.Math. It targets .NET 11 and keeps persistent simulation state in ECS components while transient collision and constraint data is packed into aligned native frames.

## Capabilities

- sphere, box, capsule, and native-arena convex hull shapes
- temporally coherent sweep-and-prune broadphase with adaptive projection axes
- GJK intersection, EPA penetration recovery, and analytic sphere contacts
- sequential impulse contacts, compliant distance joints, filtering, and materials
- sphere time-of-impact continuous collision detection
- allocation-free ray and overlap collectors
- XPBD particles with distance constraints and native spatial-hash self-collision
- Sia dispatcher contact events

## Quick start

```csharp
using var world = new World();
var shapes = world.GetPhysicsShapes();
var sphere = shapes.Add(new SphereShape(0.5f));
var body = world.CreateDynamicBody(RigidTransform.Identity, sphere, density: 1f);
using var stage = PhysicsPipeline.Default.CreateStage(world);

stage.Tick();
var pose = body.Get<RigidTransform>();
```

Use `CreateContinuousBody` for fast spheres, `CreateDistanceJoint` for rigid constraints, and `ParticlePipeline.Default` for XPBD particles. Scene queries read the most recently built `PhysicsFrame` and return the originating Sia `Entity`.

## Validation

From this directory, using the workspace-local .NET 11 SDK:

```powershell
& '..\.dotnet\dotnet.exe' build Sia.Physics.slnx -c Release
& '..\.dotnet\dotnet.exe' test Sia.Physics.Tests/Sia.Physics.Tests.csproj -c Release
& '..\.dotnet\dotnet.exe' run --project Sia.Physics.Benchmarks -c Release
```
