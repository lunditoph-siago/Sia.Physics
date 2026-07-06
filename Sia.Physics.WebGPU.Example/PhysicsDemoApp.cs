using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Sia.GLFW;
using Sia.Input;
using Sia.Window;
using global::Sia.WebGPU;

namespace Sia.Physics.WebGPU.Example;

internal sealed unsafe class PhysicsDemoApp : IDisposable
{
    private const int _initialWidth = 1280;
    private const int _initialHeight = 800;
    private const ulong _uniformSize = 80;
    private const WGPUTextureFormat _depthFormat = WGPUTextureFormat.Depth24Plus;

    private readonly HashSet<Key> _pressedKeys = [];
    private readonly HashSet<MouseButton> _pressedMouseButtons = [];
    private readonly DebugMeshBuilder _meshBuilder = new();
    private readonly int? _maximumFrameCount;
    private readonly bool _windowVisible;

    private ExhibitionScene? _scene;
    private GlfwWindow _window;
    private WgpuHandle<WGPUInstance> _instance;
    private WgpuHandle<WGPUSurface> _surface;
    private WgpuHandle<WGPUAdapter> _adapter;
    private WgpuHandle<WGPUDevice> _device;
    private WgpuHandle<WGPUQueue> _queue;
    private WgpuHandle<WGPUBuffer> _uniformBuffer;
    private WgpuHandle<WGPUBuffer> _vertexBuffer;
    private WgpuHandle<WGPUBindGroupLayout> _bindGroupLayout;
    private WgpuHandle<WGPUBindGroup> _bindGroup;
    private WgpuHandle<WGPUPipelineLayout> _pipelineLayout;
    private WgpuHandle<WGPURenderPipeline> _pipeline;
    private WgpuHandle<WGPUTexture> _depthTexture;
    private WgpuHandle<WGPUTextureView> _depthView;

    private WGPUTextureFormat _surfaceFormat;
    private WGPUCompositeAlphaMode _alphaMode;
    private WGPUPresentMode _presentMode;
    private ulong _vertexBufferCapacity;
    private int _framebufferWidth;
    private int _framebufferHeight;
    private bool _paused;
    private bool _looking;
    private bool _glfwInitialized;
    private bool _surfaceConfigured;
    private bool _disposed;
    private float _cameraYaw = math.PI;
    private float _cameraPitch = -0.16f;
    private float3 _cameraPosition = new(0f, 8f, 34f);
    private double _lastCursorX;
    private double _lastCursorY;
    private double _lastTitleUpdate;

    public PhysicsDemoApp(
        int? maximumFrameCount = null,
        bool visible = true)
    {
        if (maximumFrameCount is <= 0) {
            throw new ArgumentOutOfRangeException(nameof(maximumFrameCount));
        }
        _maximumFrameCount = maximumFrameCount;
        _windowVisible = visible;
    }

    public void Run()
    {
        Initialize();
        PrintControls();

        var clock = Stopwatch.StartNew();
        var previousTime = clock.Elapsed.TotalSeconds;
        var renderedFrameCount = 0;
        while (!Glfw.ShouldClose(_window)) {
            Glfw.PollEvents();
            var currentTime = clock.Elapsed.TotalSeconds;
            var deltaTime = (float)System.Math.Min(currentTime - previousTime, 0.1);
            previousTime = currentTime;

            HandleInput(deltaTime);
            if (!_paused) {
                _scene!.Advance(deltaTime);
            }
            if (!ResizeIfNeeded()) {
                Thread.Sleep(16);
                continue;
            }

            _meshBuilder.Clear();
            _scene!.BuildDebugMesh(_meshBuilder);
            UploadGeometry();
            WriteUniforms();
            RenderFrame();
            renderedFrameCount++;
            Wgpu.ProcessEvents(_instance);
            UpdateWindowTitle(currentTime);
            if (_maximumFrameCount is { } maximum
                && renderedFrameCount >= maximum) {
                break;
            }
        }
    }

