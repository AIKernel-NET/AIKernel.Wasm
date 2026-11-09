namespace AIKernel.Wasm.Comput;

using AIKernel.Abstractions.Compute;
using AIKernel.Abstractions.Events;
using AIKernel.Abstractions.Providers;
using AIKernel.Dtos.Capabilities;
using AIKernel.Dtos.Gpu;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// [EN] Compatibility alias for the corrected AIKernel.Wasm.Compute WebGPU backend interface.
/// [JA] 修正済み AIKernel.Wasm.Compute WebGPU backend interface の互換 alias です。
/// </summary>
public interface IWebGpuBackend : AIKernel.Wasm.Compute.IWebGpuBackend;

/// <summary>
/// [EN] Compatibility alias for the corrected AIKernel.Wasm.Compute canonical pass bridge backend interface.
/// [JA] 修正済み AIKernel.Wasm.Compute canonical pass bridge backend interface の互換 alias です。
/// </summary>
public interface IWebGpuPassBridgeBackend : IWebGpuBackend, AIKernel.Wasm.Compute.IWebGpuPassBridgeBackend;

/// <summary>
/// [EN] Compatibility alias for the corrected AIKernel.Wasm.Compute WebGPU JS interop interface.
/// [JA] 修正済み AIKernel.Wasm.Compute WebGPU JS interop interface の互換 alias です。
/// </summary>
public interface IWebGpuJsInterop : AIKernel.Wasm.Compute.IWebGpuJsInterop;

/// <summary>
/// [EN] Compatibility alias for the corrected AIKernel.Wasm.Compute canonical pass bridge JS interop interface.
/// [JA] 修正済み AIKernel.Wasm.Compute canonical pass bridge JS interop interface の互換 alias です。
/// </summary>
public interface IWebGpuPassBridgeJsInterop : IWebGpuJsInterop, AIKernel.Wasm.Compute.IWebGpuPassBridgeJsInterop;

/// <summary>
/// [EN] Compatibility alias for the corrected AIKernel.Wasm.Compute canonical envelope JS interop interface.
/// [JA] 修正済み AIKernel.Wasm.Compute canonical envelope JS interop interface の互換 alias です。
/// </summary>
public interface IWebGpuEnvelopeJsInterop : IWebGpuJsInterop, AIKernel.Wasm.Compute.IWebGpuEnvelopeJsInterop;

/// <summary>
/// [EN] Compatibility wrapper for corrected canonical dispatch envelope projection and validation.
/// [JA] 修正済み canonical dispatch envelope 投影と検証の互換 wrapper です。
/// </summary>
public static class WebGpuInteropEnvelope
{
    /// <summary>[EN] Projects GPU Aisthesis input into a stable canonical dispatch envelope. [JA] GPU Aisthesis input を安定した canonical dispatch envelope に投影します。</summary>
    public static AIKernel.Wasm.Compute.WebGpuDispatchEnvelope ForAisthesis(GpuAisthesisInput input)
        => AIKernel.Wasm.Compute.WebGpuInteropEnvelope.ForAisthesis(input);

    /// <summary>[EN] Projects GPU Spatial Reasoning input into a stable canonical dispatch envelope. [JA] GPU Spatial Reasoning input を安定した canonical dispatch envelope に投影します。</summary>
    public static AIKernel.Wasm.Compute.WebGpuDispatchEnvelope ForSpatialReasoning(GpuSpatialReasoningInput input)
        => AIKernel.Wasm.Compute.WebGpuInteropEnvelope.ForSpatialReasoning(input);

    /// <summary>[EN] Projects GPU HUD input into a stable canonical dispatch envelope. [JA] GPU HUD input を安定した canonical dispatch envelope に投影します。</summary>
    public static AIKernel.Wasm.Compute.WebGpuDispatchEnvelope ForHudComposite(GpuHudInput input)
        => AIKernel.Wasm.Compute.WebGpuInteropEnvelope.ForHudComposite(input);

