namespace AIKernel.Wasm.Perception;

using AIKernel.Wasm.Audio;

/// <summary>
/// [EN] Performs WASM-owned auditory perception over WebAudio PCM frames.
/// [JA] WebAudio PCM frame に対する WASM 所有の auditory perception を実行します。
/// </summary>
public interface IWasmAuditoryPerceptionProvider
{
    /// <summary>
    /// [EN] Analyzes one WebAudio PCM capture frame.
    /// [JA] 1 つの WebAudio PCM capture frame を解析します。
    /// </summary>
    /// <param name="request">[EN] Auditory perception request. [JA] auditory perception request です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視するトークンです。</param>
    /// <returns>[EN] Auditory perception result. [JA] auditory perception result を返します。</returns>
    ValueTask<WasmAuditoryPerceptionResult> AnalyzeAsync(
        WasmAuditoryPerceptionRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// [EN] Carries a WASM auditory perception request.
/// [JA] WASM auditory perception request を保持します。
/// </summary>
public sealed record WasmAuditoryPerceptionRequest
{
    /// <summary>[EN] Gets the observation identifier. [JA] observation 識別子を取得します。</summary>
    public string ObservationId { get; init; } = string.Empty;

    /// <summary>[EN] Gets the audio frame to analyze. [JA] 解析対象の audio frame を取得します。</summary>
    public WebAudioCaptureFrame Frame { get; init; } = new();

    /// <summary>[EN] Gets request metadata. [JA] request metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// [EN] Carries provider-neutral auditory signals extracted inside the WASM boundary.
/// [JA] WASM 境界内で抽出された provider-neutral な auditory signal を保持します。
/// </summary>
public sealed record WasmAuditoryPerceptionResult
{
    /// <summary>[EN] Gets whether extraction succeeded. [JA] extraction が成功したかを取得します。</summary>
    public bool Succeeded { get; init; }

    /// <summary>[EN] Gets the observation identifier. [JA] observation 識別子を取得します。</summary>
    public string ObservationId { get; init; } = string.Empty;

    /// <summary>[EN] Gets extracted auditory signals. [JA] 抽出された auditory signal を取得します。</summary>
    public IReadOnlyList<WasmAuditorySignal> Signals { get; init; } = [];

    /// <summary>[EN] Gets stable failure code when extraction failed. [JA] extraction が失敗した場合の stable failure code を取得します。</summary>
    public string? ErrorCode { get; init; }

    /// <summary>[EN] Gets human-readable failure message when extraction failed. [JA] extraction が失敗した場合の人間可読 message を取得します。</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>[EN] Gets extraction diagnostics. [JA] extraction diagnostics を取得します。</summary>
    public IReadOnlyList<string> Diagnostics { get; init; } = [];

    /// <summary>[EN] Gets extraction metadata. [JA] extraction metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// [EN] Describes one WASM auditory perception signal.
/// [JA] 1 つの WASM auditory perception signal を記述します。
/// </summary>
public sealed record WasmAuditorySignal
{
    /// <summary>[EN] Gets the signal identifier. [JA] signal 識別子を取得します。</summary>
    public string SignalId { get; init; } = string.Empty;

    /// <summary>[EN] Gets the signal kind. [JA] signal kind を取得します。</summary>
    public string Kind { get; init; } = string.Empty;

    /// <summary>[EN] Gets the normalized signal value. [JA] normalized signal value を取得します。</summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>[EN] Gets optional signal confidence outside CTG GateInput. [JA] CTG GateInput の外側に保持する任意の signal confidence を取得します。</summary>
    public double? Confidence { get; init; }

    /// <summary>[EN] Gets signal metadata. [JA] signal metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}