    private void Initialize()
    {
        Glfw.Initialize();
        _glfwInitialized = true;
        _window = Glfw.CreateWindow(
            new WindowDescriptor(
                _initialWidth,
                _initialHeight,
                "Sia.Physics · WebGPU Exhibition",
                Visible: _windowVisible,
                Resizable: true),
            new GlfwWindowOptions(ClientApi.NoApi));

        _instance = Wgpu.CreateInstance();
        _surface = CreateSurface(_instance, _window);
        var adapterOptions = new WGPURequestAdapterOptions {
            NextInChain = null,
            FeatureLevel = WGPUFeatureLevel.Core,
            PowerPreference = WGPUPowerPreference.HighPerformance,
            ForceFallbackAdapter = 0,
            BackendType = WGPUBackendType.Undefined,
            CompatibleSurface = Pointer(_surface),
        };
        _adapter = Wgpu.RequestAdapter(_instance, in adapterOptions);
        var surfaceInfo = GetSurfaceInfo(_surface, _adapter);
        _surfaceFormat = surfaceInfo.Format;
        _alphaMode = surfaceInfo.AlphaMode;
        _presentMode = surfaceInfo.PresentMode;
        _device = Wgpu.RequestDevice(_adapter);
        _queue = Wgpu.GetQueue(_device);

        CreateUniformResources();
        CreatePipeline();
        _scene = new ExhibitionScene();
        ResizeIfNeeded(force: true);
    }

    private static WgpuHandle<WGPUSurface> CreateSurface(
        WgpuHandle<WGPUInstance> instance,
        GlfwWindow window)
    {
        if (OperatingSystem.IsWindows()) {
            return Wgpu.CreateWindowsSurface(
                instance,
                GlfwPlatformNative.GetCurrentWin32ModuleHandle(),
                Glfw.GetWin32Window(window),
                "Sia.Physics cases surface");
        }
        if (OperatingSystem.IsLinux()) {
            var waylandDisplay = Glfw.GetWaylandDisplay();
            if (waylandDisplay != 0) {
                return Wgpu.CreateWaylandSurface(
                    instance,
                    waylandDisplay,
                    Glfw.GetWaylandWindow(window),
                    "Sia.Physics cases surface");
            }
            return Wgpu.CreateXlibSurface(
                instance,
                Glfw.GetX11Display(),
                (ulong)Glfw.GetX11Window(window),
                "Sia.Physics cases surface");
        }
        throw new PlatformNotSupportedException(
            "The example currently creates WebGPU surfaces for Win32, X11, and Wayland.");
    }

    private static SurfaceInfo GetSurfaceInfo(
        WgpuHandle<WGPUSurface> surface,
        WgpuHandle<WGPUAdapter> adapter)
    {
        var capabilities = default(WGPUSurfaceCapabilities);
        var status = WgpuUnsafe.wgpuSurfaceGetCapabilities(
            Pointer(surface),
            Pointer(adapter),
            &capabilities);
        if (status != WGPUStatus.Success) {
            throw new WgpuException($"Surface capability query failed with status {status}.");
        }

        try {
            if ((capabilities.Usages & WGPUTextureUsage.RenderAttachment) == 0
                || capabilities.FormatCount == 0) {
                throw new WgpuException("The selected surface cannot be used as a color render target.");
            }
            return new(
                PickSurfaceFormat(in capabilities),
                PickAlphaMode(in capabilities),
                PickPresentMode(in capabilities));
        }
        finally {
            WgpuUnsafe.wgpuSurfaceCapabilitiesFreeMembers(capabilities);
        }
    }

    private static WGPUTextureFormat PickSurfaceFormat(
        in WGPUSurfaceCapabilities capabilities)
    {
        ReadOnlySpan<WGPUTextureFormat> preferred = [
            WGPUTextureFormat.BGRA8UnormSrgb,
            WGPUTextureFormat.RGBA8UnormSrgb,
            WGPUTextureFormat.BGRA8Unorm,
            WGPUTextureFormat.RGBA8Unorm,
        ];
        foreach (var candidate in preferred) {
            for (nuint index = 0; index < capabilities.FormatCount; index++) {
                if (capabilities.Formats[index] == candidate) {
                    return candidate;
                }
            }
        }
        return capabilities.Formats[0];
    }

