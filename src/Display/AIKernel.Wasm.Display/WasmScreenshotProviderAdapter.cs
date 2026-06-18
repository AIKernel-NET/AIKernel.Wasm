namespace AIKernel.Wasm.Display;

using AIKernel.Dtos.Frame;
using AIKernel.Wasm.Runtime;

/// <summary>
/// [EN] Adapter from the existing runtime screenshot provider to frame snapshots.
/// [JA] 既存 runtime screenshot Provider から frame snapshot へ変換する adapter です。
/// </summary>
public sealed class WasmScreenshotProviderAdapter
{
    private readonly WasmScreenshotProvider _screenshotProvider;
    private readonly WasmDisplaySurfaceDescriptor _descriptor;
    private long _frameIndex;

    /// <summary>
    /// [EN] Initializes the screenshot adapter.
    /// [JA] screenshot adapter を初期化します。
    /// </summary>
    public WasmScreenshotProviderAdapter(
        WasmScreenshotProvider screenshotProvider,
        WasmDisplaySurfaceDescriptor? descriptor = null)
    {
        _screenshotProvider = screenshotProvider ?? throw new ArgumentNullException(nameof(screenshotProvider));
        _descriptor = descriptor ?? new WasmDisplaySurfaceDescriptor();
    }

    /// <summary>
    /// [EN] Captures one frame snapshot from the wrapped screenshot provider.
    /// [JA] wrap された screenshot Provider から 1 つの frame snapshot を取得します。
    /// </summary>
    public async ValueTask<FrameSnapshot> CaptureSnapshotAsync(CancellationToken cancellationToken)
    {
        var frame = await _screenshotProvider.CaptureAsync(cancellationToken).ConfigureAwait(false);
        var frameIndex = Interlocked.Increment(ref _frameIndex);
        return new FrameSnapshot
        {
            FrameId = $"{_descriptor.SurfaceId}:{frameIndex}",
            SourceId = _descriptor.SurfaceId,
            FrameIndex = frameIndex,
            Buffer = _descriptor.ToFrameBufferDescriptor(),
            FrameHash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(frame)),
            Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["surfaceId"] = _descriptor.SurfaceId,
                ["byteLength"] = frame.Length.ToString(System.Globalization.CultureInfo.InvariantCulture)
            }
        };
    }
}
