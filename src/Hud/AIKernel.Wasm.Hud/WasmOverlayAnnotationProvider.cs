namespace AIKernel.Wasm.Hud;

using AIKernel.Abstractions.Perception;
using AIKernel.Dtos.Perception;
using AIKernel.Enums.Perception;

/// <summary>
/// [EN] Generates overlay annotation DTOs without drawing them.
/// [JA] 描画を行わず overlay annotation DTO を生成します。
/// </summary>
public sealed class WasmOverlayAnnotationProvider : IOverlayAnnotationProvider
{
    /// <summary>
    /// [EN] Builds deterministic overlay annotations from request metadata.
    /// [JA] request metadata から deterministic な overlay annotation を構築します。
    /// </summary>
    /// <param name="request">[EN] Overlay annotation request. [JA] overlay annotation request です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視するトークンです。</param>
    /// <returns>[EN] Overlay annotation set. [JA] overlay annotation set を返します。</returns>
    public ValueTask<OverlayAnnotationSet> BuildOverlayAsync(
        OverlayAnnotationRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        var annotations = request.Metadata
            .Where(item => item.Key.StartsWith("overlay.", StringComparison.Ordinal))
            .OrderBy(item => item.Key, StringComparer.Ordinal)
            .Select((item, index) => new OverlayAnnotation
            {
                AnnotationId = item.Key,
                ShapeKind = item.Key.Contains("text", StringComparison.OrdinalIgnoreCase)
                    ? OverlayShapeKind.Text
                    : OverlayShapeKind.Rectangle,
                LayerKind = OverlayLayerKind.Perception,
                X = 0,
                Y = 0,
                Width = 1,
                Height = 1,
                Text = item.Value,
                Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["source"] = "metadata",
                    ["order"] = index.ToString(System.Globalization.CultureInfo.InvariantCulture)
                }
            })
            .ToArray();

        return ValueTask.FromResult(new OverlayAnnotationSet
        {
            AnnotationSetId = string.IsNullOrWhiteSpace(request.ObservationId)
                ? "wasm.overlay.annotations"
                : $"{request.ObservationId}.overlay",
            Succeeded = true,
            Annotations = annotations,
            ObservedAt = DateTimeOffset.UtcNow,
            Metadata = request.Metadata
        });
    }

    /// <summary>
    /// [EN] Describes supported overlay layers.
    /// [JA] 対応する overlay layer を記述します。
    /// </summary>
    /// <param name="request">[EN] Overlay layer catalog request. [JA] overlay layer catalog request です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視するトークンです。</param>
    /// <returns>[EN] Overlay layer catalog. [JA] overlay layer catalog を返します。</returns>
    public ValueTask<OverlayLayerCatalog> DescribeLayersAsync(
        OverlayLayerCatalogRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        return ValueTask.FromResult(new OverlayLayerCatalog
        {
            CatalogId = "wasm.overlay.catalog",
            Succeeded = true,
            SupportedLayers =
            [
                OverlayLayerKind.Debug,
                OverlayLayerKind.Evidence,
                OverlayLayerKind.Perception,
                OverlayLayerKind.Operator
            ],
            Metadata = request.Metadata
        });
    }
}
