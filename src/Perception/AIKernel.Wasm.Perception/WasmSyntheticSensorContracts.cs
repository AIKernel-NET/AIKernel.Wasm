namespace AIKernel.Wasm.Perception;

using AIKernel.Common.Results;

/// <summary>
/// [EN] Builds Phainesis phenomena and Nous meaning vectors from resident perception buffers.
/// [JA] resident perception buffer から Phainesis の現象と Nous の意味ベクトルを構築します。
/// </summary>
public interface IWasmSyntheticSensorProvider
{
    /// <summary>
    /// [EN] Analyzes resident buffers into synthetic sensor phenomena and vectors.
    /// [JA] resident buffer を合成 sensor の現象とベクトルへ解析します。
    /// </summary>
    /// <param name="request">[EN] Synthetic sensor request. [JA] 合成 sensor request です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視するトークンです。</param>
    /// <returns>[EN] Synthetic sensor snapshot. [JA] 合成 sensor snapshot を返します。</returns>
    ValueTask<WasmSyntheticSensorSnapshot> AnalyzeAsync(
        WasmSyntheticSensorRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// [EN] Provides a monadic LINQ-ready synthetic sensor analysis pipeline surface.
/// [JA] monad LINQ 対応の合成 sensor analysis pipeline surface を提供します。
/// </summary>
public interface IWasmSyntheticSensorPipeline
{
    /// <summary>
    /// [EN] Safely analyzes resident buffers as a Result for LINQ pipeline composition.
    /// [JA] LINQ pipeline 合成用に resident buffer を Result として安全に解析します。
    /// </summary>
    /// <param name="request">[EN] Synthetic sensor request. [JA] 合成 sensor request です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視するトークンです。</param>
    /// <returns>[EN] Result-wrapped synthetic sensor snapshot. [JA] Result で包まれた合成 sensor snapshot を返します。</returns>
    Task<Result<WasmSyntheticSensorSnapshot>> TryAnalyzeAsync(
        WasmSyntheticSensorRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// [EN] Plans WebGPU resident kernel descriptors for synthetic sensor analysis.
/// [JA] 合成 sensor 解析用の WebGPU resident kernel descriptor を計画します。
/// </summary>
public interface IWasmSyntheticSensorKernelPlanner
{
    /// <summary>
    /// [EN] Creates a fused resident kernel descriptor for Phainesis and Nous extraction.
    /// [JA] Phainesis / Nous 抽出用の fused resident kernel descriptor を作成します。
    /// </summary>
    /// <param name="request">[EN] Synthetic sensor request. [JA] 合成 sensor request です。</param>
    /// <param name="phenomenonCount">[EN] Number of phenomenon outputs. [JA] phenomenon output 数です。</param>
    /// <param name="vectorCount">[EN] Number of vector outputs. [JA] vector output 数です。</param>
    /// <returns>[EN] Resident kernel descriptor. [JA] resident kernel descriptor を返します。</returns>
    WasmResidentKernelDescriptor CreateKernelDescriptor(
        WasmSyntheticSensorRequest request,
        int phenomenonCount,
        int vectorCount);
}

/// <summary>
/// [EN] Carries resident buffers and normalized sensor scalars for Phainesis and Nous analysis.
/// [JA] Phainesis / Nous 解析用の resident buffer と正規化 sensor scalar を保持します。
/// </summary>
public sealed record WasmSyntheticSensorRequest
{
    /// <summary>[EN] Gets the request identifier. [JA] request 識別子を取得します。</summary>
    public string RequestId { get; init; } = string.Empty;

    /// <summary>[EN] Gets the current scalar frame buffer. [JA] 現在の scalar frame buffer を取得します。</summary>
    public WasmScalarBuffer CurrentFrame { get; init; } = new();

    /// <summary>[EN] Gets the previous scalar frame buffer. [JA] 前回の scalar frame buffer を取得します。</summary>
    public WasmScalarBuffer PreviousFrame { get; init; } = new();

    /// <summary>[EN] Gets a mask where larger values indicate navigable or open regions. [JA] 値が大きいほど通行可能または開いた領域を示す mask を取得します。</summary>
    public WasmScalarBuffer NavigableMask { get; init; } = new();

    /// <summary>[EN] Gets a mask where larger values indicate moving or dangerous regions. [JA] 値が大きいほど移動物体または危険領域を示す mask を取得します。</summary>
    public WasmScalarBuffer ThreatMask { get; init; } = new();

    /// <summary>[EN] Gets normalized scalar sensor values outside GateInput. [JA] GateInput 外で保持する正規化済み scalar sensor value を取得します。</summary>
    public IReadOnlyDictionary<string, double> SensorScalars { get; init; } =
        new Dictionary<string, double>(StringComparer.Ordinal);

    /// <summary>[EN] Gets deterministic request metadata. [JA] deterministic request metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// [EN] Carries one Phainesis phenomenon extracted from resident perception data.
/// [JA] resident perception data から抽出された 1 つの Phainesis 現象を保持します。
/// </summary>
public sealed record WasmPhenomenonSignal
{
    /// <summary>[EN] Gets the stable phenomenon name. [JA] 安定した phenomenon 名を取得します。</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>[EN] Gets the semantic axis such as Logos, Pathos, Ethos, or Meta. [JA] Logos / Pathos / Ethos / Meta などの semantic axis を取得します。</summary>
    public string Axis { get; init; } = string.Empty;

    /// <summary>[EN] Gets whether the phenomenon is active. [JA] phenomenon が active かどうかを取得します。</summary>
    public bool Active { get; init; }

    /// <summary>[EN] Gets the normalized phenomenon score. [JA] 正規化済み phenomenon score を取得します。</summary>
    public double Score { get; init; }

    /// <summary>[EN] Gets normalized X direction. [JA] 正規化済み X direction を取得します。</summary>
    public double DirectionX { get; init; }

    /// <summary>[EN] Gets normalized Y direction. [JA] 正規化済み Y direction を取得します。</summary>
    public double DirectionY { get; init; }

    /// <summary>[EN] Gets normalized confidence. [JA] 正規化済み confidence を取得します。</summary>
    public double Confidence { get; init; }

    /// <summary>[EN] Gets deterministic phenomenon metadata. [JA] deterministic phenomenon metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// [EN] Carries one Nous meaning vector derived from Phainesis phenomena.
/// [JA] Phainesis 現象から派生した 1 つの Nous 意味ベクトルを保持します。
/// </summary>
public sealed record WasmMeaningVector
{
    /// <summary>[EN] Gets the stable vector name. [JA] 安定した vector 名を取得します。</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>[EN] Gets the semantic axis such as Logos, Pathos, Ethos, or Meta. [JA] Logos / Pathos / Ethos / Meta などの semantic axis を取得します。</summary>
    public string Axis { get; init; } = string.Empty;

    /// <summary>[EN] Gets normalized X direction. [JA] 正規化済み X direction を取得します。</summary>
    public double X { get; init; }

    /// <summary>[EN] Gets normalized Y direction. [JA] 正規化済み Y direction を取得します。</summary>
    public double Y { get; init; }

    /// <summary>[EN] Gets normalized vector strength. [JA] 正規化済み vector strength を取得します。</summary>
    public double Strength { get; init; }

    /// <summary>[EN] Gets normalized vector confidence. [JA] 正規化済み vector confidence を取得します。</summary>
    public double Confidence { get; init; }

    /// <summary>[EN] Gets deterministic vector metadata. [JA] deterministic vector metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// [EN] Carries Phainesis and Nous synthetic sensor analysis output.
/// [JA] Phainesis / Nous 合成 sensor 解析 output を保持します。
/// </summary>
public sealed record WasmSyntheticSensorSnapshot
{
    /// <summary>[EN] Gets the snapshot identifier. [JA] snapshot 識別子を取得します。</summary>
    public string SnapshotId { get; init; } = string.Empty;

    /// <summary>[EN] Gets whether synthetic sensor analysis succeeded. [JA] 合成 sensor 解析が成功したかを取得します。</summary>
    public bool Succeeded { get; init; }

    /// <summary>[EN] Gets extracted Phainesis phenomena. [JA] 抽出された Phainesis 現象を取得します。</summary>
    public IReadOnlyList<WasmPhenomenonSignal> Phenomena { get; init; } = [];

    /// <summary>[EN] Gets derived Nous meaning vectors. [JA] 派生した Nous 意味ベクトルを取得します。</summary>
    public IReadOnlyList<WasmMeaningVector> Vectors { get; init; } = [];

    /// <summary>[EN] Gets the resident kernel descriptor used for flow analysis. [JA] flow 解析に使った resident kernel descriptor を取得します。</summary>
    public WasmResidentKernelDescriptor FlowKernel { get; init; } = new();

    /// <summary>[EN] Gets the fused WebGPU kernel descriptor for synthetic sensor analysis. [JA] 合成 sensor 解析用の fused WebGPU kernel descriptor を取得します。</summary>
    public WasmResidentKernelDescriptor SyntheticKernel { get; init; } = new();

    /// <summary>[EN] Gets stable failure code when analysis failed. [JA] 解析失敗時の stable failure code を取得します。</summary>
    public string? ErrorCode { get; init; }

    /// <summary>[EN] Gets human-readable failure message when analysis failed. [JA] 解析失敗時の人間可読 failure message を取得します。</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>[EN] Gets deterministic snapshot metadata. [JA] deterministic snapshot metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}