    /// <summary>[EN] Validates a stable canonical dispatch envelope before bridge handoff. [JA] bridge handoff 前に安定した canonical dispatch envelope を検証します。</summary>
    public static GpuValidationResult ValidateDispatchEnvelope(AIKernel.Wasm.Compute.WebGpuDispatchEnvelope envelope)
        => AIKernel.Wasm.Compute.WebGpuInteropEnvelope.ValidateDispatchEnvelope(envelope);
}

/// <summary>
/// [EN] Compatibility wrapper for corrected browser canonical bridge asset names.
/// [JA] 修正済み browser canonical bridge asset 名の互換 wrapper です。
/// </summary>
public static class WebGpuBrowserBridgeAssets
{
    /// <summary>[EN] Stable canonical dispatch envelope schema. [JA] 安定した canonical dispatch envelope schema です。</summary>
    public const string Schema = AIKernel.Wasm.Compute.WebGpuBrowserBridgeAssets.Schema;

    /// <summary>[EN] Canonical version. [JA] canonical version です。</summary>
    public const string Version = AIKernel.Wasm.Compute.WebGpuBrowserBridgeAssets.Version;

    /// <summary>[EN] Runtime path packed into the WebGPU provider package. [JA] WebGPU provider package に同梱される runtime path です。</summary>
    public const string RuntimeAssetPath = AIKernel.Wasm.Compute.WebGpuBrowserBridgeAssets.RuntimeAssetPath;

    /// <summary>[EN] Global object exposed by the browser bridge. [JA] browser bridge が公開する global object です。</summary>
    public const string GlobalObject = AIKernel.Wasm.Compute.WebGpuBrowserBridgeAssets.GlobalObject;

    /// <summary>[EN] Factory function for the browser canonical envelope bridge. [JA] browser canonical envelope bridge の factory function です。</summary>
    public const string BridgeFactory = AIKernel.Wasm.Compute.WebGpuBrowserBridgeAssets.BridgeFactory;

    /// <summary>[EN] Factory function for the browser WebGPU executor skeleton. [JA] browser WebGPU executor skeleton の factory function です。</summary>
    public const string BrowserExecutorFactory = AIKernel.Wasm.Compute.WebGpuBrowserBridgeAssets.BrowserExecutorFactory;

    /// <summary>[EN] Factory function for the deterministic null executor. [JA] deterministic null executor の factory function です。</summary>
    public const string NullExecutorFactory = AIKernel.Wasm.Compute.WebGpuBrowserBridgeAssets.NullExecutorFactory;

    /// <summary>[EN] Metadata key describing the browser executor per-pass readiness diagnostics shape. [JA] browser executor の pass 別 readiness diagnostics 形状を示す metadata key です。</summary>
    public const string PassReadinessMetadataKey = AIKernel.Wasm.Compute.WebGpuBrowserBridgeAssets.PassReadinessMetadataKey;

    /// <summary>[EN] Metadata key for the packaged browser canonical bridge runtime path. [JA] packaged browser canonical bridge runtime path 用 metadata key です。</summary>
    public const string EnvelopeBridgeMetadataKey = AIKernel.Wasm.Compute.WebGpuBrowserBridgeAssets.EnvelopeBridgeMetadataKey;

    /// <summary>[EN] Metadata key for the browser WebGPU executor factory. [JA] browser WebGPU executor factory 用 metadata key です。</summary>
    public const string EnvelopeBridgeExecutorFactoryMetadataKey = AIKernel.Wasm.Compute.WebGpuBrowserBridgeAssets.EnvelopeBridgeExecutorFactoryMetadataKey;

    /// <summary>[EN] Metadata key for the canonical envelope bridge factory. [JA] canonical envelope bridge factory 用 metadata key です。</summary>
    public const string EnvelopeBridgeFactoryMetadataKey = AIKernel.Wasm.Compute.WebGpuBrowserBridgeAssets.EnvelopeBridgeFactoryMetadataKey;

