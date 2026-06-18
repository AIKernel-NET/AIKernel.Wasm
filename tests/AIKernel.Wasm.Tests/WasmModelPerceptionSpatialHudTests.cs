namespace AIKernel.Wasm.Tests;

using AIKernel.Dtos.Frame;
using AIKernel.Dtos.Perception;
using AIKernel.Dtos.Providers;
using AIKernel.Enums;
using AIKernel.Enums.Perception;
using AIKernel.Wasm.Audio;
using AIKernel.Wasm.Hud;
using AIKernel.Wasm.Models;
using AIKernel.Wasm.Perception;
using AIKernel.Wasm.Spatial;

/// <summary>
/// [EN] Tests WASM model, perception, spatial cognition, and HUD surfaces.
/// [JA] WASM model / perception / spatial cognition / HUD surface をテストします。
/// </summary>
public sealed class WasmModelPerceptionSpatialHudTests
{
    /// <summary>
    /// [EN] Verifies descriptor-driven model execution returns a structured failure when no kernel is supplied.
    /// [JA] kernel が供給されない場合に descriptor-driven model execution が structured failure を返すことを検証します。
    /// </summary>
    [Fact]
    public async Task WebGpuResidentModelProvider_MissingKernel_ReturnsStructuredFailure()
    {
        var provider = new WebGpuResidentModelProvider();

        var result = await provider.ExecuteModelAsync(
            new WebGpuResidentModelExecutionRequest
            {
                Descriptor = new WebGpuResidentModelDescriptor
                {
                    ModelId = "test-model",
                    ModelFamily = "test-family",
                    ManifestRef = "models/test.json"
                }
            },
            TestContext.Current.CancellationToken);

        Assert.False(result.IsFailure);
        Assert.False(result.Value!.Succeeded);
        Assert.Equal("WASM_MODEL_KERNEL_REQUIRED", result.Value.ErrorCode);
    }

    /// <summary>
    /// [EN] Verifies frame perception emits deterministic frame signals.
    /// [JA] frame perception が deterministic frame signal を出力することを検証します。
    /// </summary>
    [Fact]
    public async Task WasmFramePerceptionProvider_FrameSnapshot_ReturnsFrameSignals()
    {
        var provider = new WasmFramePerceptionProvider();

        var result = await provider.AnalyzeAsync(
            new FrameSnapshot
            {
                FrameId = "frame-1",
                SourceId = "surface-1",
                FrameIndex = 7,
                FrameHash = "abc",
                Buffer = new FrameBufferDescriptor
                {
                    Width = 320,
                    Height = 200,
                    Stride = 320,
                    PixelFormat = FramePixelFormat.Rgba32,
                    ZeroCopyHint = true
                },
                Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["byteLength"] = "256000"
                }
            },
            new FramePerceptionOptions(),
            new ProviderExecutionContext
            {
                ExecutionId = "exec"
            },
            TestContext.Current.CancellationToken);

