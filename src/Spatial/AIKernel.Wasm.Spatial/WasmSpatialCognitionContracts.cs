namespace AIKernel.Wasm.Spatial;

using AIKernel.Dtos.Perception;
using AIKernel.Wasm.Perception;

/// <summary>
/// [EN] Builds WASM-owned spatial cognition snapshots from perception outputs.
/// [JA] perception output から WASM 所有の spatial cognition snapshot を構築します。
/// </summary>
public interface IWasmSpatialCognitionProvider
{
    /// <summary>
    /// [EN] Builds a spatial cognition snapshot.
    /// [JA] spatial cognition snapshot を構築します。
    /// </summary>
    /// <param name="request">[EN] Spatial cognition request. [JA] spatial cognition request です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視するトークンです。</param>
    /// <returns>[EN] Spatial cognition snapshot. [JA] spatial cognition snapshot を返します。</returns>
    ValueTask<WasmSpatialCognitionSnapshot> BuildSnapshotAsync(
        WasmSpatialCognitionRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// [EN] Carries visual and auditory perception outputs for spatial composition.
/// [JA] spatial composition 用の visual / auditory perception output を保持します。
/// </summary>
public sealed record WasmSpatialCognitionRequest
{
    /// <summary>[EN] Gets the request identifier. [JA] request 識別子を取得します。</summary>
    public string RequestId { get; init; } = string.Empty;

    /// <summary>[EN] Gets visual frame perception results. [JA] visual frame perception result を取得します。</summary>
    public IReadOnlyList<FramePerceptionResult> VisualPerceptions { get; init; } = [];

    /// <summary>[EN] Gets auditory perception results. [JA] auditory perception result を取得します。</summary>
    public IReadOnlyList<WasmAuditoryPerceptionResult> AuditoryPerceptions { get; init; } = [];

    /// <summary>[EN] Gets low-level projection input used by the WASM spatial kernel. [JA] WASM spatial kernel が使用する low-level projection input を取得します。</summary>
    public WasmSpatialProjectionInput ProjectionInput { get; init; } = new();

    /// <summary>[EN] Gets named sensor inputs for extensible spatial fusion. [JA] 拡張可能な spatial fusion 用の名前付き sensor input を取得します。</summary>
    public IReadOnlyDictionary<string, WasmSensorStateDescriptor> SensorInputs { get; init; } =
        new Dictionary<string, WasmSensorStateDescriptor>(StringComparer.Ordinal);

    /// <summary>[EN] Gets request metadata. [JA] request metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// [EN] Carries a two-dimensional point for scenario-independent spatial projection.
/// [JA] scenario 非依存 spatial projection 用の 2 次元 point を保持します。
/// </summary>
public readonly record struct WasmSpatialPoint
{
    /// <summary>[EN] Gets the horizontal component. [JA] 水平方向 component を取得します。</summary>
    public double X { get; init; }

    /// <summary>[EN] Gets the vertical component. [JA] 垂直方向 component を取得します。</summary>
    public double Y { get; init; }

    /// <summary>
    /// [EN] Initializes a spatial point.
    /// [JA] spatial point を初期化します。
    /// </summary>
    /// <param name="x">[EN] Horizontal component. [JA] 水平方向 component です。</param>
    /// <param name="y">[EN] Vertical component. [JA] 垂直方向 component です。</param>
    public WasmSpatialPoint(double x, double y)
    {
        X = x;
        Y = y;
    }
}

/// <summary>
/// [EN] Carries hot-path input for visual and auditory spatial projection.
/// [JA] visual / auditory spatial projection の hot-path input を保持します。
/// </summary>
public readonly record struct WasmSpatialProjectionInput
{
    /// <summary>[EN] Gets the observer position. [JA] observer position を取得します。</summary>
    public WasmSpatialPoint Player { get; init; }

    /// <summary>[EN] Gets the observed visual centroid. [JA] observed visual centroid を取得します。</summary>
    public WasmSpatialPoint Centroid { get; init; }

    /// <summary>[EN] Gets left-channel energy. [JA] left-channel energy を取得します。</summary>
    public double LeftEnergy { get; init; }

    /// <summary>[EN] Gets right-channel energy. [JA] right-channel energy を取得します。</summary>
    public double RightEnergy { get; init; }

    /// <summary>[EN] Gets auditory correction gain in radians. [JA] auditory correction gain を radian 単位で取得します。</summary>
    public double CorrectionGain { get; init; }

    /// <summary>[EN] Gets HUD center point for projection. [JA] projection 用 HUD center point を取得します。</summary>
    public WasmSpatialPoint HudCenter { get; init; }

    /// <summary>[EN] Gets HUD projection radius. [JA] HUD projection radius を取得します。</summary>
    public double HudRadius { get; init; }
}

/// <summary>
/// [EN] Carries calculated spatial projection values.
/// [JA] 計算済み spatial projection value を保持します。
/// </summary>
public readonly record struct WasmSpatialProjection
{
    /// <summary>[EN] Gets visual direction in radians. [JA] visual direction を radian 単位で取得します。</summary>
    public double VisualDirection { get; init; }

    /// <summary>[EN] Gets auditory correction in radians. [JA] auditory correction を radian 単位で取得します。</summary>
    public double AuditoryCorrection { get; init; }

    /// <summary>[EN] Gets fused direction in radians. [JA] fused direction を radian 単位で取得します。</summary>
    public double FusedDirection { get; init; }

    /// <summary>[EN] Gets projected HUD X coordinate. [JA] projected HUD X coordinate を取得します。</summary>
    public double HudX { get; init; }

    /// <summary>[EN] Gets projected HUD Y coordinate. [JA] projected HUD Y coordinate を取得します。</summary>
    public double HudY { get; init; }

    /// <summary>[EN] Gets projection confidence. [JA] projection confidence を取得します。</summary>
    public double Confidence { get; init; }
}

/// <summary>
/// [EN] Carries a scenario-independent spatial cognition snapshot.
/// [JA] scenario 非依存の spatial cognition snapshot を保持します。
/// </summary>
public sealed record WasmSpatialCognitionSnapshot
{
    /// <summary>[EN] Gets the snapshot identifier. [JA] snapshot 識別子を取得します。</summary>
    public string SnapshotId { get; init; } = string.Empty;

    /// <summary>[EN] Gets whether spatial composition succeeded. [JA] spatial composition が成功したかを取得します。</summary>
    public bool Succeeded { get; init; }

    /// <summary>[EN] Gets composed spatial signals. [JA] 合成された spatial signal を取得します。</summary>
    public IReadOnlyList<WasmSpatialSignal> Signals { get; init; } = [];

    /// <summary>[EN] Gets normalized sensor inputs used by spatial cognition. [JA] spatial cognition で使用した正規化済み sensor input を取得します。</summary>
    public IReadOnlyDictionary<string, WasmSensorStateDescriptor> SensorInputs { get; init; } =
        new Dictionary<string, WasmSensorStateDescriptor>(StringComparer.Ordinal);

    /// <summary>[EN] Gets an optional retry intent carrier emitted by priority sensors. [JA] 優先 sensor が出力した任意の retry intent carrier を取得します。</summary>
    public WasmRetryIntentCarrier? RetryIntent { get; init; }

    /// <summary>[EN] Gets stable failure code when composition failed. [JA] composition が失敗した場合の stable failure code を取得します。</summary>
    public string? ErrorCode { get; init; }

    /// <summary>[EN] Gets human-readable failure message when composition failed. [JA] composition が失敗した場合の人間可読 message を取得します。</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>[EN] Gets composition diagnostics. [JA] composition diagnostics を取得します。</summary>
    public IReadOnlyList<string> Diagnostics { get; init; } = [];

    /// <summary>[EN] Gets snapshot metadata. [JA] snapshot metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// [EN] Describes one WASM-local sensor input state aligned with provider-neutral perception vocabulary.
/// [JA] provider-neutral perception vocabulary と整合する 1 つの WASM-local sensor input state を記述します。
/// </summary>
public sealed record WasmSensorStateDescriptor
{
    /// <summary>[EN] Gets the stable sensor name. [JA] 安定した sensor 名を取得します。</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>[EN] Gets the ASCII-safe philosophical concept name. [JA] ASCII-safe な哲学的 concept 名を取得します。</summary>
    public string ConceptName { get; init; } = string.Empty;

    /// <summary>[EN] Gets the English sensor name associated with the concept. [JA] concept に対応する英語 sensor 名を取得します。</summary>
    public string EnglishName { get; init; } = string.Empty;

    /// <summary>[EN] Gets the sensor category such as primary or derived. [JA] primary / derived などの sensor category を取得します。</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>[EN] Gets whether this sensor is enabled. [JA] この sensor が有効かどうかを取得します。</summary>
    public bool Enabled { get; init; }

    /// <summary>[EN] Gets whether this sensor is directly observed. [JA] この sensor が直接観測されたものかどうかを取得します。</summary>
    public bool Observed { get; init; }

    /// <summary>[EN] Gets normalized sensor confidence outside CTG GateInput. [JA] CTG GateInput の外側に保持する正規化済み sensor confidence を取得します。</summary>
    public double? Confidence { get; init; }

    /// <summary>[EN] Gets sensor timestamp in ISO-8601 form. [JA] ISO-8601 形式の sensor timestamp を取得します。</summary>
    public string? Timestamp { get; init; }

    /// <summary>[EN] Gets deterministic sensor metadata. [JA] deterministic sensor metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// [EN] Carries a WASM-local high-priority retry intent without invoking Gate logic.
/// [JA] Gate logic を呼び出さない WASM-local な高優先 retry intent を保持します。
/// </summary>
public sealed record WasmRetryIntentCarrier
{
    /// <summary>[EN] Gets whether retry is requested. [JA] retry が要求されているかどうかを取得します。</summary>
    public bool Requested { get; init; }

    /// <summary>[EN] Gets the retry reason code. [JA] retry reason code を取得します。</summary>
    public string ReasonCode { get; init; } = string.Empty;

    /// <summary>[EN] Gets deterministic retry priority where larger values win. [JA] 値が大きいほど優先される deterministic retry priority を取得します。</summary>
    public int Priority { get; init; }

    /// <summary>[EN] Gets normalized retry confidence outside CTG GateInput. [JA] CTG GateInput の外側に保持する正規化済み retry confidence を取得します。</summary>
    public double Confidence { get; init; }

    /// <summary>[EN] Gets the source sensor name. [JA] source sensor 名を取得します。</summary>
    public string SourceSensor { get; init; } = string.Empty;

    /// <summary>[EN] Gets deterministic retry metadata. [JA] deterministic retry metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// [EN] Describes one scenario-independent spatial cognition signal.
/// [JA] 1 つの scenario 非依存 spatial cognition signal を記述します。
/// </summary>
public sealed record WasmSpatialSignal
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
