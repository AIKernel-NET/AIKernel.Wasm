namespace AIKernel.Wasm.Audio;

using System.Runtime.CompilerServices;
using AIKernel.Wasm.Runtime;

/// <summary>
/// [EN] Technical WASM audio provider for runtime audio buffers and optional WebAudio interop.
/// [JA] runtime audio buffer と任意の WebAudio interop を扱う技術的な WASM audio Provider です。
/// </summary>
public class WasmAudioProvider
{
    private readonly WasmRuntimeContext _context;
    private readonly IWebAudioJsInterop? _jsInterop;
    private readonly IWebAudioGpuBufferInterop? _gpuBufferInterop;
    private readonly WebAudioTimingNormalizer _timingNormalizer;
    private long _frameIndex;

    /// <summary>
    /// [EN] Initializes a WASM audio provider.
    /// [JA] WASM audio Provider を初期化します。
    /// </summary>
    public WasmAudioProvider(
        WasmRuntimeContext? context = null,
        IWebAudioJsInterop? jsInterop = null,
        WebAudioTimingNormalizer? timingNormalizer = null,
        IWebAudioGpuBufferInterop? gpuBufferInterop = null)
    {
        _context = context ?? new WasmRuntimeContext();
        _jsInterop = jsInterop;
        _gpuBufferInterop = gpuBufferInterop;
        _timingNormalizer = timingNormalizer ?? new WebAudioTimingNormalizer();
    }

    /// <summary>[EN] Plays PCM bytes through the runtime buffer and optional WebAudio interop. [JA] runtime buffer と任意の WebAudio interop 経由で PCM byte を再生します。</summary>
    public async ValueTask<WebAudioPlayResult> PlayAsync(WebAudioPcmBuffer buffer, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(buffer);
        _context.SetAudioBuffer(buffer.Payload.ToArray());

        if (_jsInterop is not null)
        {
            return await _jsInterop.PlayPcmAsync(buffer, cancellationToken).ConfigureAwait(false);
        }

        return new WebAudioPlayResult
        {
            Succeeded = true,
            Timing = _timingNormalizer.Normalize(Interlocked.Increment(ref _frameIndex), 0, _context.Clock)
        };
    }

    /// <summary>[EN] Stops playback through optional WebAudio interop. [JA] 任意の WebAudio interop 経由で playback を停止します。</summary>
    public async ValueTask<WebAudioStopResult> StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return _jsInterop is null
            ? new WebAudioStopResult { Succeeded = true }
            : await _jsInterop.StopAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>[EN] Captures PCM frames from WebAudio or the current runtime audio buffer. [JA] WebAudio または現在の runtime audio buffer から PCM frame を capture します。</summary>
    public async IAsyncEnumerable<WebAudioCaptureFrame> CaptureAsync(
        WebAudioCaptureRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (_jsInterop is not null)
        {
            await foreach (var frame in _jsInterop.CapturePcmAsync(request, cancellationToken).ConfigureAwait(false))
            {
                yield return frame;
            }

            yield break;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var maxFrames = Math.Max(1, request.MaxFrames ?? 1);
        for (var index = 0; index < maxFrames; index++)
        {
            yield return new WebAudioCaptureFrame
            {
                Buffer = new WebAudioPcmBuffer
                {
                    Payload = _context.GetAudioBuffer()
                },
                Timing = _timingNormalizer.Normalize(Interlocked.Increment(ref _frameIndex), 0, _context.Clock)
            };
            await Task.Yield();
        }
    }

    /// <summary>
    /// [EN] Uploads PCM bytes into an optional GPU-resident audio buffer path.
    /// [JA] PCM byte を任意の GPU-resident audio buffer path へ upload します。
    /// </summary>
    /// <param name="request">[EN] GPU upload request. [JA] GPU upload request です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視するトークンです。</param>
    /// <returns>[EN] GPU upload result. [JA] GPU upload result を返します。</returns>
    public async ValueTask<WebAudioGpuUploadResult> UploadGpuBufferAsync(
        WebAudioGpuUploadRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Buffer);

        _context.SetAudioBuffer(request.Buffer.Payload is byte[] bytes ? bytes : request.Buffer.Payload.ToArray());
        if (_gpuBufferInterop is not null)
        {
            return await _gpuBufferInterop.UploadPcmToGpuAsync(request, cancellationToken).ConfigureAwait(false);
        }

        return new WebAudioGpuUploadResult
        {
            Succeeded = false,
            ErrorCode = WasmAudioDiagnostics.WebAudioGpuInteropUnavailable,
            ErrorMessage = "WebAudio GPU buffer interop is unavailable.",
            Descriptor = CreateGpuBufferDescriptor(request, zeroCopy: false),
            Metadata = new Dictionary<string, string>(request.Metadata, StringComparer.Ordinal)
            {
                ["backend"] = "runtime-buffer",
                ["zeroCopyHint"] = "false"
            }
        };
    }