    /// <summary>[EN] Metadata key for the canonical browser bridge global object. [JA] canonical browser bridge global object 用 metadata key です。</summary>
    public const string EnvelopeBridgeGlobalMetadataKey = AIKernel.Wasm.Compute.WebGpuBrowserBridgeAssets.EnvelopeBridgeGlobalMetadataKey;

    /// <summary>[EN] Metadata key for the canonical dispatch envelope schema. [JA] canonical dispatch envelope schema 用 metadata key です。</summary>
    public const string EnvelopeSchemaMetadataKey = AIKernel.Wasm.Compute.WebGpuBrowserBridgeAssets.EnvelopeSchemaMetadataKey;

    /// <summary>[EN] Stable per-pass readiness diagnostics shape exposed by the browser executor. [JA] browser executor が公開する pass 別 readiness diagnostics の安定形状です。</summary>
    public const string PassReadinessShape = AIKernel.Wasm.Compute.WebGpuBrowserBridgeAssets.PassReadinessShape;

    /// <summary>[EN] Returns deterministic metadata keys for provider manifests and capability descriptors. [JA] provider manifest と capability descriptor 用の deterministic metadata key を返します。</summary>
    public static IReadOnlyDictionary<string, string> ToMetadata()
        => AIKernel.Wasm.Compute.WebGpuBrowserBridgeAssets.ToMetadata();
}

/// <summary>
/// [EN] Compatibility wrapper for the corrected AIKernel.Wasm.Compute native WebGPU backend.
/// [JA] 修正済み AIKernel.Wasm.Compute native WebGPU backend の互換 wrapper です。
/// </summary>
public class WebGpuNativeBackend : AIKernel.Wasm.Compute.WebGpuNativeBackend, IWebGpuBackend;

/// <summary>
/// [EN] Compatibility wrapper for the corrected AIKernel.Wasm.Compute browser WebGPU backend.
/// [JA] 修正済み AIKernel.Wasm.Compute browser WebGPU backend の互換 wrapper です。
/// </summary>
public class WebGpuWasmBackend(IWebGpuJsInterop? jsInterop = null)
    : AIKernel.Wasm.Compute.WebGpuWasmBackend(jsInterop), IWebGpuBackend, IWebGpuPassBridgeBackend;

/// <summary>
/// [EN] Compatibility wrapper for the corrected AIKernel.Wasm.Compute WebGPU buffer handle.
/// [JA] 修正済み AIKernel.Wasm.Compute WebGPU buffer handle の互換 wrapper です。
/// </summary>
public class WebGpuWasmBuffer(Guid id, int size) : AIKernel.Wasm.Compute.WebGpuWasmBuffer(id, size);

/// <summary>
/// [EN] Compatibility wrapper for corrected WebGPU compute settings.
/// [JA] 修正済み WebGPU compute settings の互換 wrapper です。
/// </summary>
public record WebGpuComputeSettings : AIKernel.Wasm.Compute.WebGpuComputeSettings;

/// <summary>
/// [EN] Compatibility wrapper for the corrected WebGPU capability descriptor.
/// [JA] 修正済み WebGPU capability descriptor の互換 wrapper です。
/// </summary>
public record WebGpuComputeCapabilityDescriptor(
    string CapabilityId,
    string AdapterProfile,
    IReadOnlyDictionary<string, string> Metadata)
    : AIKernel.Wasm.Compute.WebGpuComputeCapabilityDescriptor(CapabilityId, AdapterProfile, Metadata);

/// <summary>
/// [EN] Compatibility wrapper for the corrected WebGPU compute provider.
/// [JA] 修正済み WebGPU compute Provider の互換 wrapper です。
/// </summary>
public class WebGpuComputeProvider(
    WebGpuComputeSettings settings,
    IWebGpuBackend? backend = null,
    IComputeProvider? cpuFallback = null,
    IEventBus? eventBus = null)
    : AIKernel.Wasm.Compute.WebGpuComputeProvider(settings, backend, cpuFallback, eventBus)
{
    /// <summary>
    /// [EN] Initializes the compatibility provider with default settings.
    /// [JA] default settings で互換 Provider を初期化します。
    /// </summary>
    public WebGpuComputeProvider()
        : this(new WebGpuComputeSettings())
    {
    }
}

