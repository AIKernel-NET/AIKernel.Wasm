namespace AIKernel.Wasm.Hud;

using AIKernel.Abstractions.Perception;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// [EN] Registers WASM HUD DTO generation services.
/// [JA] WASM HUD DTO generation service を登録します。
/// </summary>
public static class WasmHudServiceCollectionExtensions
{
    /// <summary>
    /// [EN] Adds HUD signal and overlay annotation providers.
    /// [JA] HUD signal / overlay annotation Provider を追加します。
    /// </summary>
    /// <param name="services">[EN] Service collection to update. [JA] 更新対象の service collection です。</param>
    /// <returns>[EN] The same service collection. [JA] 同じ service collection を返します。</returns>
    public static IServiceCollection AddAIKernelWasmHud(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<WasmHudSignalProvider>();
        services.TryAddSingleton<IHudSignalProvider>(provider => provider.GetRequiredService<WasmHudSignalProvider>());
        services.TryAddSingleton<WasmOverlayAnnotationProvider>();
        services.TryAddSingleton<IOverlayAnnotationProvider>(provider => provider.GetRequiredService<WasmOverlayAnnotationProvider>());

        return services;
    }
}