    private static WGPUCompositeAlphaMode PickAlphaMode(
        in WGPUSurfaceCapabilities capabilities)
    {
        for (nuint index = 0; index < capabilities.AlphaModeCount; index++) {
            if (capabilities.AlphaModes[index] == WGPUCompositeAlphaMode.Opaque) {
                return WGPUCompositeAlphaMode.Opaque;
            }
        }
        return capabilities.AlphaModeCount == 0
            ? WGPUCompositeAlphaMode.Auto
            : capabilities.AlphaModes[0];
    }

    private static WGPUPresentMode PickPresentMode(
        in WGPUSurfaceCapabilities capabilities)
    {
        for (nuint index = 0; index < capabilities.PresentModeCount; index++) {
            if (capabilities.PresentModes[index] == WGPUPresentMode.Fifo) {
                return WGPUPresentMode.Fifo;
            }
        }
        return capabilities.PresentModeCount == 0
            ? WGPUPresentMode.Fifo
            : capabilities.PresentModes[0];
    }

    private void CreateUniformResources()
    {
        var bufferDescriptor = new WGPUBufferDescriptor {
            NextInChain = null,
            Label = default,
            Usage = WGPUBufferUsage.Uniform | WGPUBufferUsage.CopyDst,
            Size = _uniformSize,
            MappedAtCreation = 0,
        };
        _uniformBuffer = Wgpu.CreateBuffer(_device, in bufferDescriptor);

        var layoutEntry = new WGPUBindGroupLayoutEntry {
            NextInChain = null,
            Binding = 0,
            Visibility = WGPUShaderStage.Vertex | WGPUShaderStage.Fragment,
            BindingArraySize = 0,
            Buffer = new WGPUBufferBindingLayout {
                NextInChain = null,
                Type = WGPUBufferBindingType.Uniform,
                HasDynamicOffset = 0,
                MinBindingSize = _uniformSize,
            },
            Sampler = default,
            Texture = default,
            StorageTexture = default,
        };
        var layoutDescriptor = new WGPUBindGroupLayoutDescriptor {
            NextInChain = null,
            Label = default,
            EntryCount = 1,
            Entries = &layoutEntry,
        };
        _bindGroupLayout = Wgpu.CreateBindGroupLayout(_device, in layoutDescriptor);

        var bindEntry = new WGPUBindGroupEntry {
            NextInChain = null,
            Binding = 0,
            Buffer = Pointer(_uniformBuffer),
            Offset = 0,
            Size = _uniformSize,
            Sampler = null,
            TextureView = null,
        };
        var bindDescriptor = new WGPUBindGroupDescriptor {
            NextInChain = null,
            Label = default,
            Layout = Pointer(_bindGroupLayout),
            EntryCount = 1,
            Entries = &bindEntry,
        };
        _bindGroup = Wgpu.CreateBindGroup(_device, in bindDescriptor);
    }