/// <summary>
/// [EN] Compatibility wrapper for the corrected WebGPU compute invoker.
/// [JA] 修正済み WebGPU compute invoker の互換 wrapper です。
/// </summary>
public class WebGpuComputeInvoker : AIKernel.Wasm.Compute.WebGpuComputeInvoker;

/// <summary>
/// [EN] Compatibility wrapper for corrected WebGPU sample kernels.
/// [JA] 修正済み WebGPU sample kernel の互換 wrapper です。
/// </summary>
public static class WebGpuSampleKernels
{
    /// <summary>[EN] WGSL vector-add kernel. [JA] WGSL vector-add kernel です。</summary>
    public const string VectorAdd = AIKernel.Wasm.Compute.WebGpuSampleKernels.VectorAdd;
}

/// <summary>
/// [EN] Compatibility wrapper for corrected WebGPU capability contract mapping.
/// [JA] 修正済み WebGPU capability contract mapping の互換 wrapper です。
/// </summary>
public static class WebGpuComputeCapabilityContracts
{
    /// <summary>
    /// [EN] Converts a compatibility descriptor into the shared capability module contract.
    /// [JA] 互換 descriptor を共有 capability module contract へ変換します。
    /// </summary>
    public static CapabilityModuleDescriptor ToContract(WebGpuComputeCapabilityDescriptor descriptor)
        => AIKernel.Wasm.Compute.WebGpuComputeCapabilityContracts.ToContract(descriptor);
}

/// <summary>
/// [EN] Compatibility wrapper for corrected WebGPU compute dependency-injection registration.
/// [JA] 修正済み WebGPU compute dependency-injection registration の互換 wrapper です。
/// </summary>
public static class WebGpuComputeServiceCollectionExtensions
{
    /// <summary>
    /// [EN] Registers the compatibility WebGPU compute provider.
    /// [JA] 互換 WebGPU compute Provider を登録します。
    /// </summary>
    public static IServiceCollection AddWebGpuComputeProvider(
        this IServiceCollection services,
        WebGpuComputeSettings? settings = null,
        IWebGpuBackend? backend = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(_ => settings ?? new WebGpuComputeSettings());
        services.AddSingleton(_ => backend ?? new WebGpuWasmBackend());
        services.AddSingleton<WebGpuComputeProvider>(static provider => new WebGpuComputeProvider(
            provider.GetRequiredService<WebGpuComputeSettings>(),
            provider.GetRequiredService<IWebGpuBackend>(),
            null,
            provider.GetService<IEventBus>()));
        services.AddSingleton<IComputeProvider>(static provider => provider.GetRequiredService<WebGpuComputeProvider>());
        services.AddSingleton<IProvider>(static provider => provider.GetRequiredService<WebGpuComputeProvider>());
        return services;
    }
}

/// <summary>
/// [EN] Compatibility wrapper for corrected WebGPU Python bridge helpers.
/// [JA] 修正済み WebGPU Python bridge helper の互換 wrapper です。
/// </summary>
public static class WebGpuComputePythonBridge
{
    /// <summary>[EN] Creates a deterministic capability descriptor for Python wrappers. [JA] Python wrapper 用の決定論的 capability descriptor を作成します。</summary>
    public static object ToContract(string providerId, string adapterProfile)
        => AIKernel.Wasm.Compute.WebGpuComputePythonBridge.ToContract(providerId, adapterProfile);

    /// <summary>[EN] Creates a provider with default settings. [JA] default settings の Provider を作成します。</summary>
    public static object CreateProvider()
        => new WebGpuComputeProvider();

    /// <summary>[EN] Creates a capability module invoker. [JA] capability module invoker を作成します。</summary>
    public static object CreateInvoker()
        => new WebGpuComputeInvoker();
}
