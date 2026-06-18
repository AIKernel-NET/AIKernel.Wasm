using AIKernel.Abstractions.Compute;
using AIKernel.Abstractions.Events;
using AIKernel.Abstractions.Providers;
using AIKernel.Providers.Standard.Compute;
using Microsoft.Extensions.DependencyInjection;

namespace AIKernel.Wasm.Compute;

/// <summary>
/// [EN] Dependency-injection extensions for WebGpuComputeProvider registration.
/// [JA] WebGpuComputeProvider 登録用の dependency-injection extension です。
/// </summary>
public static class WebGpuComputeServiceCollectionExtensions
{
    /// <summary>
    /// [EN] Registers WebGpuComputeProvider and its compute contract in a service collection.
    /// [JA] WebGpuComputeProvider と compute contract を service collection に登録します。
    /// </summary>
    public static IServiceCollection AddWebGpuComputeProvider(
        this IServiceCollection services,
        WebGpuComputeSettings? settings = null,
        IWebGpuBackend? backend = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton(_ => settings ?? new WebGpuComputeSettings());
        services.AddSingleton(_ => backend ?? new WebGpuWasmBackend());
        services.AddSingleton<CpuComputeProvider>();
        services.AddSingleton<WebGpuComputeProvider>(static provider => new WebGpuComputeProvider(
            provider.GetRequiredService<WebGpuComputeSettings>(),
            provider.GetRequiredService<IWebGpuBackend>(),
            provider.GetRequiredService<CpuComputeProvider>(),
            provider.GetService<IEventBus>()));
        services.AddSingleton<IComputeProvider>(static provider => provider.GetRequiredService<WebGpuComputeProvider>());
        services.AddSingleton<IProvider>(static provider => provider.GetRequiredService<WebGpuComputeProvider>());
        return services;
    }
}