    private void CreatePipeline()
    {
        var layouts = stackalloc WGPUBindGroupLayout*[1];
        layouts[0] = Pointer(_bindGroupLayout);
        var pipelineLayoutDescriptor = new WGPUPipelineLayoutDescriptor {
            NextInChain = null,
            Label = default,
            BindGroupLayoutCount = 1,
            BindGroupLayouts = layouts,
            ImmediateSize = 0,
        };
        _pipelineLayout = Wgpu.CreatePipelineLayout(_device, in pipelineLayoutDescriptor);

        var shader = Wgpu.CreateWgslShaderModule(
            _device,
            PhysicsDemoShaders.Source,
            "Sia.Physics debug shader");
        try {
            using var vertexEntryPoint = new NativeString("vs_main");
            using var fragmentEntryPoint = new NativeString("fs_main");
            var attributes = stackalloc WGPUVertexAttribute[3];
            attributes[0] = new() {
                NextInChain = null,
                Format = WGPUVertexFormat.Float32x3,
                Offset = 0,
                ShaderLocation = 0,
            };
            attributes[1] = new() {
                NextInChain = null,
                Format = WGPUVertexFormat.Float32x3,
                Offset = 12,
                ShaderLocation = 1,
            };
            attributes[2] = new() {
                NextInChain = null,
                Format = WGPUVertexFormat.Float32x4,
                Offset = 24,
                ShaderLocation = 2,
            };
            var vertexBufferLayout = new WGPUVertexBufferLayout {
                NextInChain = null,
                StepMode = WGPUVertexStepMode.Vertex,
                ArrayStride = DebugMeshBuilder.VertexStride,
                AttributeCount = 3,
                Attributes = attributes,
            };
            var colorTarget = new WGPUColorTargetState {
                NextInChain = null,
                Format = _surfaceFormat,
                Blend = null,
                WriteMask = WGPUColorWriteMask.All,
            };
            var fragment = new WGPUFragmentState {
                NextInChain = null,
                Module = Pointer(shader),
                EntryPoint = fragmentEntryPoint.View,
                ConstantCount = 0,
                Constants = null,
                TargetCount = 1,
                Targets = &colorTarget,
            };
            var stencil = new WGPUStencilFaceState {
                Compare = WGPUCompareFunction.Always,
                FailOp = WGPUStencilOperation.Keep,
                DepthFailOp = WGPUStencilOperation.Keep,
                PassOp = WGPUStencilOperation.Keep,
            };
            var depthStencil = new WGPUDepthStencilState {
                NextInChain = null,
                Format = _depthFormat,
                DepthWriteEnabled = WGPUOptionalBool.True,
                DepthCompare = WGPUCompareFunction.Less,
                StencilFront = stencil,
                StencilBack = stencil,
                StencilReadMask = uint.MaxValue,
                StencilWriteMask = uint.MaxValue,
                DepthBias = 0,
                DepthBiasSlopeScale = 0,
                DepthBiasClamp = 0,
            };
            var descriptor = new WGPURenderPipelineDescriptor {
                NextInChain = null,
                Label = default,
                Layout = Pointer(_pipelineLayout),
                Vertex = new WGPUVertexState {
                    NextInChain = null,
                    Module = Pointer(shader),
                    EntryPoint = vertexEntryPoint.View,
                    ConstantCount = 0,
                    Constants = null,
                    BufferCount = 1,
                    Buffers = &vertexBufferLayout,
                },
                Primitive = new WGPUPrimitiveState {
                    NextInChain = null,
                    Topology = WGPUPrimitiveTopology.TriangleList,
                    StripIndexFormat = WGPUIndexFormat.Undefined,
                    FrontFace = WGPUFrontFace.CCW,
                    CullMode = WGPUCullMode.None,
                    UnclippedDepth = 0,
                },
                DepthStencil = &depthStencil,
                Multisample = new WGPUMultisampleState {
                    NextInChain = null,
                    Count = 1,
                    Mask = uint.MaxValue,
                    AlphaToCoverageEnabled = 0,
                },
                Fragment = &fragment,
            };
            _pipeline = Wgpu.CreateRenderPipeline(_device, in descriptor);
        }
        finally {
            Wgpu.Release(ref shader);
        }
    }

    private bool ResizeIfNeeded(bool force = false)
    {
        var size = Glfw.GetFramebufferSize(_window);
        if (size.Width <= 0 || size.Height <= 0) {
            return false;
        }
        if (!force
            && size.Width == _framebufferWidth
            && size.Height == _framebufferHeight) {
            return true;
        }

        ReleaseDepthResources();
        _framebufferWidth = size.Width;
        _framebufferHeight = size.Height;
        var configuration = new WGPUSurfaceConfiguration {
            NextInChain = null,
            Device = Pointer(_device),
            Format = _surfaceFormat,
            Usage = WGPUTextureUsage.RenderAttachment,
            Width = (uint)_framebufferWidth,
            Height = (uint)_framebufferHeight,
            ViewFormatCount = 0,
            ViewFormats = null,
            AlphaMode = _alphaMode,
            PresentMode = _presentMode,
        };
        Wgpu.ConfigureSurface(_surface, in configuration);
        _surfaceConfigured = true;
        CreateDepthResources();
        return true;
    }

