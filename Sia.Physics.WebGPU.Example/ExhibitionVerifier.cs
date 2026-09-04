namespace Sia.Physics.WebGPU.Example;

internal static class ExhibitionVerifier
{
    public static int Run(TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);
        try {
            using var scene = new ExhibitionScene();
            for (var step = 0; step < 120; step++) {
                scene.StepFixed();
                scene.ValidateState();
            }

            foreach (var region in Enum.GetValues<ExhibitionRegion>()) {
                var origin = ExhibitionScene.GetCenter(region) + new float3(0f, 18f, 0f);
                if (!scene.RaycastReset(origin, new float3(0f, -1f, 0f)) ||
                    scene.LastResetRegion != region) {
                    throw new InvalidOperationException(
                        $"The reset ray did not select {ExhibitionScene.GetName(region)}.");
                }
                scene.ValidateState();
            }

            var builder = new DebugDrawList();
            scene.BuildDebugMesh(builder);
            ValidateMesh(builder);
            output.WriteLine(
                $"PASS exhibition bodies={scene.BodyCount} particles={scene.ParticleCount} " +
                $"contacts={scene.ContactCount} triangles={builder.TriangleCount}");
            output.WriteLine("PASS raycast reset selected all six regions");
            return 0;
        }
        catch (Exception exception) {
            error.WriteLine($"FAIL {exception.GetType().Name}: {exception.Message}");
            return 1;
        }
    }

    private static void ValidateMesh(DebugDrawList builder)
    {
        if (builder.VertexCount == 0 || builder.VertexCount % 3 != 0) {
            throw new InvalidOperationException("The exhibition produced no valid triangle-list geometry.");
        }
        foreach (ref readonly var vertex in builder.Vertices) {
            if (!vertex.IsFinite) {
                throw new InvalidOperationException("The exhibition produced a non-finite debug vertex.");
            }
        }
    }
}
