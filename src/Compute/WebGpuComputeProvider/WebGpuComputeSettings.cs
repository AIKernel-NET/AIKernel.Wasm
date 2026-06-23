using AIKernel.Common.Results;
using AIKernel.Dtos.Gpu;
using AIKernel.Enums;

namespace AIKernel.Wasm.Compute;

/// <summary>
/// [EN] Settings for the WebGPU compute provider boundary.
/// [JA] WebGPU compute Provider 境界の設定です。
/// </summary>
public record WebGpuComputeSettings
{
    /// <summary>[EN] Provider id. [JA] Provider id です。</summary>
    public string ProviderId { get; init; } = "webgpu.compute";

    /// <summary>[EN] Human-readable provider name. [JA] 人間可読な Provider 名です。</summary>
    public string Name { get; init; } = "WebGPU Compute Provider";

    /// <summary>[EN] Provider contract version. [JA] Provider 契約 version です。</summary>
    public string Version { get; init; } = "0.1.3";

    /// <summary>[EN] WebGPU adapter profile used for metadata. [JA] metadata に使用する WebGPU adapter profile です。</summary>
    public string AdapterProfile { get; init; } = "webgpu-browser";

    /// <summary>[EN] Whether CPU fallback is forced even when a GPU backend exists. [JA] GPU backend があっても CPU fallback を強制するかどうかです。</summary>
    public bool ForceCpuFallback { get; init; }

    /// <summary>[EN] Optional WebGPU backend name. [JA] 任意の WebGPU backend 名です。</summary>
    public string? BackendName { get; init; } = "browser-webgpu";

    /// <summary>[EN] Returns deterministic metadata for capability export. [JA] capability export 用の決定論的 metadata を返します。</summary>
    public IReadOnlyDictionary<string, string> ToMetadata()
    {
        var metadata = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [GpuProviderMetadataKeys.AdapterProfile] = AdapterProfile,
            [GpuProviderMetadataKeys.AisMatrixOrder] = "topos,route,threat,zoe",
            [GpuProviderMetadataKeys.AotCompilerHooks] = "planned-gpu-native-execution",
            [GpuProviderMetadataKeys.DeterministicFrameSampling] = "frame-token-index-sample-ticks",
            [GpuProviderMetadataKeys.Fallback] = FallbackMode(ForceCpuFallback),
            [GpuProviderMetadataKeys.GpuBackend] = GpuBackend.WebGpu.ToString(),
            [GpuProviderMetadataKeys.GpuBypass] = "raw-texture-binding",
            [GpuProviderMetadataKeys.GpuCapabilities] = (
                GpuProviderCapabilities.SupportsCompute |
                GpuProviderCapabilities.SupportsHudComposite |
                GpuProviderCapabilities.SupportsAisthesis |
                GpuProviderCapabilities.SupportsSpatialReasoning |
                GpuProviderCapabilities.SupportsZeroCopyRawTexture |
                GpuProviderCapabilities.SupportsNativeValidation |
                GpuProviderCapabilities.SupportsCpuFallback |
                GpuProviderCapabilities.SupportsFrameDiagnostics).ToString(),
            [GpuProviderMetadataKeys.NativeJsBridge] = "rev3-envelope-bridge",
            [GpuProviderMetadataKeys.PassBridge] = "optional-native-or-js",
            [GpuProviderMetadataKeys.ProviderFamily] = "aikernel.gpu.rev3",
            [GpuProviderMetadataKeys.ProviderRole] = "webgpu-browser",
            [GpuProviderMetadataKeys.RawCaptureSource] = "raw-framebuffer",
            [GpuProviderMetadataKeys.Rev3] = "true",
            [GpuProviderMetadataKeys.Version] = Version,
            [GpuProviderMetadataKeys.ZeroCopyBufferHandling] = "raw-framebuffer-texture"
        };

        if (!string.IsNullOrWhiteSpace(BackendName))
        {
            metadata[GpuProviderMetadataKeys.Backend] = BackendName;
        }

        foreach (var pair in WebGpuRev3BrowserBridgeAssets.ToMetadata())
        {
            metadata[pair.Key] = pair.Value;
        }

        return metadata;
    }

    private static string FallbackMode(bool forceCpuFallback)
        => MonadicDecision.SelectText(forceCpuFallback, "webgpu-or-cpu", "cpu");
}