    private void CreateDepthResources()
    {
        var textureDescriptor = new WGPUTextureDescriptor {
            NextInChain = null,
            Label = default,
            Usage = WGPUTextureUsage.RenderAttachment,
            Dimension = WGPUTextureDimension._2D,
            Size = new WGPUExtent3D {
                Width = (uint)_framebufferWidth,
                Height = (uint)_framebufferHeight,
                DepthOrArrayLayers = 1,
            },
            Format = _depthFormat,
            MipLevelCount = 1,
            SampleCount = 1,
            ViewFormatCount = 0,
            ViewFormats = null,
        };
        _depthTexture = Wgpu.CreateTexture(_device, in textureDescriptor);
        var viewDescriptor = new WGPUTextureViewDescriptor {
            NextInChain = null,
            Label = default,
            Format = _depthFormat,
            Dimension = WGPUTextureViewDimension._2D,
            BaseMipLevel = 0,
            MipLevelCount = 1,
            BaseArrayLayer = 0,
            ArrayLayerCount = 1,
            Aspect = WGPUTextureAspect.DepthOnly,
            Usage = WGPUTextureUsage.RenderAttachment,
        };
        _depthView = Wgpu.CreateTextureView(_depthTexture, in viewDescriptor);
    }

    private void UploadGeometry()
    {
        var requiredBytes = checked((ulong)_meshBuilder.VertexCount * DebugMeshBuilder.VertexStride);
        EnsureVertexBuffer(requiredBytes);
        if (requiredBytes != 0) {
            Wgpu.WriteBuffer(_queue, _vertexBuffer, 0, _meshBuilder.Vertices);
        }
    }

    private void EnsureVertexBuffer(ulong requiredBytes)
    {
        if (requiredBytes <= _vertexBufferCapacity) {
            return;
        }
        var newCapacity = _vertexBufferCapacity == 0 ? 64UL * 1024 : _vertexBufferCapacity;
        while (newCapacity < requiredBytes) {
            newCapacity = checked(newCapacity * 2);
        }
        Wgpu.Release(ref _vertexBuffer);
        var descriptor = new WGPUBufferDescriptor {
            NextInChain = null,
            Label = default,
            Usage = WGPUBufferUsage.Vertex | WGPUBufferUsage.CopyDst,
            Size = newCapacity,
            MappedAtCreation = 0,
        };
        _vertexBuffer = Wgpu.CreateBuffer(_device, in descriptor);
        _vertexBufferCapacity = newCapacity;
    }

    private void WriteUniforms()
    {
        var forward = GetCameraForward();
        var view = CreateLookAt(
            _cameraPosition,
            _cameraPosition + forward,
            new float3(0f, 1f, 0f));
        var projection = CreatePerspectiveFieldOfView(
            MathF.PI / 4,
            (float)_framebufferWidth / _framebufferHeight,
            0.05f,
            500);
        var uniforms = new CameraUniforms {
            // math.mul(a, b) composes as standard(a * b), so this yields
            // the usual projection * view (apply view, then projection).
            ViewProjection = math.mul(projection, view),
            EyePosition = new float4(_cameraPosition, 1f),
        };
        Wgpu.WriteBuffer(_queue, _uniformBuffer, 0, [uniforms]);
    }

    // Right-handed view matrix (camera looks down local -Z), column-vector
    // convention with translation in the last column, WGPU-native layout —
    // deliberately hand-built rather than float4x4.LookAt()/fastinverse(),
    // since LookAt() returns a camera-to-world matrix whose forward axis is
    // +Z, the opposite of what CreatePerspectiveFieldOfView below expects.
    private static float4x4 CreateLookAt(float3 eye, float3 target, float3 up)
    {
        var forward = math.normalize(target - eye);
        var right = math.normalize(math.cross(forward, up));
        var trueUp = math.cross(right, forward);
        return new float4x4(
            right.x, right.y, right.z, -math.dot(right, eye),
            trueUp.x, trueUp.y, trueUp.z, -math.dot(trueUp, eye),
            -forward.x, -forward.y, -forward.z, math.dot(forward, eye),
            0f, 0f, 0f, 1f);
    }