        Assert.Equal("surface-1", result.SurfaceId);
        Assert.Contains(result.Signals, signal => signal.SignalId == "frame.hash");
        Assert.Contains(result.Signals, signal => signal.SignalId == "frame.zero_copy_hint");
    }

    /// <summary>
    /// [EN] Verifies auditory perception emits provider-neutral PCM signals.
    /// [JA] auditory perception が provider-neutral な PCM signal を出力することを検証します。
    /// </summary>
    [Fact]
    public async Task WasmAuditoryPerceptionProvider_PcmFrame_ReturnsAudioSignals()
    {
        var provider = new WasmAuditoryPerceptionProvider();

        var result = await provider.AnalyzeAsync(
            new WasmAuditoryPerceptionRequest
            {
                ObservationId = "audio-obs",
                Frame = new WebAudioCaptureFrame
                {
                    Buffer = new WebAudioPcmBuffer
                    {
                        Payload = [1, 2, 3, 4],
                        SampleRate = 48000,
                        Channels = 2,
                        SampleFormat = "f32"
                    }
                }
            },
            TestContext.Current.CancellationToken);

        Assert.True(result.Succeeded);
        Assert.Contains(result.Signals, signal => signal.SignalId == "audio.sample_rate");
        Assert.Contains(result.Signals, signal => signal.SignalId == "audio.left_energy");
        Assert.Contains(result.Signals, signal => signal.SignalId == "audio.balance");
        Assert.True(result.Metadata.ContainsKey("eventDetected"));
    }

    /// <summary>
    /// [EN] Verifies spatial cognition composes visual and auditory perception without scenario semantics.
    /// [JA] spatial cognition が scenario semantics なしで visual / auditory perception を合成することを検証します。
    /// </summary>
    [Fact]
    public async Task WasmSpatialCognitionProvider_Perceptions_ReturnsSnapshot()
    {
        var provider = new WasmSpatialCognitionProvider();

        var result = await provider.BuildSnapshotAsync(
            new WasmSpatialCognitionRequest
            {
                RequestId = "spatial",
                VisualPerceptions =
                [
                    new FramePerceptionResult
                    {
                        ObservationId = "visual",
                        FrameId = "frame",
                        FrameIndex = 1
                    }
                ],
                AuditoryPerceptions =
                [
                    new WasmAuditoryPerceptionResult
                    {
                        Succeeded = true,
                        ObservationId = "audio",
                        Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["leftEnergy"] = "0.1",
                            ["rightEnergy"] = "0.3"
                        }
                    }
                ],
                ProjectionInput = new WasmSpatialProjectionInput
                {
                    Player = new WasmSpatialPoint(0, 0),
                    Centroid = new WasmSpatialPoint(0, 1),
                    CorrectionGain = 0.5,
                    HudCenter = new WasmSpatialPoint(0.5, 0.45),
                    HudRadius = 0.3
                }
            },
            TestContext.Current.CancellationToken);

        Assert.True(result.Succeeded);
        Assert.Contains(result.Signals, signal => signal.SignalId == "spatial.fused_direction");
        Assert.Equal("0.25", result.Metadata["fusedDirection"]);
        Assert.Contains(result.Signals, signal => signal.SignalId == "spatial.hud_position");
        Assert.True(result.Metadata.ContainsKey("hudX"));
    }

    /// <summary>
    /// [EN] Verifies spatial cognition carries ASCII-safe sensor concepts and retry intent.
    /// [JA] spatial cognition が ASCII-safe な sensor concept と retry intent を保持することを検証します。
    /// </summary>
    [Fact]
    public async Task WasmSpatialCognitionProvider_HealthSensor_ReturnsRetryIntent()
    {
        var provider = new WasmSpatialCognitionProvider();

        var result = await provider.BuildSnapshotAsync(
            new WasmSpatialCognitionRequest
            {
                RequestId = "spatial-health",
                VisualPerceptions =
                [
                    new FramePerceptionResult
                    {
                        ObservationId = "visual",
                        FrameId = "frame",
                        FrameIndex = 1
                    }
                ],
                SensorInputs = new Dictionary<string, WasmSensorStateDescriptor>(StringComparer.Ordinal)
                {
                    ["health"] = new()
                    {
                        Enabled = true,
                        Observed = true,
                        Confidence = 0.93,
                        Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["likelyDead"] = "true",
                            ["zeroScore"] = "0.91"
                        }
                    }
                }
            },
            TestContext.Current.CancellationToken);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.RetryIntent);
        Assert.True(result.RetryIntent.Requested);
        Assert.Equal("health-death", result.RetryIntent.ReasonCode);
        Assert.Equal("Aisthesis", result.SensorInputs["health"].ConceptName);
        Assert.Equal("True", result.Metadata["retryRequested"]);
    }

    /// <summary>
    /// [EN] Verifies resident perception algorithms expose WebGPU descriptors with CPU fallback results.
    /// [JA] resident perception algorithm が WebGPU descriptor と CPU fallback result を公開することを検証します。
    /// </summary>
    [Fact]
    public async Task WasmResidentPerceptionAlgorithmLibrary_ValidInputs_ReturnsDescriptorsAndFallbackResults()
    {
        var library = new WasmResidentPerceptionAlgorithmLibrary();

        var hsv = await library.CreateHsvThresholdMaskAsync(
            new WasmHsvThresholdMaskRequest
            {
                Buffer = new WasmRgbBuffer
                {
                    Width = 2,
                    Height = 1,
                    RgbBytes = [255, 80, 20, 20, 20, 20]
                },
                MinHue = 0,
                MaxHue = 40,
                MinSaturation = 0.4,
                IgnoreValue = true
            },
            TestContext.Current.CancellationToken);
        var pool = await library.MaxPoolAsync(
            new WasmMaxPoolingRequest
            {
                Buffer = new WasmScalarBuffer
                {
                    Width = 4,
                    Height = 4,
                    Values = [0, 0, 0.2, 0.1, 0, 0.9, 0, 0, 0.1, 0, 0.8, 0, 0, 0, 0, 0.7]
                },
                OutputWidth = 2,
                OutputHeight = 2
            },
            TestContext.Current.CancellationToken);
        var morphology = await library.ApplyMorphologyAsync(
            new WasmMorphologyRequest
            {
                Buffer = new WasmScalarBuffer
                {
                    Width = 3,
                    Height = 3,
                    Values = [0, 0, 0, 0, 1, 0, 0, 0, 0]
                },
                Operation = "dilation"
            },
            TestContext.Current.CancellationToken);
        var flow = await library.EstimateDenseOpticalFlowAsync(
            new WasmDenseOpticalFlowRequest
            {
                Previous = new WasmScalarBuffer { Width = 2, Height = 2, Values = [0, 1, 0, 0] },
                Current = new WasmScalarBuffer { Width = 2, Height = 2, Values = [0, 0, 1, 0] }
            },
            TestContext.Current.CancellationToken);
        var spectrum = await library.ComputeAudioSpectrumAsync(
            new WasmAudioSpectrumRequest
            {
                Buffer = new WasmPcmBuffer { Samples = [0, 1, 0, -1, 0, 1, 0, -1], Channels = 1 },
                MaxFftSize = 8
            },
            TestContext.Current.CancellationToken);

        Assert.True(hsv.Succeeded);
        Assert.Equal("hsv-threshold-mask", hsv.Kernel.AlgorithmName);
        Assert.Equal([1, 0], hsv.Mask);
        Assert.True(pool.Succeeded);
        Assert.Equal("max-pooling-downsample", pool.Kernel.AlgorithmName);
        Assert.Equal(4, pool.Buffer.Values.Count);
        Assert.True(morphology.Succeeded);
        Assert.Equal("morphology", morphology.Kernel.AlgorithmName);
        Assert.All(morphology.Buffer.Values, value => Assert.Equal(1, value));
        Assert.True(flow.Succeeded);
        Assert.Equal("dense-optical-flow", flow.Kernel.AlgorithmName);
        Assert.Equal(4, flow.Vectors.Count);
        Assert.True(spectrum.Succeeded);
        Assert.Equal("audio-fft-spectrum", spectrum.Kernel.AlgorithmName);
        Assert.NotEmpty(spectrum.Magnitudes);
    }

    /// <summary>
    /// [EN] Verifies HUD and overlay providers generate DTOs without rendering.
    /// [JA] HUD / overlay Provider が rendering なしで DTO を生成することを検証します。
    /// </summary>
    [Fact]
    public async Task WasmHudProviders_Metadata_ReturnDtoCarriers()
    {
        var hud = new WasmHudSignalProvider();
        var overlay = new WasmOverlayAnnotationProvider();

        var signals = await hud.ExtractAsync(
            new HudSignalRequest
            {
                ObservationId = "obs",
                Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["hud.health"] = "42"
                }
            },
            TestContext.Current.CancellationToken);
        var annotations = await overlay.BuildOverlayAsync(
            new OverlayAnnotationRequest
            {
                ObservationId = "obs",
                Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["overlay.text"] = "door"
                }
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HudSignalKind.Health, signals.Signals[0].Kind);
        Assert.Equal(OverlayShapeKind.Text, annotations.Annotations[0].ShapeKind);
    }
}
