namespace AIKernel.Wasm.Hud;

using AIKernel.Abstractions.Perception;
using AIKernel.Dtos.Perception;
using AIKernel.Enums.Perception;

/// <summary>
/// [EN] Generates lightweight HUD signal DTOs without rendering or scenario-specific interpretation.
/// [JA] rendering や scenario 固有解釈を行わず lightweight HUD signal DTO を生成します。
/// </summary>
public sealed class WasmHudSignalProvider : IHudSignalProvider
{
    /// <summary>
    /// [EN] Extracts HUD signals from deterministic request metadata.
    /// [JA] deterministic request metadata から HUD signal を抽出します。
    /// </summary>
    /// <param name="request">[EN] HUD signal request. [JA] HUD signal request です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視するトークンです。</param>
    /// <returns>[EN] HUD signal set. [JA] HUD signal set を返します。</returns>
    public ValueTask<HudSignalSet> ExtractAsync(
        HudSignalRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        var signals = request.Metadata
            .Where(item => item.Key.StartsWith("hud.", StringComparison.Ordinal))
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select((item, index) => new HudSignal
            {
                SignalId = item.Key,
                Kind = InferKind(item.Key),
                Value = item.Value,
                NumericValue = double.TryParse(item.Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number)
                    ? number
                    : null,
                Confidence = SignalConfidenceKind.Medium,
                Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["source"] = "metadata",
                    ["order"] = index.ToString(System.Globalization.CultureInfo.InvariantCulture)
                }
            })
            .ToArray();

        return ValueTask.FromResult(new HudSignalSet
        {
            SignalSetId = string.IsNullOrWhiteSpace(request.ObservationId)
                ? "wasm.hud.signals"
                : $"{request.ObservationId}.hud",
            Succeeded = true,
            Signals = signals,
            ObservedAt = DateTimeOffset.UtcNow,
            Metadata = request.Metadata
        });
    }

    /// <summary>
    /// [EN] Describes supported generic HUD signal kinds.
    /// [JA] 対応する generic HUD signal kind を記述します。
    /// </summary>
    /// <param name="request">[EN] HUD signal catalog request. [JA] HUD signal catalog request です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視するトークンです。</param>
    /// <returns>[EN] HUD signal catalog. [JA] HUD signal catalog を返します。</returns>
    public ValueTask<HudSignalCatalog> DescribeSignalsAsync(
        HudSignalCatalogRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        return ValueTask.FromResult(new HudSignalCatalog
        {
            CatalogId = "wasm.hud.catalog",
            Succeeded = true,
            SupportedSignals =
            [
                HudSignalKind.Health,
                HudSignalKind.Resource,
                HudSignalKind.Timer,
                HudSignalKind.Status,
                HudSignalKind.Warning
            ],
            Metadata = request.Metadata
        });
    }

    private static HudSignalKind InferKind(string key)
    {
        if (key.Contains("health", StringComparison.OrdinalIgnoreCase))
        {
            return HudSignalKind.Health;
        }

        if (key.Contains("ammo", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("resource", StringComparison.OrdinalIgnoreCase))
        {
            return HudSignalKind.Resource;
        }

        if (key.Contains("timer", StringComparison.OrdinalIgnoreCase) ||
            key.Contains("time", StringComparison.OrdinalIgnoreCase))
        {
            return HudSignalKind.Timer;
        }

        if (key.Contains("warning", StringComparison.OrdinalIgnoreCase))
        {
            return HudSignalKind.Warning;
        }

        return HudSignalKind.Status;
    }
}