    // Right-handed perspective matrix with WebGPU/D3D-style [0, 1] depth
    // range (float4x4.PerspectiveFov in Sia.Math targets OpenGL's [-1, 1]
    // range instead, which isn't valid for a WebGPU depth attachment).
    private static float4x4 CreatePerspectiveFieldOfView(float verticalFov, float aspect, float near, float far)
    {
        var cotangent = 1f / MathF.Tan(verticalFov * 0.5f);
        return new float4x4(
            cotangent / aspect, 0f, 0f, 0f,
            0f, cotangent, 0f, 0f,
            0f, 0f, far / (near - far), near * far / (near - far),
            0f, 0f, -1f, 0f);
    }

    private void RenderFrame()
    {
        var surfaceTexture = Wgpu.AcquireSurfaceTexture(_surface);
        if (surfaceTexture.Status is not (
            WGPUSurfaceGetCurrentTextureStatus.SuccessOptimal
            or WGPUSurfaceGetCurrentTextureStatus.SuccessSuboptimal)) {
            if (surfaceTexture.HasTexture) {
                Wgpu.Release(ref surfaceTexture);
            }
            if (surfaceTexture.Status is WGPUSurfaceGetCurrentTextureStatus.Outdated
                or WGPUSurfaceGetCurrentTextureStatus.Lost) {
                ResizeIfNeeded(force: true);
                return;
            }
            if (surfaceTexture.Status == WGPUSurfaceGetCurrentTextureStatus.Timeout) {
                return;
            }
            throw new WgpuException($"Surface texture acquisition failed with status {surfaceTexture.Status}.");
        }

        var surfaceView = default(WgpuHandle<WGPUTextureView>);
        try {
            var viewDescriptor = new WGPUTextureViewDescriptor {
                NextInChain = null,
                Label = default,
                Format = _surfaceFormat,
                Dimension = WGPUTextureViewDimension._2D,
                BaseMipLevel = 0,
                MipLevelCount = 1,
                BaseArrayLayer = 0,
                ArrayLayerCount = 1,
                Aspect = WGPUTextureAspect.All,
                Usage = WGPUTextureUsage.RenderAttachment,
            };
            surfaceView = Wgpu.CreateTextureView(surfaceTexture, in viewDescriptor);
            EncodeAndSubmit(surfaceView);
            Wgpu.PresentSurfaceOrThrow(_surface);
        }
        finally {
            Wgpu.Release(ref surfaceView);
            Wgpu.Release(ref surfaceTexture);
        }
    }

    private void EncodeAndSubmit(WgpuHandle<WGPUTextureView> colorView)
    {
        var colorAttachment = new WGPURenderPassColorAttachment {
            NextInChain = null,
            View = Pointer(colorView),
            DepthSlice = uint.MaxValue,
            ResolveTarget = null,
            LoadOp = WGPULoadOp.Clear,
            StoreOp = WGPUStoreOp.Store,
            ClearValue = new WGPUColor {
                R = 0.035,
                G = 0.045,
                B = 0.06,
                A = 1,
            },
        };
        var depthAttachment = new WGPURenderPassDepthStencilAttachment {
            NextInChain = null,
            View = Pointer(_depthView),
            DepthLoadOp = WGPULoadOp.Clear,
            DepthStoreOp = WGPUStoreOp.Store,
            DepthClearValue = 1,
            DepthReadOnly = 0,
            StencilLoadOp = WGPULoadOp.Undefined,
            StencilStoreOp = WGPUStoreOp.Undefined,
            StencilClearValue = 0,
            StencilReadOnly = 1,
        };
        var passDescriptor = new WGPURenderPassDescriptor {
            NextInChain = null,
            Label = default,
            ColorAttachmentCount = 1,
            ColorAttachments = &colorAttachment,
            DepthStencilAttachment = &depthAttachment,
            OcclusionQuerySet = null,
            TimestampWrites = null,
        };
        var encoder = Wgpu.CreateCommandEncoder(_device);
        var commandBuffer = default(WgpuHandle<WGPUCommandBuffer>);
        try {
            var pass = Wgpu.BeginRenderPass(encoder, in passDescriptor);
            try {
                Wgpu.SetRenderPipeline(pass, _pipeline);
                Wgpu.SetBindGroup(pass, 0, _bindGroup);
                if (_meshBuilder.VertexCount != 0) {
                    Wgpu.SetVertexBuffer(
                        pass,
                        0,
                        _vertexBuffer,
                        size: checked((ulong)_meshBuilder.VertexCount * DebugMeshBuilder.VertexStride));
                    Wgpu.Draw(pass, (uint)_meshBuilder.VertexCount);
                }
                Wgpu.EndRenderPass(pass);
            }
            finally {
                Wgpu.Release(ref pass);
            }

            var commandDescriptor = new WGPUCommandBufferDescriptor {
                NextInChain = null,
                Label = default,
            };
            commandBuffer = Wgpu.FinishCommandEncoder(encoder, in commandDescriptor);
            Wgpu.Submit(_queue, [commandBuffer]);
        }
        finally {
            Wgpu.Release(ref commandBuffer);
            Wgpu.Release(ref encoder);
        }
    }