    private static WebAudioGpuBufferDescriptor CreateGpuBufferDescriptor(
        WebAudioGpuUploadRequest request,
        bool zeroCopy)
    {
        var sampleWidth = request.Buffer.SampleFormat.Equals("f32", StringComparison.OrdinalIgnoreCase) ? 4 : 1;
        var frameCount = request.Buffer.Channels <= 0
            ? 0
            : Math.Max(0, request.Buffer.Payload.Count / Math.Max(1, sampleWidth * request.Buffer.Channels));
        return new WebAudioGpuBufferDescriptor
        {
            BufferId = string.IsNullOrWhiteSpace(request.BufferId) ? "web-audio.gpu-buffer" : request.BufferId,
            ByteLength = request.Buffer.Payload.Count,
            SampleRate = request.Buffer.SampleRate,
            Channels = request.Buffer.Channels,
            SampleFormat = request.Buffer.SampleFormat,
            DType = request.Buffer.SampleFormat.Equals("f32", StringComparison.OrdinalIgnoreCase) ? "f32" : "u8",
            Shape = $"{frameCount.ToString(System.Globalization.CultureInfo.InvariantCulture)},{request.Buffer.Channels.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
            Stride = request.Buffer.Channels.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Layout = "interleaved",
            ZeroCopyHint = zeroCopy,
            Metadata = new Dictionary<string, string>(request.Metadata, StringComparer.Ordinal)
            {
                ["preferZeroCopy"] = request.PreferZeroCopy ? "true" : "false"
            }
        };
    }
}

/// <summary>[EN] Technical WASM audio playback provider. [JA] 技術的な WASM audio playback Provider です。</summary>
public sealed class WasmAudioPlayProvider(WasmRuntimeContext? context = null, IWebAudioJsInterop? jsInterop = null)
    : WasmAudioProvider(context, jsInterop);

/// <summary>[EN] Technical WASM audio recording provider. [JA] 技術的な WASM audio recording Provider です。</summary>
public sealed class WasmAudioRecProvider(WasmRuntimeContext? context = null, IWebAudioJsInterop? jsInterop = null)
    : WasmAudioProvider(context, jsInterop);

/// <summary>[EN] Technical browser WebAudio provider boundary. [JA] 技術的な browser WebAudio Provider 境界です。</summary>
public sealed class WebAudioProvider(WasmRuntimeContext? context = null, IWebAudioJsInterop? jsInterop = null)
    : WasmAudioProvider(context, jsInterop);

/// <summary>[EN] Defines stable WASM audio diagnostic codes. [JA] stable な WASM audio diagnostic code を定義します。</summary>
public static class WasmAudioDiagnostics
{
    /// <summary>[EN] Payload is missing. [JA] payload が不足しています。</summary>
    public const string PayloadMissing = "WASM_AUDIO_PAYLOAD_MISSING";

    /// <summary>[EN] WebAudio interop is not available. [JA] WebAudio interop が利用できません。</summary>
    public const string WebAudioInteropUnavailable = "WASM_WEBAUDIO_INTEROP_UNAVAILABLE";

    /// <summary>[EN] WebAudio GPU buffer interop is not available. [JA] WebAudio GPU buffer interop が利用できません。</summary>
    public const string WebAudioGpuInteropUnavailable = "WASM_WEBAUDIO_GPU_INTEROP_UNAVAILABLE";
}
