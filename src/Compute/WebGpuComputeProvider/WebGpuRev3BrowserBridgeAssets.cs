namespace AIKernel.Wasm.Compute;

using AIKernel.Dtos.Gpu;

/// <summary>
/// [EN] Canonical asset names for the browser-side rev3 WebGPU envelope bridge.
/// [JA] browser-side rev3 WebGPU envelope bridge 用の canonical asset 名です。
/// </summary>
public static class WebGpuRev3BrowserBridgeAssets
{
    /// <summary>[EN] Stable rev3 dispatch envelope schema. [JA] 安定した rev3 dispatch envelope schema です。</summary>
    public const string Schema = "aikernel.gpu.rev3.dispatch";

    /// <summary>[EN] Canonical rev3 version. [JA] canonical rev3 version です。</summary>
    public const string Version = "0.1.3";

    /// <summary>[EN] Runtime path packed into the WebGPU provider package. [JA] WebGPU provider package に同梱される runtime path です。</summary>
    public const string RuntimeAssetPath = "runtime/browser/webgpu-rev3-envelope-bridge.js";

    /// <summary>[EN] Global object exposed by the bridge for non-module script consumers. [JA] non-module script consumer 用に bridge が公開する global object です。</summary>
    public const string GlobalObject = "AIKernelWebGpuRev3";

    /// <summary>[EN] Factory function for the browser rev3 envelope bridge. [JA] browser rev3 envelope bridge の factory function です。</summary>
    public const string BridgeFactory = "createWebGpuRev3EnvelopeBridge";

    /// <summary>[EN] Factory function for the browser WebGPU executor skeleton. [JA] browser WebGPU executor skeleton の factory function です。</summary>
    public const string BrowserExecutorFactory = "createWebGpuRev3BrowserExecutor";

    /// <summary>[EN] Factory function for the deterministic null executor. [JA] deterministic null executor の factory function です。</summary>
    public const string NullExecutorFactory = "createNullWebGpuRev3Executor";

    /// <summary>[EN] Metadata key describing the browser executor per-pass readiness diagnostics shape. [JA] browser executor の pass 別 readiness diagnostics 形状を示す metadata key です。</summary>
    public const string PassReadinessMetadataKey = GpuProviderMetadataKeys.Rev3BrowserPassReadiness;

    /// <summary>[EN] Metadata key for the packaged browser rev3 bridge runtime path. [JA] packaged browser rev3 bridge runtime path 用 metadata key です。</summary>
    public const string EnvelopeBridgeMetadataKey = GpuProviderMetadataKeys.Rev3EnvelopeBridge;

    /// <summary>[EN] Metadata key for the browser WebGPU executor factory. [JA] browser WebGPU executor factory 用 metadata key です。</summary>
    public const string EnvelopeBridgeExecutorFactoryMetadataKey = GpuProviderMetadataKeys.Rev3EnvelopeBridgeExecutorFactory;

    /// <summary>[EN] Metadata key for the rev3 envelope bridge factory. [JA] rev3 envelope bridge factory 用 metadata key です。</summary>
    public const string EnvelopeBridgeFactoryMetadataKey = GpuProviderMetadataKeys.Rev3EnvelopeBridgeFactory;

    /// <summary>[EN] Metadata key for the rev3 browser bridge global object. [JA] rev3 browser bridge global object 用 metadata key です。</summary>
    public const string EnvelopeBridgeGlobalMetadataKey = GpuProviderMetadataKeys.Rev3EnvelopeBridgeGlobal;

    /// <summary>[EN] Metadata key for the rev3 dispatch envelope schema. [JA] rev3 dispatch envelope schema 用 metadata key です。</summary>
    public const string EnvelopeSchemaMetadataKey = GpuProviderMetadataKeys.Rev3EnvelopeSchema;

    /// <summary>[EN] Stable per-pass readiness diagnostics shape exposed by the browser executor. [JA] browser executor が公開する pass 別 readiness diagnostics の安定形状です。</summary>
    public const string PassReadinessShape = "Passes.{Aisthesis,SpatialReasoning,HudComposite}:ShaderBound,PipelineCached,BuiltInExecutor,InjectedExecutor,ReadyForBuiltIn";

    /// <summary>[EN] Returns deterministic metadata keys for provider manifests and capability descriptors. [JA] provider manifest と capability descriptor 用の deterministic metadata key を返します。</summary>
    public static IReadOnlyDictionary<string, string> ToMetadata()
        => new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            [PassReadinessMetadataKey] = PassReadinessShape,
            [EnvelopeBridgeMetadataKey] = RuntimeAssetPath,
            [EnvelopeBridgeExecutorFactoryMetadataKey] = BrowserExecutorFactory,
            [EnvelopeBridgeFactoryMetadataKey] = BridgeFactory,
            [EnvelopeBridgeGlobalMetadataKey] = GlobalObject,
            [EnvelopeSchemaMetadataKey] = Schema
        };
}