    private void HandleInput(float deltaTime)
    {
        if (IsDown(Key.Escape)) {
            Glfw.RequestClose(_window);
        }
        if (Pressed(Key.Space)) {
            _paused = !_paused;
        }
        if (Pressed(Key.R)) {
            _scene!.ResetAll();
        }
        if (Pressed(Key.Home)) {
            ResetCamera();
        }
        if (MousePressed(MouseButton.Left)) {
            var reset = _scene!.RaycastReset(_cameraPosition, GetCameraForward());
            Console.WriteLine(reset
                ? $"Reset region: {_scene.LastResetName}"
                : "Reset ray missed every physics region.");
        }

        UpdateMouseLook();
        var orbitSpeed = 1.35f * deltaTime;
        if (IsDown(Key.Left)) {
            _cameraYaw -= orbitSpeed;
        }
        if (IsDown(Key.Right)) {
            _cameraYaw += orbitSpeed;
        }
        if (IsDown(Key.Up)) {
            _cameraPitch = System.Math.Clamp(_cameraPitch + orbitSpeed, -1.2f, 1.2f);
        }
        if (IsDown(Key.Down)) {
            _cameraPitch = System.Math.Clamp(_cameraPitch - orbitSpeed, -1.2f, 1.2f);
        }

        var forward = GetCameraForward();
        var planarForward = math.normalizesafe(new float3(forward.x, 0f, forward.z));
        var right = math.normalizesafe(math.cross(planarForward, new float3(0f, 1f, 0f)));
        var movement = float3.zero;
        if (IsDown(Key.W)) {
            movement += planarForward;
        }
        if (IsDown(Key.S)) {
            movement -= planarForward;
        }
        if (IsDown(Key.D)) {
            movement += right;
        }
        if (IsDown(Key.A)) {
            movement -= right;
        }
        if (IsDown(Key.E)) {
            movement.y += 1f;
        }
        if (IsDown(Key.Q)) {
            movement.y -= 1f;
        }
        var movementLengthSquared = math.lengthsq(movement);
        if (movementLengthSquared > 1e-8f) {
            var speed = IsDown(Key.LeftShift) || IsDown(Key.RightShift) ? 18f : 8f;
            _cameraPosition += movement * math.rsqrt(movementLengthSquared) * speed * deltaTime;
        }
    }

    private void UpdateMouseLook()
    {
        var cursor = Glfw.GetCursorPosition(_window);
        if (Glfw.GetMouseButton(_window, MouseButton.Right) == InputAction.Release) {
            _looking = false;
            return;
        }

        if (_looking) {
            _cameraYaw -= (float)(cursor.X - _lastCursorX) * 0.003f;
            _cameraPitch = System.Math.Clamp(
                _cameraPitch - (float)(cursor.Y - _lastCursorY) * 0.003f,
                -1.48f,
                1.48f);
        }
        _looking = true;
        _lastCursorX = cursor.X;
        _lastCursorY = cursor.Y;
    }

    private float3 GetCameraForward()
    {
        var horizontal = math.cos(_cameraPitch);
        return math.normalize(new float3(
            math.sin(_cameraYaw) * horizontal,
            math.sin(_cameraPitch),
            math.cos(_cameraYaw) * horizontal));
    }

