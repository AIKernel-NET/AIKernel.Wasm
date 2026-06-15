namespace AIKernel.Wasm.Spatial;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// [EN] Registers WASM spatial cognition services.
/// [JA] WASM spatial cognition service を登録します。
/// </summary>
public static class WasmSpatialServiceCollectionExtensions
{
    /// <summary>
    /// [EN] Adds scenario-independent WASM spatial cognition services.
    /// [JA] scenario 非依存の WASM spatial cognition service を追加します。
    /// </summary>
    /// <param name="services">[EN] Service collection to update. [JA] 更新対象の service collection です。</param>
    /// <returns>[EN] The same service collection. [JA] 同じ service collection を返します。</returns>
    public static IServiceCollection AddAIKernelWasmSpatialCognition(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<WasmSpatialCognitionProvider>();
        services.TryAddSingleton<IWasmSpatialCognitionProvider>(provider => provider.GetRequiredService<WasmSpatialCognitionProvider>());

        return services;
    }
}
