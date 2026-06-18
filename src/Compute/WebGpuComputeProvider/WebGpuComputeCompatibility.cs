namespace AIKernel.Wasm.Comput;

using AIKernel.Abstractions.Compute;
using AIKernel.Abstractions.Events;
using AIKernel.Abstractions.Providers;
using AIKernel.Dtos.Capabilities;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// [EN] Compatibility alias for the corrected AIKernel.Wasm.Compute WebGPU backend interface.
/// [JA] 修正済み AIKernel.Wasm.Compute WebGPU backend interface の互換 alias です。
/// </summary>
public interface IWebGpuBackend : AIKernel.Wasm.Compute.IWebGpuBackend;

/// <summary>
/// [EN] Compatibility alias for the corrected AIKernel.Wasm.Compute WebGPU JS interop interface.
/// [JA] 修正済み AIKernel.Wasm.Compute WebGPU JS interop interface の互換 alias です。
/// </summary>
public interface IWebGpuJsInterop : AIKernel.Wasm.Compute.IWebGpuJsInterop;

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
    : AIKernel.Wasm.Compute.WebGpuWasmBackend(jsInterop), IWebGpuBackend;

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
