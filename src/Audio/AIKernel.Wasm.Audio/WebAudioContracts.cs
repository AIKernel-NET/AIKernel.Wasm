namespace AIKernel.Wasm.Audio;

/// <summary>
/// [EN] Opaque browser WebAudio context handle kept inside the WASM audio boundary.
/// [JA] WASM audio 境界内に閉じる opaque な browser WebAudio context handle です。
/// </summary>
public sealed record WebAudioContextHandle
{
    /// <summary>[EN] Gets the context identifier. [JA] context identifier を取得します。</summary>
    public string ContextId { get; init; } = "wasm.webaudio.context";

    /// <summary>[EN] Gets optional metadata. [JA] 任意の metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// [EN] Carries PCM bytes across the WASM WebAudio boundary without backend-specific types.
/// [JA] backend-specific 型を使わず WASM WebAudio 境界を越えて PCM byte を運びます。
/// </summary>
public sealed record WebAudioPcmBuffer
{
    /// <summary>[EN] Gets PCM payload bytes. [JA] PCM payload byte を取得します。</summary>
    public IReadOnlyList<byte> Payload { get; init; } = [];

    /// <summary>[EN] Gets sample rate in hertz. [JA] hertz 単位の sample rate を取得します。</summary>
    public int SampleRate { get; init; } = 48000;

    /// <summary>[EN] Gets channel count. [JA] channel count を取得します。</summary>
    public int Channels { get; init; } = 2;

    /// <summary>[EN] Gets the sample format identifier. [JA] sample format identifier を取得します。</summary>
    public string SampleFormat { get; init; } = "f32";

    /// <summary>[EN] Gets optional metadata. [JA] 任意の metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// [EN] Describes WebAudio timing normalized at the WASM boundary.
/// [JA] WASM 境界で正規化された WebAudio timing を記述します。
/// </summary>
public sealed record AudioTimingInfo
{
    /// <summary>[EN] Gets frame index. [JA] frame index を取得します。</summary>
    public long FrameIndex { get; init; }

    /// <summary>[EN] Gets sample offset. [JA] sample offset を取得します。</summary>
    public long SampleOffset { get; init; }

    /// <summary>[EN] Gets timestamp. [JA] timestamp を取得します。</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>[EN] Gets optional metadata. [JA] 任意の metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>[EN] Carries WebAudio playback result data. [JA] WebAudio playback result data を運びます。</summary>
public sealed record WebAudioPlayResult
{
    /// <summary>[EN] Gets whether playback succeeded. [JA] playback が成功したかを取得します。</summary>
    public bool Succeeded { get; init; }

    /// <summary>[EN] Gets failure code. [JA] failure code を取得します。</summary>
    public string? ErrorCode { get; init; }

    /// <summary>[EN] Gets failure message. [JA] failure message を取得します。</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>[EN] Gets timing information. [JA] timing information を取得します。</summary>
    public AudioTimingInfo Timing { get; init; } = new();
}

/// <summary>[EN] Carries WebAudio stop result data. [JA] WebAudio stop result data を運びます。</summary>
public sealed record WebAudioStopResult
{
    /// <summary>[EN] Gets whether stop succeeded. [JA] stop が成功したかを取得します。</summary>
    public bool Succeeded { get; init; }

    /// <summary>[EN] Gets failure code. [JA] failure code を取得します。</summary>
    public string? ErrorCode { get; init; }

    /// <summary>[EN] Gets failure message. [JA] failure message を取得します。</summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>[EN] Carries a WebAudio capture request. [JA] WebAudio capture request を運びます。</summary>
public sealed record WebAudioCaptureRequest
{
    /// <summary>[EN] Gets maximum frame count. [JA] maximum frame count を取得します。</summary>
    public int? MaxFrames { get; init; }

    /// <summary>[EN] Gets optional metadata. [JA] 任意の metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>[EN] Carries one captured WebAudio PCM frame. [JA] capture された WebAudio PCM frame を運びます。</summary>
public sealed record WebAudioCaptureFrame
{
    /// <summary>[EN] Gets PCM payload. [JA] PCM payload を取得します。</summary>
    public WebAudioPcmBuffer Buffer { get; init; } = new();

    /// <summary>[EN] Gets timing information. [JA] timing information を取得します。</summary>
    public AudioTimingInfo Timing { get; init; } = new();
}

/// <summary>
/// [EN] Describes an audio PCM buffer that may be resident behind the WebGPU boundary.
/// [JA] WebGPU 境界の背後に resident として保持できる audio PCM buffer を記述します。
/// </summary>
public sealed record WebAudioGpuBufferDescriptor
{
    /// <summary>[EN] Gets the stable buffer identifier. [JA] 安定した buffer 識別子を取得します。</summary>
    public string BufferId { get; init; } = string.Empty;

    /// <summary>[EN] Gets the buffer byte length. [JA] buffer byte length を取得します。</summary>
    public int ByteLength { get; init; }

    /// <summary>[EN] Gets sample rate in hertz. [JA] hertz 単位の sample rate を取得します。</summary>
    public int SampleRate { get; init; } = 48000;

