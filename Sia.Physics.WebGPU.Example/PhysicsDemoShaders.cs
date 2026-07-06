namespace Sia.Physics.WebGPU.Example;

internal static class PhysicsDemoShaders
{
    public const string Source = """
        struct CameraUniforms {
            view_projection: mat4x4<f32>,
            eye_position: vec4<f32>,
        };

        @group(0) @binding(0)
        var<uniform> camera: CameraUniforms;

        struct VertexInput {
            @location(0) position: vec3<f32>,
            @location(1) normal: vec3<f32>,
            @location(2) color: vec4<f32>,
        };

        struct VertexOutput {
            @builtin(position) clip_position: vec4<f32>,
            @location(0) world_position: vec3<f32>,
            @location(1) normal: vec3<f32>,
            @location(2) color: vec4<f32>,
        };

        @vertex
        fn vs_main(input: VertexInput) -> VertexOutput {
            var output: VertexOutput;
            output.clip_position = camera.view_projection * vec4<f32>(input.position, 1.0);
            output.world_position = input.position;
            output.normal = normalize(input.normal);
            output.color = input.color;
            return output;
        }

        @fragment
        fn fs_main(input: VertexOutput) -> @location(0) vec4<f32> {
            let sun = normalize(vec3<f32>(0.45, 0.82, 0.34));
            let diffuse = 0.22 + 0.78 * abs(dot(normalize(input.normal), sun));
            let distance_fade = 1.0 / (1.0 + 0.0012 * distance(camera.eye_position.xyz, input.world_position));
            let horizon = 0.92 + 0.08 * clamp(input.normal.y, 0.0, 1.0);
            return vec4<f32>(input.color.rgb * diffuse * distance_fade * horizon, input.color.a);
        }
        """;
}
