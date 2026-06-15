namespace AIKernel.Wasm.Models;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// [EN] Registers WebGPU resident model services for WASM hosts.
/// [JA] WASM host 向け WebGPU resident model service を登録します。
/// </summary>
public static class WebGpuResidentModelServiceCollectionExtensions
{
    /// <summary>
    /// [EN] Adds descriptor-driven WebGPU resident model execution services.
    /// [JA] descriptor-driven WebGPU resident model execution service を追加します。
    /// </summary>
    /// <param name="services">[EN] Service collection to update. [JA] 更新対象の service collection です。</param>
    /// <returns>[EN] The same service collection. [JA] 同じ service collection を返します。</returns>
    public static IServiceCollection AddAIKernelWasmModels(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<WebGpuResidentModelProvider>();

        return services;
    }
}