    private void ResetCamera()
    {
        _cameraPosition = new float3(0f, 8f, 34f);
        _cameraYaw = math.PI;
        _cameraPitch = -0.16f;
    }

    private bool IsDown(Key key)
        => Glfw.GetKey(_window, key) != InputAction.Release;

    private bool Pressed(Key key)
    {
        if (IsDown(key)) {
            return _pressedKeys.Add(key);
        }
        _pressedKeys.Remove(key);
        return false;
    }

    private bool MousePressed(MouseButton button)
    {
        if (Glfw.GetMouseButton(_window, button) != InputAction.Release) {
            return _pressedMouseButtons.Add(button);
        }
        _pressedMouseButtons.Remove(button);
        return false;
    }

    private void UpdateWindowTitle(double currentTime)
    {
        if (currentTime - _lastTitleUpdate < 0.2 || _scene is null) {
            return;
        }
        _lastTitleUpdate = currentTime;
        var state = _paused ? "paused" : "running";
        Glfw.SetTitle(
            _window,
            $"Sia.Physics Exhibition · {state} · bodies {_scene.BodyCount} · "
                + $"particles {_scene.ParticleCount} · contacts {_scene.ContactCount} · "
                + $"last reset {_scene.LastResetName} · triangles {_meshBuilder.TriangleCount:N0}");
    }

    private static void PrintControls()
    {
        Console.WriteLine("Sia.Physics WebGPU controls:");
        Console.WriteLine("  WASD move · Q/E descend/ascend · Shift sprint");
        Console.WriteLine("  Right-drag or arrows look · left click raycast-reset region");
        Console.WriteLine("  R reset all · Home reset camera · Space pause · Esc close");
    }

    private void ReleaseDepthResources()
    {
        Wgpu.Release(ref _depthView);
        Wgpu.Release(ref _depthTexture);
    }

    public void Dispose()
    {
        if (_disposed) {
            return;
        }
        _disposed = true;
        _scene?.Dispose();
        _scene = null;
        ReleaseDepthResources();
        Wgpu.Release(ref _vertexBuffer);
        Wgpu.Release(ref _pipeline);
        Wgpu.Release(ref _pipelineLayout);
        Wgpu.Release(ref _bindGroup);
        Wgpu.Release(ref _bindGroupLayout);
        Wgpu.Release(ref _uniformBuffer);
        Wgpu.Release(ref _queue);

        if (!_surface.IsNull && _surfaceConfigured) {
            Wgpu.UnconfigureSurface(_surface);
            _surfaceConfigured = false;
        }
        if (!_device.IsNull) {
            Wgpu.DestroyDevice(_device);
        }
        Wgpu.Release(ref _device);
        Wgpu.Release(ref _adapter);
        Wgpu.Release(ref _surface);
        Wgpu.Release(ref _instance);

        if (!_window.IsNull) {
            Glfw.DestroyWindow(ref _window);
        }
        if (_glfwInitialized) {
            Glfw.Terminate();
            _glfwInitialized = false;
        }
    }

    private static T* Pointer<T>(WgpuHandle<T> handle)
        where T : unmanaged
        => (T*)handle.DangerousGetHandle();

    [StructLayout(LayoutKind.Sequential)]
    private struct CameraUniforms
    {
        public float4x4 ViewProjection;
        public float4 EyePosition;
    }

    private readonly record struct SurfaceInfo(
        WGPUTextureFormat Format,
        WGPUCompositeAlphaMode AlphaMode,
        WGPUPresentMode PresentMode);

    private sealed class NativeString : IDisposable
    {
        private nint _memory;

        public NativeString(string value)
        {
            _memory = Marshal.StringToCoTaskMemUTF8(value);
            View = new WGPUStringView {
                Data = (byte*)_memory,
                Length = (nuint)Encoding.UTF8.GetByteCount(value),
            };
        }

        public WGPUStringView View { get; }

        public void Dispose()
        {
            if (_memory == 0) {
                return;
            }
            Marshal.FreeCoTaskMem(_memory);
            _memory = 0;
        }
    }
}
