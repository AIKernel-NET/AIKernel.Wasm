using AIKernel.Dtos.Capabilities;
using AIKernel.Dtos.Gpu;
using AIKernel.Enums;
using AIKernel.Abstractions.Compute;
using AIKernel.Abstractions.Gpu;
using AIKernel.Providers.Standard.EventBus;
using AIKernel.Wasm.Compute;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace AIKernel.Wasm.Tests;

public sealed class WebGpuComputeProviderContractTests
{
    [Fact]
    public void ToContract_ExposesWebGpuProviderOperations()
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [GpuProviderMetadataKeys.AdapterProfile] = "webgpu-browser",
            [GpuProviderMetadataKeys.Backend] = "browser-webgpu",
            [GpuProviderMetadataKeys.Fallback] = "webgpu-or-cpu",
            [GpuProviderMetadataKeys.Version] = "0.1.3"
        };

        var contract = WebGpuComputeCapabilityContracts.ToContract(
            new WebGpuComputeCapabilityDescriptor(
                "providers.webgpu",
                "webgpu-browser",
                metadata));

        Assert.Equal("providers.webgpu", contract.CapabilityId);
        Assert.Equal("WebGPU Compute Provider", contract.Name);
        Assert.Equal(CapabilityModuleKind.NativeLibrary, contract.Kind);
        Assert.Equal(CapabilityInvocationMode.Direct, contract.InvocationMode);
        Assert.Equal(GpuOperationNames.WebGpuDispatchEntryPoint, contract.EntryPoint);
        Assert.Equal(
            [
                GpuOperationNames.ComputeDispatch,
                GpuOperationNames.ComputeVectorAdd,
                GpuOperationNames.GpuHudComposite,
                GpuOperationNames.GpuAisthesisRawFrame,
                GpuOperationNames.GpuSpatialReasoning,
                GpuOperationNames.GpuZeroCopyRawTexture
            ],
            contract.ProvidedOperations);
        Assert.Equal(GpuPermissionNames.WebGpuComputeRequiredPermissions, contract.RequiredPermissions);
    }

    [Fact]
    public void Provider_DefaultIdentityMatchesWasmManifest()
    {
        var provider = new global::AIKernel.Wasm.Compute.WebGpuComputeProvider();

        Assert.Equal("webgpu.compute", provider.ProviderId);
        Assert.True(provider.GetCapabilities().SupportsOperation(GpuOperationNames.ComputeDispatch));
        Assert.True(provider.GetCapabilities().SupportsOperation(GpuOperationNames.GpuHudComposite));
        Assert.True(provider.GetCapabilities().SupportsOperation(GpuOperationNames.GpuAisthesisRawFrame));
        Assert.True(provider.GetCapabilities().SupportsOperation(GpuOperationNames.GpuSpatialReasoning));
    }

    [Fact]
    public async Task Provider_LifecycleAndCapabilitiesRemainContractPure()
    {
        var provider = new global::AIKernel.Wasm.Compute.WebGpuComputeProvider(new WebGpuComputeSettings
        {
            ProviderId = "providers.webgpu",
            ForceCpuFallback = true
        });

        Assert.True(provider.IsAvailable());
        Assert.True(await provider.IsAvailableAsync());
        await provider.InitializeAsync();

        Assert.True(await provider.IsAvailableAsync());
        Assert.True(provider.GetCapabilities().SupportsOperation(GpuOperationNames.ComputeDispatch));
        Assert.True(provider.GetCapabilities().SupportsOperation(GpuOperationNames.ComputeVectorAdd));
        Assert.Equal("providers.webgpu", provider.ToCapabilityDescriptor().CapabilityId);

        await provider.ShutdownAsync();
        Assert.True(provider.IsAvailable());
    }

    [Fact]
    public async Task CpuFallback_VectorAdd1000ElementsMatchesExpected()
    {
        const int count = 1000;
        var provider = new global::AIKernel.Wasm.Compute.WebGpuComputeProvider(new WebGpuComputeSettings
        {
            ForceCpuFallback = true
        });

        var left = Enumerable.Range(0, count).Select(x => (float)x).ToArray();
        var right = Enumerable.Range(0, count).Select(x => (float)(x * 2)).ToArray();
        var expected = left.Zip(right, static (a, b) => a + b).ToArray();

        using var a = await provider.CreateBufferAsync(count * sizeof(float));
        using var b = await provider.CreateBufferAsync(count * sizeof(float));
        using var output = await provider.CreateBufferAsync(count * sizeof(float));

        await provider.WriteBufferAsync(a, MemoryMarshal.AsBytes<float>(left.AsSpan()).ToArray());
        await provider.WriteBufferAsync(b, MemoryMarshal.AsBytes<float>(right.AsSpan()).ToArray());
        await provider.ExecuteKernelAsync(ComputeKernel.CreateVectorAdd(count), a, b, output);

        var actualBytes = new byte[count * sizeof(float)];
        await provider.ReadBufferAsync(output, actualBytes);
        var actual = MemoryMarshal.Cast<byte, float>(actualBytes).ToArray();

        Assert.Equal(expected, actual);
        Assert.True(provider.UsingCpuFallback);
    }

    [Fact]
    public async Task GpuRequested_UsesCpuFallbackWhenBackendUnavailableAndMatchesExpected()
    {
        const int count = 1000;
        var provider = new global::AIKernel.Wasm.Compute.WebGpuComputeProvider(new WebGpuComputeSettings
        {
            ForceCpuFallback = false
        });

        var left = Enumerable.Range(0, count).Select(x => (float)(x + 1)).ToArray();
        var right = Enumerable.Range(0, count).Select(x => (float)(x + 3)).ToArray();
        var expected = left.Zip(right, static (a, b) => a + b).ToArray();

        using var a = await provider.CreateBufferAsync(count * sizeof(float));
        using var b = await provider.CreateBufferAsync(count * sizeof(float));
        using var output = await provider.CreateBufferAsync(count * sizeof(float));

        await provider.WriteBufferAsync(a, MemoryMarshal.AsBytes<float>(left.AsSpan()).ToArray());
        await provider.WriteBufferAsync(b, MemoryMarshal.AsBytes<float>(right.AsSpan()).ToArray());
        await provider.ExecuteKernelAsync(ComputeKernel.CreateVectorAdd(count), a, b, output);

        var actualBytes = new byte[count * sizeof(float)];
        await provider.ReadBufferAsync(output, actualBytes);
        var actual = MemoryMarshal.Cast<byte, float>(actualBytes).ToArray();

        Assert.Equal(expected, actual);
        Assert.True(provider.UsingCpuFallback);
    }

    [Fact]
    public async Task WasmBackend_WhenInteropAvailable_ExecutesVectorAddPipeline()
    {
        const int count = 4;
        var provider = new global::AIKernel.Wasm.Compute.WebGpuComputeProvider(
            new WebGpuComputeSettings { ForceCpuFallback = false },
            new WebGpuWasmBackend(new FakeWebGpuJsInterop()));
        var left = new[] { 1.0f, 2.0f, 3.0f, 4.0f };
        var right = new[] { 10.0f, 20.0f, 30.0f, 40.0f };

        using var a = await provider.CreateBufferAsync(count * sizeof(float));
        using var b = await provider.CreateBufferAsync(count * sizeof(float));
        using var output = await provider.CreateBufferAsync(count * sizeof(float));

        await provider.WriteBufferAsync(a, MemoryMarshal.AsBytes<float>(left.AsSpan()).ToArray());
        await provider.WriteBufferAsync(b, MemoryMarshal.AsBytes<float>(right.AsSpan()).ToArray());
        await provider.ExecuteKernelAsync(ComputeKernel.CreateVectorAdd(count), a, b, output);

        var actualBytes = new byte[count * sizeof(float)];
        await provider.ReadBufferAsync(output, actualBytes);
        var actual = MemoryMarshal.Cast<byte, float>(actualBytes).ToArray();

        Assert.Equal([11.0f, 22.0f, 33.0f, 44.0f], actual);
        Assert.False(provider.UsingCpuFallback);
    }

    [Fact]
    public async Task CanonicalGpuContext_WhenBackendUnavailable_ReportsCpuFallbackDiagnostics()
    {
        var provider = new global::AIKernel.Wasm.Compute.WebGpuComputeProvider(new WebGpuComputeSettings
        {
            ForceCpuFallback = false
        });

        await using var context = await provider.CreateContextAsync(new(), TestContext.Current.CancellationToken);

        Assert.True(context.UsingCpuFallback);
        Assert.Equal(GpuBackend.CpuFallback, context.Backend);
        Assert.Equal(GpuReadbackPolicy.RequiredFallback, context.Diagnostics.SensorPath.Readback);
        Assert.Equal("cpu-fallback", context.Diagnostics.HudPath.FallbackReason);
        Assert.Equal("sensor", context.Diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPathRole]);
        Assert.Equal("sensor", context.Diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPassId]);
        Assert.Equal("false", context.Diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalAuthoritativeReady]);
        Assert.Equal("csharp-deterministic", context.Diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalExecutionMode]);
        Assert.Equal("cpu-fallback", context.Diagnostics.SensorPath.Metadata[GpuProviderMetadataKeys.GpuBypass]);
        Assert.Equal("not-active", context.Diagnostics.SensorPath.Metadata[GpuProviderMetadataKeys.NativeJsBridge]);
        Assert.Equal("cpu-readback-buffer", context.Diagnostics.SensorPath.Metadata[GpuProviderMetadataKeys.ZeroCopyBufferHandling]);
        Assert.Equal("true", context.Diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPromotionBlocked]);
        Assert.Equal("false", context.Diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPromotionCandidateReady]);
        Assert.Equal(GpuPromotionGates.NotEvaluated, context.Diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPromotionReason]);
        Assert.Contains("ReadyForBuiltIn=false", context.Diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPassReadiness]);
        Assert.True(GpuCanonicalValidation.ValidateExecutionLayerMetadata(context.Diagnostics.SensorPath.Metadata).IsValid);
        Assert.False(GpuCanonicalValidation.EvaluatePromotionReadiness(
            context.Diagnostics.SensorPath.Metadata).IsAuthoritativeReady);
    }

    [Fact]
    public async Task CanonicalGpuContext_WhenWebGpuBackendAvailable_ReportsZeroCopyDiagnostics()
    {
        var provider = new global::AIKernel.Wasm.Compute.WebGpuComputeProvider(
            new WebGpuComputeSettings { ForceCpuFallback = false },
            new WebGpuWasmBackend(new FakeWebGpuJsInterop()));

        await using var context = await provider.CreateContextAsync(new(), TestContext.Current.CancellationToken);
        var validation = GpuCanonicalValidation.ValidateFrameDiagnostics(context.Diagnostics);

        Assert.False(context.UsingCpuFallback);
        Assert.Equal(GpuBackend.WebGpu, context.Backend);
        Assert.True(context.Diagnostics.GamePath.ZeroCopy);
        Assert.True(context.Diagnostics.BonsaiPath.ZeroCopy);
        Assert.True(context.Diagnostics.HudPath.ZeroCopy);
        Assert.True(context.Diagnostics.SensorPath.ZeroCopy);
        Assert.Equal(GpuReadbackPolicy.None, context.Diagnostics.SensorPath.Readback);
        Assert.Equal("raw-texture-binding", context.Diagnostics.SensorPath.Metadata[GpuProviderMetadataKeys.GpuBypass]);
        Assert.Equal("canonical-envelope-bridge", context.Diagnostics.SensorPath.Metadata[GpuProviderMetadataKeys.NativeJsBridge]);
        Assert.Equal("raw-framebuffer-texture", context.Diagnostics.SensorPath.Metadata[GpuProviderMetadataKeys.ZeroCopyBufferHandling]);
        Assert.Equal("0", context.Diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalFrameIndex]);
        Assert.Equal("0", context.Diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalSampleTicks]);
        Assert.Equal("true", context.Diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPromotionBlocked]);
        Assert.Equal("false", context.Diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPromotionCandidateReady]);
        Assert.Equal(GpuPromotionGates.NotEvaluated, context.Diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPromotionReason]);
        Assert.Contains("BuiltInExecutor=true", context.Diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPassReadiness]);
        Assert.True(GpuCanonicalValidation.ValidateExecutionLayerMetadata(context.Diagnostics.SensorPath.Metadata).IsValid);
        Assert.False(GpuCanonicalValidation.EvaluatePromotionReadiness(
            context.Diagnostics.SensorPath.Metadata).IsAuthoritativeReady);
        Assert.True(validation.IsValid);
    }

    [Fact]
    public async Task CanonicalGpuDiagnostics_CapturesFrameSpecificFourPathTable()
    {
        var provider = new global::AIKernel.Wasm.Compute.WebGpuComputeProvider(
            new WebGpuComputeSettings { ForceCpuFallback = false },
            new WebGpuWasmBackend(new FakeWebGpuJsInterop()));
        var raw = RawTarget("raw") with
        {
            PixelFormat = FramePixelFormat.Indexed8
        };
        var hud = raw with
        {
            TargetId = "hud",
            Kind = GpuFrameTargetKind.HudCompositeOffscreen,
            PixelFormat = FramePixelFormat.Rgba32
        };
        var frame = FrameToken(raw) with
        {
            HudTarget = hud
        };

        var diagnosticsProvider = Assert.IsAssignableFrom<IGpuDiagnostics>(provider);
        var diagnostics = await diagnosticsProvider.CaptureFrameDiagnosticsAsync(
            frame,
            TestContext.Current.CancellationToken);
        var validation = GpuCanonicalValidation.ValidateFrameDiagnostics(diagnostics);

        Assert.True(validation.IsValid);
        Assert.Equal("frame-1", diagnostics.SensorPath.FrameId);
        Assert.Equal("sensor", diagnostics.SensorPath.PassId);
        Assert.True(diagnostics.GamePath.ZeroCopy);
        Assert.True(diagnostics.BonsaiPath.ZeroCopy);
        Assert.True(diagnostics.HudPath.ZeroCopy);
        Assert.True(diagnostics.SensorPath.ZeroCopy);
        Assert.Equal(GpuReadbackPolicy.None, diagnostics.SensorPath.Readback);
        Assert.Equal(320 * 200, diagnostics.SensorPath.MemoryEstimate);
        Assert.Equal(320 * 200 * 4, diagnostics.HudPath.MemoryEstimate);
        Assert.Equal("sensor", diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPathRole]);
        Assert.Equal("sensor", diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPassId]);
        Assert.Equal("1", diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalFrameIndex]);
        Assert.Equal("100", diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalSampleTicks]);
        Assert.Equal("csharp-deterministic", diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalExecutionMode]);
        Assert.Equal("raw-texture-binding", diagnostics.SensorPath.Metadata[GpuProviderMetadataKeys.GpuBypass]);
        Assert.Equal("frame-token-index-sample-ticks", diagnostics.SensorPath.Metadata[GpuProviderMetadataKeys.DeterministicFrameSampling]);
        Assert.Contains("Passes.{Aisthesis,SpatialReasoning,HudComposite}", diagnostics.SensorPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPassReadiness]);
        Assert.Equal("hud", diagnostics.HudPath.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPathRole]);
    }

    [Fact]
    public async Task CanonicalGpuAisthesis_ProducesFeatureVectorAndRejectsHudTarget()
    {
        var provider = new global::AIKernel.Wasm.Compute.WebGpuComputeProvider(
            new WebGpuComputeSettings { ForceCpuFallback = false },
            new WebGpuWasmBackend(new FakeWebGpuJsInterop()));
        var raw = RawTarget("raw");
        await provider.InitializeAsync();

        var output = await provider.ProcessAsync(new GpuAisthesisInput
        {
            Frame = FrameToken(raw),
            RawFramebuffer = raw,
            Features = new Dictionary<string, bool>(StringComparer.Ordinal)
            {
                ["edge"] = true,
                ["redPanel"] = true
            }
        }, TestContext.Current.CancellationToken);

        Assert.Equal(GpuCanonicalLayouts.FeatureVector.Stride, output.FeatureVector.Count);
        Assert.Equal(320.0f, output.FeatureVector[0]);
        Assert.Equal(200.0f, output.FeatureVector[1]);
        Assert.Equal("WebGpu", output.Diagnostics.Backend);
        Assert.True(output.Diagnostics.ZeroCopy);
        await Assert.ThrowsAsync<ArgumentException>(() => provider.ProcessAsync(new GpuAisthesisInput
        {
            Frame = FrameToken(raw),
            RawFramebuffer = raw with
            {
                TargetId = "hud",
                Kind = GpuFrameTargetKind.HudCompositeOffscreen
            }
        }, TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task CanonicalGpuSpatialReasoning_ProducesStableSpatialVector()
    {
        var provider = new global::AIKernel.Wasm.Compute.WebGpuComputeProvider(new WebGpuComputeSettings
        {
            ForceCpuFallback = true
        });
        var matrices = Enumerable
            .Range(0, GpuCanonicalLayouts.AisMatrix.MaxItems)
            .Select(index => Enumerable.Repeat(index / 10.0f, GpuCanonicalLayouts.AisMatrix.Stride).ToArray())
            .Cast<IReadOnlyList<float>>()
            .ToArray();

        var output = await provider.ReasonAsync(new GpuSpatialReasoningInput
        {
            Frame = FrameToken(RawTarget("raw")),
            AisMatrices = matrices,
            StateVector = [0.25f, 0.75f]
        }, TestContext.Current.CancellationToken);

        Assert.Equal(GpuCanonicalLayouts.SpatialVector.Stride, output.SpatialVector.Count);
        Assert.Equal(0.0f, output.SpatialVector[0]);
        Assert.Equal(0.1f, output.SpatialVector[4], precision: 5);
        Assert.Equal(0.25f, output.SpatialVector[16]);
        Assert.Equal(GpuReadbackPolicy.RequiredFallback, output.Diagnostics.Readback);
    }

    [Fact]
    public async Task CanonicalGpuHudComposer_ReturnsHudCompositeTarget()
    {
        var provider = new global::AIKernel.Wasm.Compute.WebGpuComputeProvider(new WebGpuComputeSettings
        {
            ForceCpuFallback = true
        });
        var raw = RawTarget("raw");
        var output = await provider.ComposeAsync(new GpuHudInput
        {
            Frame = FrameToken(raw),
            HudPanelRects = new float[GpuCanonicalLayouts.HudPanelRect.Stride],
            HudPanelStateVectors = new float[GpuCanonicalLayouts.HudPanelStateVector.Stride]
        }, TestContext.Current.CancellationToken);

        Assert.Equal(GpuFrameTargetKind.HudCompositeOffscreen, output.Kind);
        Assert.Equal(GpuBackend.CpuFallback, output.Backend);
        Assert.Equal(320, output.Width);
        Assert.Equal(200, output.Height);
    }

    [Fact]
    public async Task CanonicalGpuPasses_WhenCanonicalInteropAvailable_DispatchThroughJsBoundary()
    {
        var interop = new FakeWebGpuCanonicalJsInterop();
        var provider = new global::AIKernel.Wasm.Compute.WebGpuComputeProvider(
            new WebGpuComputeSettings { ForceCpuFallback = false },
            new WebGpuWasmBackend(interop));
        var raw = RawTarget("raw");
        var frame = FrameToken(raw);
        var matrices = Enumerable
            .Range(0, GpuCanonicalLayouts.AisMatrix.MaxItems)
            .Select(_ => Enumerable.Repeat(0.25f, GpuCanonicalLayouts.AisMatrix.Stride).ToArray())
            .Cast<IReadOnlyList<float>>()
            .ToArray();

        var aisthesis = await provider.ProcessAsync(new GpuAisthesisInput
        {
            Frame = frame,
            RawFramebuffer = raw,
            Features = new Dictionary<string, bool>(StringComparer.Ordinal) { ["edge"] = true }
        }, TestContext.Current.CancellationToken);
        var spatial = await provider.ReasonAsync(new GpuSpatialReasoningInput
        {
            Frame = frame,
            AisMatrices = matrices,
            StateVector = [0.5f]
        }, TestContext.Current.CancellationToken);
        var hud = await provider.ComposeAsync(new GpuHudInput
        {
            Frame = frame,
            HudPanelRects = new float[GpuCanonicalLayouts.HudPanelRect.Stride],
            HudPanelStateVectors = new float[GpuCanonicalLayouts.HudPanelStateVector.Stride],
            EgoRadar = new GpuHudEgoRadarInput
            {
                CompassHeadingDegrees = 45.0f,
                CompassConfidence = 0.75f,
                Kinesis = new GpuVector2(0.25f, 0.75f)
            }
        }, TestContext.Current.CancellationToken);

        Assert.Equal(1, interop.AisthesisDispatches);
        Assert.Equal(1, interop.SpatialDispatches);
        Assert.Equal(1, interop.HudDispatches);
        Assert.Equal(42.0f, aisthesis.FeatureVector[0]);
        Assert.Equal(24.0f, spatial.SpatialVector[0]);
        Assert.Equal("js-mask", aisthesis.MaskTexture?.TargetId);
        Assert.Equal("js-hud", hud.TargetId);
        Assert.Equal(GpuFrameTargetKind.HudCompositeOffscreen, hud.Kind);
        Assert.Equal("js-aisthesis", aisthesis.Diagnostics.PassId);
        Assert.Equal("js-spatial", spatial.Diagnostics.PassId);
        Assert.Equal("js-aisthesis", aisthesis.Diagnostics.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPassId]);
        Assert.Equal("sensor", aisthesis.Diagnostics.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPathRole]);
        Assert.Equal("js-spatial", spatial.Diagnostics.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPassId]);
        Assert.False(provider.UsingCpuFallback);
    }

    [Fact]
    public void CanonicalInteropEnvelope_ProjectsCanonicalInputsIntoStableBridgeShape()
    {
        var raw = RawTarget("raw");
        var hud = raw with
        {
            TargetId = "hud",
            Kind = GpuFrameTargetKind.HudCompositeOffscreen
        };
        var frame = FrameToken(raw) with { HudTarget = hud };
        var matrices = Enumerable
            .Range(0, GpuCanonicalLayouts.AisMatrix.MaxItems)
            .Select(index => Enumerable.Repeat(index / 10.0f, GpuCanonicalLayouts.AisMatrix.Stride).ToArray())
            .Cast<IReadOnlyList<float>>()
            .ToArray();

        var aisthesis = WebGpuInteropEnvelope.ForAisthesis(new GpuAisthesisInput
        {
            Frame = frame,
            RawFramebuffer = raw,
            Features = new Dictionary<string, bool>(StringComparer.Ordinal)
            {
                ["red-panel-detect"] = true,
                ["edge-detect"] = true
            }
        });
        var spatial = WebGpuInteropEnvelope.ForSpatialReasoning(new GpuSpatialReasoningInput
        {
            Frame = frame,
            AisMatrices = matrices,
            StateVector = [0.25f, 0.75f]
        });
        var hudEnvelope = WebGpuInteropEnvelope.ForHudComposite(new GpuHudInput
        {
            Frame = frame,
            HudPanelRects = Enumerable.Range(0, GpuCanonicalLayouts.HudPanelRect.Stride).Select(static value => (float)value).ToArray(),
            HudPanelStateVectors = Enumerable.Range(0, GpuCanonicalLayouts.HudPanelStateVector.Stride).Select(static value => value / 10.0f).ToArray(),
            EgoRadar = new GpuHudEgoRadarInput
            {
                CompassHeadingDegrees = 45.0f,
                CompassConfidence = 0.75f,
                CompassUsable = true,
                Kinesis = new GpuVector2(0.25f, 0.5f),
                VisualEnemyYawDegrees = -12.0f,
                VisualEnemyConfidence = 0.8f,
                AudioEnemyYawDegrees = 15.0f,
                AudioEnemyConfidence = 0.6f,
                FusedEnemyYawDegrees = 0.0f,
                FusedEnemyConfidence = 0.9f,
                SdfLink = true,
                RadarMode = GpuEgoRadarMode.Suppressed,
                Summary = "priority pathos"
            },
            Labels =
            [
                new GpuHudLabel { Id = "zeta", Text = "Z", X = 0.8f, Y = 0.2f },
                new GpuHudLabel { Id = "alpha", Text = "A", X = 0.1f, Y = 0.2f }
            ]
        });

        Assert.Equal("aikernel.gpu.dispatch", aisthesis.Schema);
        Assert.Equal(GpuOperationNames.GpuAisthesisRawFrame, aisthesis.PassId);
        Assert.Equal(WebGpuPassKind.Aisthesis, aisthesis.PassKind);
        Assert.Equal(["edge-detect", "red-panel-detect"], aisthesis.FeatureFlags.Keys.ToArray());
        Assert.Equal("raw-framebuffer", aisthesis.Tags[GpuProviderMetadataKeys.RawCaptureSource]);
        Assert.Equal(GpuOperationNames.GpuAisthesisRawFrame, aisthesis.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPassId]);
        Assert.Equal(GpuExecutionModes.BrowserWebGpuCompute, aisthesis.Metadata[GpuDiagnosticsMetadataKeys.CanonicalExecutionMode]);
        Assert.Equal(GpuCanonicalPilotStates.BrowserBuiltinCompute, aisthesis.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPilotState]);
        Assert.Equal(GpuPromotionGates.TraceCandidate, aisthesis.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPromotionGate]);
        Assert.Equal("true", aisthesis.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPromotionBlocked]);
        Assert.Equal("false", aisthesis.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPromotionCandidateReady]);
        Assert.Equal("storage-texture-not-ready", aisthesis.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPromotionReason]);
        Assert.True(aisthesis.Targets.Single().ZeroCopy);

        Assert.Equal(GpuOperationNames.GpuSpatialReasoning, spatial.PassId);
        Assert.Equal(
            ["ais-matrix:topos", "ais-matrix:route", "ais-matrix:threat", "ais-matrix:zoe", "state-vector"],
            spatial.Buffers.Select(static buffer => buffer.Name).ToArray());
        Assert.Equal(GpuCanonicalLayouts.StateVector.Stride, spatial.Buffers.Last().Values.Count);
        Assert.Equal(0.25f, spatial.Buffers.Last().Values[0]);
        Assert.Equal(0.0f, spatial.Buffers.Last().Values[2]);
        Assert.Equal(GpuOperationNames.GpuSpatialReasoning, spatial.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPassId]);
        Assert.Equal("topos,route,threat,zoe", spatial.Metadata[GpuProviderMetadataKeys.AisMatrixOrder]);

        Assert.Equal(GpuOperationNames.GpuHudComposite, hudEnvelope.PassId);
        Assert.Equal(2, hudEnvelope.Targets.Count);
        Assert.Equal("hud-panel-rects", hudEnvelope.Buffers[0].Name);
        Assert.Equal(GpuCanonicalLayouts.HudPanelRect.Stride, hudEnvelope.Buffers[0].Stride);
        Assert.Equal(1, hudEnvelope.Buffers[0].Count);
        Assert.Equal(45.0f, hudEnvelope.Scalars["compassHeadingDegrees"]);
        Assert.Equal(1.0f, hudEnvelope.Scalars["sdfLink"]);
        Assert.Equal((float)GpuEgoRadarMode.Suppressed, hudEnvelope.Scalars["radarMode"]);
        Assert.Equal(["alpha", "zeta"], hudEnvelope.Labels.Select(static label => label.Id).ToArray());
        Assert.Equal("priority pathos", hudEnvelope.Tags["summary"]);
        Assert.Equal(GpuOperationNames.GpuHudComposite, hudEnvelope.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPassId]);
        Assert.Equal("priority pathos", hudEnvelope.Metadata["summary"]);
        Assert.True(WebGpuInteropEnvelope.ValidateDispatchEnvelope(aisthesis).IsValid);
        Assert.True(WebGpuInteropEnvelope.ValidateDispatchEnvelope(spatial).IsValid);
        Assert.True(WebGpuInteropEnvelope.ValidateDispatchEnvelope(hudEnvelope).IsValid);
    }

    [Fact]
    public void WebGpuInteropEnvelope_RejectsInvalidBridgeEnvelope()
    {
        var raw = RawTarget("raw");
        var valid = WebGpuInteropEnvelope.ForAisthesis(new GpuAisthesisInput
        {
            Frame = FrameToken(raw),
            RawFramebuffer = raw
        });
        var invalid = valid with
        {
            PassKind = WebGpuPassKind.HudComposite,
            Frame = valid.Frame with { FrameIndex = -1 },
            Metadata = new Dictionary<string, string>(valid.Metadata, StringComparer.Ordinal)
            {
                [GpuDiagnosticsMetadataKeys.CanonicalPromotionGate] = "trace-candidte"
            },
            Buffers =
            [
                new WebGpuBufferEnvelope
                {
                    Name = "bad-buffer",
                    LayoutName = "Bad",
                    Stride = 3,
                    Count = 1,
                    Values = [1.0f, 2.0f]
                }
            ]
        };

        var validation = WebGpuInteropEnvelope.ValidateDispatchEnvelope(invalid);

        Assert.False(validation.IsValid);
        Assert.Contains(validation.Errors, static issue => issue.Code == "WEBGPU_ENVELOPE_PASS_ID_KIND_MISMATCH");
        Assert.Contains(validation.Errors, static issue => issue.Code == "WEBGPU_ENVELOPE_FRAME_INDEX_INVALID");
        Assert.Contains(validation.Errors, static issue => issue.Code == "WEBGPU_ENVELOPE_BUFFER_VALUES_STRIDE_MISMATCH");
        Assert.Contains(validation.Errors, static issue => issue.Code == "WEBGPU_ENVELOPE_GPU_DIAGNOSTICS_METADATA_VALUE_UNKNOWN");

        var missingMetadataValidation = WebGpuInteropEnvelope.ValidateDispatchEnvelope(valid with
        {
            Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        });
        Assert.Contains(missingMetadataValidation.Errors, static issue => issue.Code == "WEBGPU_ENVELOPE_METADATA_REQUIRED");
    }

    [Fact]
    public async Task WebGpuWasmBackend_ValidatesEnvelopeBeforeJsHandoff()
    {
        var interop = new FakeWebGpuCanonicalEnvelopeJsInterop();
        var backend = new WebGpuWasmBackend(interop);
        await backend.InitializeAsync(TestContext.Current.CancellationToken);
        var raw = RawTarget("raw");
        var frame = FrameToken(raw) with { FrameIndex = -1 };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await backend.DispatchAisthesisAsync(new GpuAisthesisInput
            {
                Frame = frame,
                RawFramebuffer = raw
            }, TestContext.Current.CancellationToken));

        Assert.Contains("WEBGPU_ENVELOPE_FRAME_INDEX_INVALID", exception.Message);
        Assert.Null(interop.LastAisthesisEnvelope);
    }

    [Fact]
    public async Task CanonicalGpuPasses_WhenEnvelopeInteropAvailable_DispatchStableCanonicalEnvelopes()
    {
        var interop = new FakeWebGpuCanonicalEnvelopeJsInterop();
        var provider = new global::AIKernel.Wasm.Compute.WebGpuComputeProvider(
            new WebGpuComputeSettings { ForceCpuFallback = false },
            new WebGpuWasmBackend(interop));
        var raw = RawTarget("raw");
        var frame = FrameToken(raw);
        var matrices = Enumerable
            .Range(0, GpuCanonicalLayouts.AisMatrix.MaxItems)
            .Select(_ => Enumerable.Repeat(0.5f, GpuCanonicalLayouts.AisMatrix.Stride).ToArray())
            .Cast<IReadOnlyList<float>>()
            .ToArray();

        await provider.ProcessAsync(new GpuAisthesisInput
        {
            Frame = frame,
            RawFramebuffer = raw,
            Features = new Dictionary<string, bool>(StringComparer.Ordinal) { ["red-panel-detect"] = true }
        }, TestContext.Current.CancellationToken);
        await provider.ReasonAsync(new GpuSpatialReasoningInput
        {
            Frame = frame,
            AisMatrices = matrices,
            StateVector = [0.25f]
        }, TestContext.Current.CancellationToken);
        await provider.ComposeAsync(new GpuHudInput
        {
            Frame = frame,
            HudPanelRects = new float[GpuCanonicalLayouts.HudPanelRect.Stride],
            HudPanelStateVectors = new float[GpuCanonicalLayouts.HudPanelStateVector.Stride],
            EgoRadar = new GpuHudEgoRadarInput
            {
                CompassHeadingDegrees = 90.0f,
                CompassUsable = true,
                Kinesis = new GpuVector2(-0.5f, 0.75f)
            }
        }, TestContext.Current.CancellationToken);

        Assert.Equal(GpuOperationNames.GpuAisthesisRawFrame, interop.LastAisthesisEnvelope?.PassId);
        Assert.Equal("raw", interop.LastAisthesisEnvelope?.Targets.Single().Role);
        Assert.Equal(GpuExecutionModes.BrowserWebGpuCompute, interop.LastAisthesisEnvelope?.Metadata[GpuDiagnosticsMetadataKeys.CanonicalExecutionMode]);
        Assert.Equal("storage-texture-not-ready", interop.LastAisthesisEnvelope?.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPromotionReason]);
        Assert.Equal(GpuOperationNames.GpuSpatialReasoning, interop.LastSpatialEnvelope?.PassId);
        Assert.Equal("ais-matrix:topos", interop.LastSpatialEnvelope?.Buffers[0].Name);
        Assert.Equal(GpuPromotionGates.TraceCandidate, interop.LastSpatialEnvelope?.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPromotionGate]);
        Assert.Equal("state-vector", interop.LastSpatialEnvelope?.Buffers.Last().Name);
        Assert.Equal(GpuOperationNames.GpuHudComposite, interop.LastHudEnvelope?.PassId);
        Assert.Equal(90.0f, interop.LastHudEnvelope?.Scalars["compassHeadingDegrees"]);
        Assert.Equal(0.75f, interop.LastHudEnvelope?.Scalars["kinesisY"]);
        Assert.Equal(GpuPromotionGates.TraceCandidate, interop.LastHudEnvelope?.Metadata[GpuDiagnosticsMetadataKeys.CanonicalPromotionGate]);
        Assert.False(provider.UsingCpuFallback);
    }

    [Fact]
    public async Task KernelExecution_PublishesGpuKernelExecutedEvent()
    {
        var eventBus = new EventBusProvider();
        var backend = "";
        eventBus.Subscribe<Dictionary<string, string>>("GpuKernelExecuted", payload =>
        {
            backend = payload[GpuProviderMetadataKeys.Backend];
            return Task.CompletedTask;
        });

        var provider = new global::AIKernel.Wasm.Compute.WebGpuComputeProvider(
            new WebGpuComputeSettings { ForceCpuFallback = true },
            eventBus: eventBus);
        using var left = await provider.CreateBufferAsync(sizeof(float));
        using var right = await provider.CreateBufferAsync(sizeof(float));
        using var output = await provider.CreateBufferAsync(sizeof(float));

        await provider.WriteBufferAsync(left, MemoryMarshal.AsBytes<float>(new[] { 1.0f }.AsSpan()).ToArray());
        await provider.WriteBufferAsync(right, MemoryMarshal.AsBytes<float>(new[] { 2.0f }.AsSpan()).ToArray());
        await provider.ExecuteKernelAsync(ComputeKernel.CreateVectorAdd(1), left, right, output);

        Assert.Equal("cpu-fallback", backend);
    }

    [Fact]
    public async Task BufferSizeMismatch_Throws()
    {
        var provider = new global::AIKernel.Wasm.Compute.WebGpuComputeProvider();
        using var buffer = await provider.CreateBufferAsync(4);

        await Assert.ThrowsAsync<ArgumentException>(
            () => provider.WriteBufferAsync(buffer, new byte[8]));
    }

    [Fact]
    public async Task Invoker_RejectsUnsupportedOperationFailClosed()
    {
        var invoker = new WebGpuComputeInvoker();

        var result = await invoker.InvokeAsync(new CapabilityInvocationRequest(
            "invoke-1",
            "webgpu.compute",
            "unknown.operation",
            new Dictionary<string, string>(),
            null,
            "sha256:replay",
            new Dictionary<string, string>()),
            TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
        Assert.Equal("WEBGPU_OPERATION_NOT_SUPPORTED", result.ErrorCode);
        Assert.Equal("true", result.Metadata[GpuProviderMetadataKeys.Canonical]);
        Assert.Equal(GpuBackend.WebGpu.ToString(), result.Metadata[GpuProviderMetadataKeys.GpuBackend]);
        Assert.Equal("raw-texture-binding", result.Metadata[GpuProviderMetadataKeys.GpuBypass]);
        Assert.Equal("canonical-envelope-bridge", result.Metadata[GpuProviderMetadataKeys.NativeJsBridge]);
        Assert.True(GpuCanonicalValidation.ValidateBrowserBridgeMetadata(result.Metadata).IsValid);
        Assert.True(GpuCanonicalValidation.ValidateExecutionLayerMetadata(result.Metadata).IsValid);
    }

    [Fact]
    public async Task Invoker_OverwritesSpoofedGpuExecutionMetadata()
    {
        var invoker = new WebGpuComputeInvoker();

        var result = await invoker.InvokeAsync(new CapabilityInvocationRequest(
            "invoke-spoofed-metadata",
            "webgpu.compute",
            GpuOperationNames.ComputeDispatch,
            new Dictionary<string, string>(),
            null,
            "sha256:replay",
            new Dictionary<string, string>
            {
                ["caller_trace"] = "preserve-me",
                [GpuProviderMetadataKeys.Backend] = GpuBackend.Cuda.ToString(),
                [GpuProviderMetadataKeys.GpuBackend] = GpuBackend.Cuda.ToString(),
                [GpuProviderMetadataKeys.GpuBypass] = "native-cuda-buffer-dispatch",
                [GpuProviderMetadataKeys.NativeJsBridge] = "not-required-native-provider",
                [GpuProviderMetadataKeys.PassBridge] = "native-abi",
                [GpuProviderMetadataKeys.ZeroCopyBufferHandling] = "native-cuda-device-buffer"
            }),
            TestContext.Current.CancellationToken);

        Assert.True(result.Succeeded);
        Assert.Equal("preserve-me", result.Metadata["caller_trace"]);
        Assert.Equal("browser-webgpu", result.Metadata[GpuProviderMetadataKeys.Backend]);
        Assert.Equal(GpuBackend.WebGpu.ToString(), result.Metadata[GpuProviderMetadataKeys.GpuBackend]);
        Assert.Equal("raw-texture-binding", result.Metadata[GpuProviderMetadataKeys.GpuBypass]);
        Assert.Equal("canonical-envelope-bridge", result.Metadata[GpuProviderMetadataKeys.NativeJsBridge]);
        Assert.Equal("optional-native-or-js", result.Metadata[GpuProviderMetadataKeys.PassBridge]);
        Assert.Equal("raw-framebuffer-texture", result.Metadata[GpuProviderMetadataKeys.ZeroCopyBufferHandling]);
        Assert.True(GpuCanonicalValidation.ValidateBrowserBridgeMetadata(result.Metadata).IsValid);
        Assert.True(GpuCanonicalValidation.ValidateExecutionLayerMetadata(result.Metadata).IsValid);
    }

    [Fact]
    public void ProviderManifest_IncludesCliSettings()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "webgpu.provider.json");

        Assert.True(File.Exists(path));

        var json = File.ReadAllText(path);
        Assert.Contains("\"cli\"", json);
        Assert.Contains("\"defaultOperation\": \"compute.vector_add\"", json);
        Assert.Contains("\"command\": \"gpu\"", json);
        Assert.Contains("\"version\": \"0.1.3\"", json);
        Assert.Contains("\"gpu.hud.composite\"", json);
        Assert.Contains("\"gpu.aisthesis.raw-frame\"", json);
        Assert.Contains("\"gpu.spatial-reasoning\"", json);
        Assert.Contains("\"gpu.zero-copy.raw-texture\"", json);
        Assert.Contains("\"rawCaptureSource\": \"raw-framebuffer\"", json);
        Assert.Contains("\"passBridge\": \"optional-native-or-js\"", json);
        Assert.Contains("\"gpuBypass\": \"raw-texture-binding\"", json);
        Assert.Contains("\"nativeJsBridge\": \"canonical-envelope-bridge\"", json);
        Assert.Contains("\"aotCompilerHooks\": \"planned-gpu-native-execution\"", json);
        Assert.Contains("\"deterministicFrameSampling\": \"frame-token-index-sample-ticks\"", json);
        Assert.Contains("\"zeroCopyBufferHandling\": \"raw-framebuffer-texture\"", json);
        Assert.Contains("\"aisMatrixOrder\": \"topos,route,threat,zoe\"", json);
        Assert.Contains($"\"envelopeBridge\": \"{WebGpuBrowserBridgeAssets.RuntimeAssetPath}\"", json);
        Assert.Contains($"\"envelopeSchema\": \"{WebGpuBrowserBridgeAssets.Schema}\"", json);
        Assert.Contains($"\"envelopeBridgeFactory\": \"{WebGpuBrowserBridgeAssets.BridgeFactory}\"", json);
        Assert.Contains($"\"envelopeBridgeExecutorFactory\": \"{WebGpuBrowserBridgeAssets.BrowserExecutorFactory}\"", json);
        Assert.Contains($"\"envelopeBridgeGlobal\": \"{WebGpuBrowserBridgeAssets.GlobalObject}\"", json);
        Assert.Contains($"\"browserPassReadiness\": \"{WebGpuBrowserBridgeAssets.PassReadinessShape}\"", json);
    }

    [Fact]
    public void ProviderPackage_IncludesBrowserCanonicalEnvelopeBridge()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Runtime",
            "Browser",
            Path.GetFileName(WebGpuBrowserBridgeAssets.RuntimeAssetPath));

        Assert.True(File.Exists(path));

        var js = File.ReadAllText(path);
        Assert.Contains(WebGpuBrowserBridgeAssets.BridgeFactory, js);
        Assert.Contains(WebGpuBrowserBridgeAssets.BrowserExecutorFactory, js);
        Assert.Contains(WebGpuBrowserBridgeAssets.NullExecutorFactory, js);
        Assert.Contains("dispatchAisthesisEnvelope", js);
        Assert.Contains("dispatchSpatialReasoningEnvelope", js);
        Assert.Contains("dispatchHudCompositeEnvelope", js);
        Assert.Contains("textureRegistry", js);
        Assert.Contains("passExecutors", js);
        Assert.Contains("HasPassExecutors", js);
        Assert.Contains("createStorageBufferFromValues", js);
        Assert.Contains("executeAisthesisComputePass", js);
        Assert.Contains("executeSpatialReasoningComputePass", js);
        Assert.Contains("executeHudCompositeComputePass", js);
        Assert.Contains("createComputePipeline", js);
        Assert.Contains("createFeatureMaskTexture", js);
        Assert.Contains("feature_mask_storage_texture", js);
        Assert.Contains("hud-texture-view-unavailable", js);
        Assert.Contains("readFloatBuffer", js);
        Assert.Contains("__aikernelCanonicalLostObserved", js);
        Assert.Contains("onDeviceLost", js);
        Assert.Contains("raw-texture-not-bound", js);
        Assert.Contains("raw-texture-view-unavailable", js);
        Assert.Contains("pass-executor-not-bound", js);
        Assert.Contains("FallbackCount", js);
        Assert.Contains("LastFallbackReason", js);
        Assert.Contains("Passes", js);
        Assert.Contains("ShaderBound", js);
        Assert.Contains("PipelineCached", js);
        Assert.Contains("ReadyForBuiltIn", js);
        Assert.Contains("InjectedExecutor", js);
        Assert.Contains("execution_mode", js);
        Assert.Contains("promotion_blocked", js);
        Assert.Contains("promotion_candidate_ready", js);
        Assert.Contains("promotion_diagnostic_stable", js);
        Assert.Contains("promotion_reason", js);
        Assert.Contains("valueOf(envelope, \"Metadata\", \"metadata\")", js);
        Assert.Contains("metadataOf", js);
        Assert.Contains("browser-builtin-compute", js);
        Assert.Contains("browser-deterministic-fallback", js);
        Assert.Contains("trace-candidate", js);
        Assert.Contains("authoritative-not-ready", js);
        Assert.Contains("storage-texture-not-ready", js);
        Assert.Contains(WebGpuBrowserBridgeAssets.Schema, js);
        Assert.Contains(WebGpuBrowserBridgeAssets.GlobalObject, js);
    }

    [Fact]
    public void ProviderPackage_BrowserCanonicalEnvelopeBridgeMirrorsCanonicalDiagnosticsValueCatalogs()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Runtime",
            "Browser",
            Path.GetFileName(WebGpuBrowserBridgeAssets.RuntimeAssetPath));

        Assert.True(File.Exists(path));

        var js = File.ReadAllText(path);
        Assert.Contains("GPU_VALUES", js);
        Assert.Contains(GpuExecutionModes.BrowserWebGpuCompute, js);
        Assert.Contains(GpuExecutionModes.DeterministicFallback, js);
        Assert.Contains(GpuPathRoles.Game, js);
        Assert.Contains(GpuPathRoles.Bonsai, js);
        Assert.Contains(GpuPathRoles.Hud, js);
        Assert.Contains(GpuPathRoles.Sensor, js);
        Assert.Contains(GpuCanonicalPilotStates.BrowserBuiltinCompute, js);
        Assert.Contains(GpuCanonicalPilotStates.BrowserDeterministicFallback, js);
        Assert.Contains(GpuCanonicalPilotStates.NotEvaluated, js);
        Assert.Contains(GpuPromotionGates.TraceCandidate, js);
        Assert.Contains(GpuPromotionGates.Fallback, js);
        Assert.Contains(GpuPromotionGates.NotEvaluated, js);
    }

    [Fact]
    public void ProviderPackage_IncludesHudCompositeComputeShaderEntryPoint()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Shaders",
            "Hud",
            "hud-composite.wgsl");

        Assert.True(File.Exists(path));

        var wgsl = File.ReadAllText(path);
        Assert.Contains("@compute @workgroup_size", wgsl);
        Assert.Contains("texture_storage_2d<rgba8unorm, write>", wgsl);
        Assert.Contains("compose_ego_radar_pixel", wgsl);
        Assert.Contains("textureStore", wgsl);
    }

    [Fact]
    public void ProviderPackage_IncludesAisthesisFeatureMaskComputeShaderOutput()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Shaders",
            "Aisthesis",
            "aisthesis.wgsl");

        Assert.True(File.Exists(path));

        var wgsl = File.ReadAllText(path);
        Assert.Contains("@compute @workgroup_size", wgsl);
        Assert.Contains("texture_storage_2d<rgba8unorm, write>", wgsl);
        Assert.Contains("featureMask", wgsl);
        Assert.Contains("textureStore", wgsl);
    }

    [Fact]
    public void BrowserBridgeAssets_DefineStableCanonicalRuntimeBoundary()
    {
        var metadata = WebGpuBrowserBridgeAssets.ToMetadata();
        var validation = GpuCanonicalValidation.ValidateBrowserBridgeMetadata(metadata);

        Assert.Equal("aikernel.gpu.dispatch", WebGpuBrowserBridgeAssets.Schema);
        Assert.Equal("0.1.3", WebGpuBrowserBridgeAssets.Version);
        Assert.Equal("runtime/browser/webgpu-envelope-bridge.js", WebGpuBrowserBridgeAssets.RuntimeAssetPath);
        Assert.Equal("AIKernelWebGpu", WebGpuBrowserBridgeAssets.GlobalObject);
        Assert.Equal("createWebGpuEnvelopeBridge", WebGpuBrowserBridgeAssets.BridgeFactory);
        Assert.Equal("createWebGpuBrowserExecutor", WebGpuBrowserBridgeAssets.BrowserExecutorFactory);
        Assert.Equal(GpuProviderMetadataKeys.CanonicalBrowserPassReadiness, WebGpuBrowserBridgeAssets.PassReadinessMetadataKey);
        Assert.Equal(GpuProviderMetadataKeys.CanonicalEnvelopeBridge, WebGpuBrowserBridgeAssets.EnvelopeBridgeMetadataKey);
        Assert.Equal(GpuProviderMetadataKeys.CanonicalEnvelopeBridgeExecutorFactory, WebGpuBrowserBridgeAssets.EnvelopeBridgeExecutorFactoryMetadataKey);
        Assert.Equal(GpuProviderMetadataKeys.CanonicalEnvelopeBridgeFactory, WebGpuBrowserBridgeAssets.EnvelopeBridgeFactoryMetadataKey);
        Assert.Equal(GpuProviderMetadataKeys.CanonicalEnvelopeBridgeGlobal, WebGpuBrowserBridgeAssets.EnvelopeBridgeGlobalMetadataKey);
        Assert.Equal(GpuProviderMetadataKeys.CanonicalEnvelopeSchema, WebGpuBrowserBridgeAssets.EnvelopeSchemaMetadataKey);
        Assert.Contains("ReadyForBuiltIn", WebGpuBrowserBridgeAssets.PassReadinessShape);
        Assert.Equal(
            [
                WebGpuBrowserBridgeAssets.PassReadinessMetadataKey,
                WebGpuBrowserBridgeAssets.EnvelopeBridgeMetadataKey,
                WebGpuBrowserBridgeAssets.EnvelopeBridgeExecutorFactoryMetadataKey,
                WebGpuBrowserBridgeAssets.EnvelopeBridgeFactoryMetadataKey,
                WebGpuBrowserBridgeAssets.EnvelopeBridgeGlobalMetadataKey,
                WebGpuBrowserBridgeAssets.EnvelopeSchemaMetadataKey
            ],
            metadata.Keys.ToArray());
        Assert.True(validation.IsValid);
    }

    [Fact]
    public void CompatibilityNamespace_ExposesBrowserBridgeAssets()
    {
        Assert.Equal(
            WebGpuBrowserBridgeAssets.RuntimeAssetPath,
            global::AIKernel.Wasm.Comput.WebGpuBrowserBridgeAssets.RuntimeAssetPath);
        Assert.Equal(
            WebGpuBrowserBridgeAssets.BridgeFactory,
            global::AIKernel.Wasm.Comput.WebGpuBrowserBridgeAssets.BridgeFactory);
        Assert.Equal(
            WebGpuBrowserBridgeAssets.BrowserExecutorFactory,
            global::AIKernel.Wasm.Comput.WebGpuBrowserBridgeAssets.BrowserExecutorFactory);
        Assert.Equal(
            WebGpuBrowserBridgeAssets.GlobalObject,
            global::AIKernel.Wasm.Comput.WebGpuBrowserBridgeAssets.GlobalObject);
        Assert.Equal(
            WebGpuBrowserBridgeAssets.PassReadinessMetadataKey,
            global::AIKernel.Wasm.Comput.WebGpuBrowserBridgeAssets.PassReadinessMetadataKey);
        Assert.Equal(
            WebGpuBrowserBridgeAssets.EnvelopeBridgeMetadataKey,
            global::AIKernel.Wasm.Comput.WebGpuBrowserBridgeAssets.EnvelopeBridgeMetadataKey);
        Assert.Equal(
            WebGpuBrowserBridgeAssets.EnvelopeBridgeExecutorFactoryMetadataKey,
            global::AIKernel.Wasm.Comput.WebGpuBrowserBridgeAssets.EnvelopeBridgeExecutorFactoryMetadataKey);
        Assert.Equal(
            WebGpuBrowserBridgeAssets.EnvelopeBridgeFactoryMetadataKey,
            global::AIKernel.Wasm.Comput.WebGpuBrowserBridgeAssets.EnvelopeBridgeFactoryMetadataKey);
        Assert.Equal(
            WebGpuBrowserBridgeAssets.EnvelopeBridgeGlobalMetadataKey,
            global::AIKernel.Wasm.Comput.WebGpuBrowserBridgeAssets.EnvelopeBridgeGlobalMetadataKey);
        Assert.Equal(
            WebGpuBrowserBridgeAssets.EnvelopeSchemaMetadataKey,
            global::AIKernel.Wasm.Comput.WebGpuBrowserBridgeAssets.EnvelopeSchemaMetadataKey);
        Assert.Equal(
            WebGpuBrowserBridgeAssets.PassReadinessShape,
            global::AIKernel.Wasm.Comput.WebGpuBrowserBridgeAssets.PassReadinessShape);
    }

    [Fact]
    public void CompatibilityNamespace_ExposesCanonicalEnvelopeProjectionAndValidation()
    {
        var raw = RawTarget("raw");
        var envelope = global::AIKernel.Wasm.Comput.WebGpuInteropEnvelope.ForAisthesis(new GpuAisthesisInput
        {
            Frame = FrameToken(raw),
            RawFramebuffer = raw,
            Features = new Dictionary<string, bool>(StringComparer.Ordinal)
            {
                ["edge-detect"] = true
            }
        });
        var validation = global::AIKernel.Wasm.Comput.WebGpuInteropEnvelope.ValidateDispatchEnvelope(envelope);

        Assert.Equal(GpuOperationNames.GpuAisthesisRawFrame, envelope.PassId);
        Assert.True(validation.IsValid);
    }

    [Fact]
    public void CanonicalLayoutRegistry_MatchesCanonicalGpuLayouts()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Buffers",
            "Layouts",
            "gpu-layouts.json");

        Assert.True(File.Exists(path));

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        Assert.Equal("aikernel.gpu.layouts.canonical", document.RootElement.GetProperty("schema").GetString());
        Assert.Equal("0.1.3", document.RootElement.GetProperty("version").GetString());
        var layouts = document.RootElement
            .GetProperty("layouts")
            .EnumerateArray()
            .Select(static element => new GpuFlatBufferLayout
            {
                Name = element.GetProperty("name").GetString() ?? "",
                Stride = element.GetProperty("stride").GetInt32(),
                MaxItems = element.GetProperty("maxItems").GetInt32()
            })
            .ToArray();

        var result = GpuCanonicalValidation.ValidateLayouts(layouts);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Settings_MetadataIsDeterministicallyOrdered()
    {
        var settings = new WebGpuComputeSettings
        {
            ForceCpuFallback = true
        };

        Assert.Equal(
            [
                GpuProviderMetadataKeys.AdapterProfile,
                GpuProviderMetadataKeys.AisMatrixOrder,
                GpuProviderMetadataKeys.AotCompilerHooks,
                GpuProviderMetadataKeys.Backend,
                GpuProviderMetadataKeys.CanonicalBrowserPassReadiness,
                GpuProviderMetadataKeys.Canonical,
                GpuProviderMetadataKeys.DeterministicFrameSampling,
                GpuProviderMetadataKeys.CanonicalEnvelopeBridge,
                GpuProviderMetadataKeys.CanonicalEnvelopeBridgeExecutorFactory,
                GpuProviderMetadataKeys.CanonicalEnvelopeBridgeFactory,
                GpuProviderMetadataKeys.CanonicalEnvelopeBridgeGlobal,
                GpuProviderMetadataKeys.CanonicalEnvelopeSchema,
                GpuProviderMetadataKeys.Fallback,
                GpuProviderMetadataKeys.GpuBackend,
                GpuProviderMetadataKeys.GpuBypass,
                GpuProviderMetadataKeys.GpuCapabilities,
                GpuProviderMetadataKeys.NativeJsBridge,
                GpuProviderMetadataKeys.PassBridge,
                GpuProviderMetadataKeys.ProviderFamily,
                GpuProviderMetadataKeys.ProviderRole,
                GpuProviderMetadataKeys.RawCaptureSource,
                GpuProviderMetadataKeys.Version,
                GpuProviderMetadataKeys.ZeroCopyBufferHandling
            ],
            settings.ToMetadata().Keys.ToArray());
        Assert.True(GpuCanonicalValidation.ValidateBrowserBridgeMetadata(settings.ToMetadata()).IsValid);
        Assert.True(GpuCanonicalValidation.ValidateExecutionLayerMetadata(settings.ToMetadata()).IsValid);
        Assert.Equal(WebGpuBrowserBridgeAssets.RuntimeAssetPath, settings.ToMetadata()[WebGpuBrowserBridgeAssets.EnvelopeBridgeMetadataKey]);
        Assert.Equal(WebGpuBrowserBridgeAssets.PassReadinessShape, settings.ToMetadata()[WebGpuBrowserBridgeAssets.PassReadinessMetadataKey]);
    }

    [Fact]
    public void CompatibilityNamespace_DefaultProvider_RemainsAvailable()
    {
        var provider = new global::AIKernel.Wasm.Comput.WebGpuComputeProvider();

        Assert.Equal("webgpu.compute", provider.ProviderId);
        Assert.True(provider.GetCapabilities().SupportsOperation(GpuOperationNames.ComputeDispatch));
    }

    private sealed class FakeWebGpuJsInterop : IWebGpuJsInterop
    {
        private readonly Dictionary<object, byte[]> _buffers = new();

        public bool IsWebGpuSupported() => true;

        public Task WriteBufferAsync(object? buffer, ReadOnlyMemory<byte> data)
        {
            _buffers[RequireBuffer(buffer)] = data.ToArray();
            return Task.CompletedTask;
        }

        public Task<byte[]> ReadBufferAsync(object? buffer, int length)
            => Task.FromResult(_buffers[RequireBuffer(buffer)].Take(length).ToArray());

        public Task ExecuteKernelAsync(string wgsl, IReadOnlyList<object?> buffers, int x, int y, int z)
        {
            var left = MemoryMarshal.Cast<byte, float>(_buffers[RequireBuffer(buffers[0])]);
            var right = MemoryMarshal.Cast<byte, float>(_buffers[RequireBuffer(buffers[1])]);
            var output = new byte[_buffers[RequireBuffer(buffers[0])].Length];
            var outputFloats = MemoryMarshal.Cast<byte, float>(output.AsSpan());
            for (var index = 0; index < left.Length; index++)
            {
                outputFloats[index] = left[index] + right[index];
            }

            _buffers[RequireBuffer(buffers[2])] = output;
            return Task.CompletedTask;
        }

        private static object RequireBuffer(object? buffer)
            => buffer ?? throw new ArgumentNullException(nameof(buffer));
    }

    private sealed class FakeWebGpuCanonicalJsInterop : IWebGpuPassBridgeJsInterop
    {
        private readonly FakeWebGpuJsInterop _compute = new();

        public int AisthesisDispatches { get; private set; }

        public int SpatialDispatches { get; private set; }

        public int HudDispatches { get; private set; }

        public bool IsWebGpuSupported() => true;

        public Task WriteBufferAsync(object? buffer, ReadOnlyMemory<byte> data)
            => _compute.WriteBufferAsync(buffer, data);

        public Task<byte[]> ReadBufferAsync(object? buffer, int length)
            => _compute.ReadBufferAsync(buffer, length);

        public Task ExecuteKernelAsync(string wgsl, IReadOnlyList<object?> buffers, int x, int y, int z)
            => _compute.ExecuteKernelAsync(wgsl, buffers, x, y, z);

        public Task<GpuAisthesisOutput?> DispatchAisthesisAsync(
            GpuAisthesisInput input,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            AisthesisDispatches++;
            return Task.FromResult<GpuAisthesisOutput?>(new GpuAisthesisOutput
            {
                Frame = input.Frame,
                FeatureVector = [42.0f],
                MaskTexture = new GpuFrameTarget
                {
                    TargetId = "js-mask",
                    Kind = GpuFrameTargetKind.FeatureMask,
                    Backend = GpuBackend.WebGpu,
                    Width = input.RawFramebuffer.Width,
                    Height = input.RawFramebuffer.Height,
                    ZeroCopy = true
                },
                Diagnostics = new GpuDiagnosticsPathInfo
                {
                    Backend = "WebGpu",
                    ZeroCopy = true,
                    PassId = "js-aisthesis"
                }
            });
        }

        public Task<GpuSpatialReasoningOutput?> DispatchSpatialReasoningAsync(
            GpuSpatialReasoningInput input,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SpatialDispatches++;
            return Task.FromResult<GpuSpatialReasoningOutput?>(new GpuSpatialReasoningOutput
            {
                Frame = input.Frame,
                SpatialVector = [24.0f],
                Diagnostics = new GpuDiagnosticsPathInfo
                {
                    Backend = "WebGpu",
                    ZeroCopy = true,
                    PassId = "js-spatial"
                }
            });
        }

        public Task<GpuFrameTarget?> DispatchHudCompositeAsync(
            GpuHudInput input,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            HudDispatches++;
            return Task.FromResult<GpuFrameTarget?>(new GpuFrameTarget
            {
                TargetId = "js-hud",
                Kind = GpuFrameTargetKind.HudCompositeOffscreen,
                Backend = GpuBackend.WebGpu,
                Width = input.Frame.RawTarget.Width,
                Height = input.Frame.RawTarget.Height,
                ZeroCopy = true
            });
        }
    }

    private sealed class FakeWebGpuCanonicalEnvelopeJsInterop : IWebGpuEnvelopeJsInterop
    {
        private readonly FakeWebGpuJsInterop _compute = new();

        public WebGpuDispatchEnvelope? LastAisthesisEnvelope { get; private set; }

        public WebGpuDispatchEnvelope? LastSpatialEnvelope { get; private set; }

        public WebGpuDispatchEnvelope? LastHudEnvelope { get; private set; }

        public bool IsWebGpuSupported() => true;

        public Task WriteBufferAsync(object? buffer, ReadOnlyMemory<byte> data)
            => _compute.WriteBufferAsync(buffer, data);

        public Task<byte[]> ReadBufferAsync(object? buffer, int length)
            => _compute.ReadBufferAsync(buffer, length);

        public Task ExecuteKernelAsync(string wgsl, IReadOnlyList<object?> buffers, int x, int y, int z)
            => _compute.ExecuteKernelAsync(wgsl, buffers, x, y, z);

        public Task<GpuAisthesisOutput?> DispatchAisthesisEnvelopeAsync(
            WebGpuDispatchEnvelope envelope,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastAisthesisEnvelope = envelope;
            var raw = RequireTarget(envelope, "raw");
            return Task.FromResult<GpuAisthesisOutput?>(new GpuAisthesisOutput
            {
                Frame = FrameFrom(envelope, raw),
                FeatureVector = [12.0f],
                MaskTexture = new GpuFrameTarget
                {
                    TargetId = $"{raw.TargetId}:mask",
                    Kind = GpuFrameTargetKind.FeatureMask,
                    Backend = GpuBackend.WebGpu,
                    Width = raw.Width,
                    Height = raw.Height,
                    ZeroCopy = true
                },
                Diagnostics = new GpuDiagnosticsPathInfo
                {
                    Backend = "WebGpu",
                    ZeroCopy = true,
                    PassId = envelope.PassId
                }
            });
        }

        public Task<GpuSpatialReasoningOutput?> DispatchSpatialReasoningEnvelopeAsync(
            WebGpuDispatchEnvelope envelope,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastSpatialEnvelope = envelope;
            var raw = RequireTarget(envelope, "raw");
            return Task.FromResult<GpuSpatialReasoningOutput?>(new GpuSpatialReasoningOutput
            {
                Frame = FrameFrom(envelope, raw),
                SpatialVector = [16.0f],
                Diagnostics = new GpuDiagnosticsPathInfo
                {
                    Backend = "WebGpu",
                    ZeroCopy = true,
                    PassId = envelope.PassId
                }
            });
        }

        public Task<GpuFrameTarget?> DispatchHudCompositeEnvelopeAsync(
            WebGpuDispatchEnvelope envelope,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastHudEnvelope = envelope;
            var raw = RequireTarget(envelope, "raw");
            return Task.FromResult<GpuFrameTarget?>(new GpuFrameTarget
            {
                TargetId = "hud-from-envelope",
                Kind = GpuFrameTargetKind.HudCompositeOffscreen,
                Backend = GpuBackend.WebGpu,
                Width = raw.Width,
                Height = raw.Height,
                ZeroCopy = true
            });
        }

        private static GpuFrameTarget RequireTarget(WebGpuDispatchEnvelope envelope, string role)
        {
            var target = envelope.Targets.Single(item => item.Role == role);
            return new GpuFrameTarget
            {
                TargetId = target.TargetId,
                Kind = target.Kind,
                Backend = target.Backend,
                Width = target.Width,
                Height = target.Height,
                PixelFormat = target.PixelFormat,
                ZeroCopy = target.ZeroCopy
            };
        }

        private static GpuFrameToken FrameFrom(WebGpuDispatchEnvelope envelope, GpuFrameTarget raw)
            => new()
            {
                FrameId = envelope.Frame.FrameId,
                FrameIndex = envelope.Frame.FrameIndex,
                SampleTicks = envelope.Frame.SampleTicks,
                RawTarget = raw
            };
    }

    private static GpuFrameTarget RawTarget(string id)
        => new()
        {
            TargetId = id,
            Kind = GpuFrameTargetKind.RawFramebuffer,
            Backend = GpuBackend.WebGpu,
            Width = 320,
            Height = 200,
            ZeroCopy = true
        };

    private static GpuFrameToken FrameToken(GpuFrameTarget raw)
        => new()
        {
            FrameId = "frame-1",
            FrameIndex = 1,
            SampleTicks = 100,
            RawTarget = raw
        };
}