    /// <summary>[EN] Gets channel count. [JA] channel count を取得します。</summary>
    public int Channels { get; init; } = 2;

    /// <summary>[EN] Gets the normalized sample format identifier. [JA] 正規化された sample format identifier を取得します。</summary>
    public string SampleFormat { get; init; } = "f32";

    /// <summary>[EN] Gets the typed element identifier for tensor-like metadata. [JA] tensor-like metadata 用の typed element identifier を取得します。</summary>
    public string DType { get; init; } = "f32";

    /// <summary>[EN] Gets the normalized shape string. [JA] 正規化された shape 文字列を取得します。</summary>
    public string Shape { get; init; } = string.Empty;

    /// <summary>[EN] Gets the optional stride string. [JA] 任意の stride 文字列を取得します。</summary>
    public string? Stride { get; init; }

    /// <summary>[EN] Gets the optional memory layout name. [JA] 任意の memory layout 名を取得します。</summary>
    public string? Layout { get; init; }

    /// <summary>[EN] Gets whether the backend can keep this buffer on a GPU resident path. [JA] backend が GPU resident path に buffer を保持できるかどうかを取得します。</summary>
    public bool ZeroCopyHint { get; init; }

    /// <summary>[EN] Gets descriptor metadata. [JA] descriptor metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// [EN] Carries a request to upload WebAudio PCM bytes into a GPU-resident buffer.
/// [JA] WebAudio PCM byte を GPU-resident buffer へ upload する request を保持します。
/// </summary>
public sealed record WebAudioGpuUploadRequest
{
    /// <summary>[EN] Gets the PCM buffer to upload. [JA] upload 対象の PCM buffer を取得します。</summary>
    public WebAudioPcmBuffer Buffer { get; init; } = new();

    /// <summary>[EN] Gets the requested buffer identifier. [JA] 要求された buffer 識別子を取得します。</summary>
    public string BufferId { get; init; } = string.Empty;

    /// <summary>[EN] Gets whether the caller prefers a GPU resident path. [JA] caller が GPU resident path を優先するかどうかを取得します。</summary>
    public bool PreferZeroCopy { get; init; } = true;

    /// <summary>[EN] Gets upload metadata. [JA] upload metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// [EN] Carries the result of a WebAudio GPU buffer upload.
/// [JA] WebAudio GPU buffer upload の結果を保持します。
/// </summary>
public sealed record WebAudioGpuUploadResult
{
    /// <summary>[EN] Gets whether the upload succeeded. [JA] upload が成功したかを取得します。</summary>
    public bool Succeeded { get; init; }

    /// <summary>[EN] Gets the uploaded buffer descriptor. [JA] upload された buffer descriptor を取得します。</summary>
    public WebAudioGpuBufferDescriptor Descriptor { get; init; } = new();

    /// <summary>[EN] Gets stable failure code when upload failed. [JA] upload が失敗した場合の stable failure code を取得します。</summary>
    public string? ErrorCode { get; init; }

    /// <summary>[EN] Gets human-readable failure message when upload failed. [JA] upload が失敗した場合の人間可読 message を取得します。</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>[EN] Gets upload metadata. [JA] upload metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// [EN] JavaScript interop boundary for WebAudio, contained inside AIKernel.Wasm.Audio.
/// [JA] AIKernel.Wasm.Audio 内に閉じる WebAudio 用 JavaScript interop 境界です。
/// </summary>
public interface IWebAudioJsInterop
{
    /// <summary>[EN] Ensures a browser WebAudio context. [JA] browser WebAudio context を確保します。</summary>
    ValueTask<WebAudioContextHandle> EnsureContextAsync(CancellationToken cancellationToken);

    /// <summary>[EN] Plays PCM data through WebAudio. [JA] WebAudio 経由で PCM data を再生します。</summary>
    ValueTask<WebAudioPlayResult> PlayPcmAsync(WebAudioPcmBuffer buffer, CancellationToken cancellationToken);

    /// <summary>[EN] Stops WebAudio playback. [JA] WebAudio playback を停止します。</summary>
    ValueTask<WebAudioStopResult> StopAsync(CancellationToken cancellationToken);

    /// <summary>[EN] Captures PCM frames from WebAudio. [JA] WebAudio から PCM frame を capture します。</summary>
    IAsyncEnumerable<WebAudioCaptureFrame> CapturePcmAsync(WebAudioCaptureRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// [EN] Optional JavaScript interop boundary for GPU-resident WebAudio buffers.
/// [JA] GPU-resident WebAudio buffer 用の任意 JavaScript interop 境界です。
/// </summary>
public interface IWebAudioGpuBufferInterop
{
    /// <summary>[EN] Uploads PCM bytes to a GPU-resident audio buffer. [JA] PCM byte を GPU-resident audio buffer へ upload します。</summary>
    ValueTask<WebAudioGpuUploadResult> UploadPcmToGpuAsync(WebAudioGpuUploadRequest request, CancellationToken cancellationToken);
}
