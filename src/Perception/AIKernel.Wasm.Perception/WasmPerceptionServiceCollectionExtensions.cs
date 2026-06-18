namespace AIKernel.Wasm.Perception;

using AIKernel.Abstractions.Perception;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// [EN] Registers WASM perception services.
/// [JA] WASM perception service を登録します。
/// </summary>
public static class WasmPerceptionServiceCollectionExtensions
{
    /// <summary>
    /// [EN] Adds WASM frame, auditory, and observation perception services.
    /// [JA] WASM frame / auditory / observation perception service を追加します。
    /// </summary>
    /// <param name="services">[EN] Service collection to update. [JA] 更新対象の service collection です。</param>
    /// <returns>[EN] The same service collection. [JA] 同じ service collection を返します。</returns>
    public static IServiceCollection AddAIKernelWasmPerception(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<WasmFramePerceptionProvider>();
        services.TryAddSingleton<IFramePerceptionProvider>(provider => provider.GetRequiredService<WasmFramePerceptionProvider>());
        services.TryAddSingleton<WasmAuditoryPerceptionProvider>();
        services.TryAddSingleton<IWasmAuditoryPerceptionProvider>(provider => provider.GetRequiredService<WasmAuditoryPerceptionProvider>());
        services.TryAddSingleton<WasmResidentPerceptionAlgorithmLibrary>();
        services.TryAddSingleton<IWasmResidentPerceptionAlgorithmLibrary>(
            provider => provider.GetRequiredService<WasmResidentPerceptionAlgorithmLibrary>());
        services.TryAddSingleton<WasmObservationProvider>();
        services.TryAddSingleton<IObservationProvider>(provider => provider.GetRequiredService<WasmObservationProvider>());

        return services;
    }
}
