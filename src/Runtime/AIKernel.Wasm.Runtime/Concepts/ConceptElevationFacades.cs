namespace AIKernel.Wasm.Runtime.Concepts;

/// <summary>
/// [Perception layer - Aisthesis / アイステーシス]
/// [EN] Concept facade for raw framebuffer observation from a WASM runtime surface.
/// [JA] WASM runtime surface からの raw framebuffer observation を扱う概念 facade です。
/// Old technical name: FrameSource / Observation.
/// Do not use this term for DTO, JSInterop, NativeBridge, Mapper, or Provider implementation names.
/// </summary>
public sealed class AisthesisFrameSource
{
    /// <summary>
    /// [EN] Captures a defensive copy of raw frame bytes.
    /// [JA] raw frame byte の defensive copy を取得します。
    /// </summary>
    public byte[] Capture(ReadOnlyMemory<byte> frame)
        => frame.ToArray();
}

/// <summary>
/// [Perception layer - Aisthesis / アイステーシス]
/// [EN] Concept facade for the observable WASM runtime surface, above framebuffer and screenshot providers.
/// [JA] framebuffer / screenshot Provider より上位にある observable WASM runtime surface の概念 facade です。
/// Old technical name: ObservationSurface.
/// Do not use this term for DTO, JSInterop, NativeBridge, Mapper, or Provider implementation names.
/// </summary>
public sealed class AisthesisObservationSurface
{
    /// <summary>
    /// [EN] Creates a stable observation surface identifier.
    /// [JA] 安定した observation surface identifier を作成します。
    /// </summary>
    public string SurfaceId(string runtimeId)
        => string.IsNullOrWhiteSpace(runtimeId)
            ? "aisthesis.surface"
            : $"aisthesis.surface.{runtimeId}";
}

/// <summary>
/// [Perception layer - Phantasia / ファンタシア]
/// [EN] Concept facade for scene-surface representation derived from WASM observation.
/// [JA] WASM observation から導出される scene surface representation の概念 facade です。
/// Old technical name: SceneSurface.
/// Do not use this term for DTO, JSInterop, NativeBridge, Mapper, or Provider implementation names.
/// </summary>
public sealed class PhantasiaSceneSurface
{
    /// <summary>
    /// [EN] Creates a stable scene label.
    /// [JA] 安定した scene label を作成します。
    /// </summary>
    public string Label(string surfaceId)
        => string.IsNullOrWhiteSpace(surfaceId)
            ? "phantasia.scene"
            : $"phantasia.scene.{surfaceId}";
}

/// <summary>
/// [Perception layer - Phantasia / ファンタシア]
/// [EN] Concept facade for a frame-level visual representation derived from raw WASM observation.
/// [JA] raw WASM observation から導出される frame-level visual representation の概念 facade です。
/// Old technical name: FrameModel.
/// Do not use this term for DTO, JSInterop, NativeBridge, Mapper, or Provider implementation names.
/// </summary>
public sealed class PhantasiaFrameModel
{
    /// <summary>
    /// [EN] Creates a deterministic frame model key from source and frame identifiers.
    /// [JA] source / frame identifier から deterministic な frame model key を作成します。
    /// </summary>
    public string Key(string sourceId, long frameIndex)
        => $"{sourceId}:{frameIndex}";
}

/// <summary>
/// [Temporal layer - Chronos / クロノス]
/// [EN] Concept facade for objective replay windows and deterministic runtime time.
/// [JA] objective replay window と deterministic runtime time を扱う概念 facade です。
/// Old technical name: ReplayWindow.
/// Do not use this term for DTO, JSInterop, NativeBridge, Mapper, or Provider implementation names.
/// </summary>
public sealed class ChronosWindow
{
    /// <summary>
    /// [EN] Returns whether a timestamp belongs to the inclusive temporal window.
    /// [JA] timestamp が inclusive temporal window に属するかを返します。
    /// </summary>
    public bool Contains(DateTimeOffset timestamp, DateTimeOffset start, DateTimeOffset end)
        => timestamp >= start && timestamp <= end;
}

/// <summary>
/// [Temporal layer - Chronos / クロノス]
/// [EN] Concept facade for deterministic temporal buffers.
/// [JA] deterministic temporal buffer を扱う概念 facade です。
/// Old technical name: TimeBuffer.
/// Do not use this term for DTO, JSInterop, NativeBridge, Mapper, or Provider implementation names.
/// </summary>
public sealed class ChronosBuffer
{
    /// <summary>
    /// [EN] Orders timestamps deterministically.
    /// [JA] timestamp を決定論的に整列します。
    /// </summary>
    public IReadOnlyList<DateTimeOffset> Order(IEnumerable<DateTimeOffset> timestamps)
        => timestamps.Order().ToArray();
}

/// <summary>
/// [Temporal layer - Chronos / クロノス]
/// [EN] Concept facade for replay windows over deterministic WASM runtime history.
/// [JA] deterministic な WASM runtime history 上の replay window を扱う概念 facade です。
/// Old technical name: ReplayWindow.
/// Do not use this term for DTO, JSInterop, NativeBridge, Mapper, or Provider implementation names.
/// </summary>
public sealed class ChronosReplayWindow
{
    /// <summary>
    /// [EN] Returns a deterministic replay window label.
    /// [JA] deterministic な replay window label を返します。
    /// </summary>
    public string Label(DateTimeOffset start, DateTimeOffset end)
        => $"{start:O}/{end:O}";
}

/// <summary>
/// [Temporal layer - Chronos / クロノス]
/// [EN] Concept facade for audio observation windows above runtime audio buffers.
/// [JA] runtime audio buffer より上位にある audio observation window の概念 facade です。
/// Old technical name: AudioWindow.
/// Do not use this term for DTO, JSInterop, NativeBridge, Mapper, or Provider implementation names.
/// </summary>
public sealed class ChronosAudioWindow
{
    /// <summary>
    /// [EN] Returns whether an audio timestamp belongs to the inclusive temporal window.
    /// [JA] audio timestamp が inclusive temporal window に属するかを返します。
    /// </summary>
    public bool Contains(TimeSpan timestamp, TimeSpan start, TimeSpan end)
        => timestamp >= start && timestamp <= end;
}

/// <summary>
/// [Temporal layer - Chronos / クロノス]
/// [EN] Concept facade for deterministic audio buffer duration calculations.
/// [JA] deterministic な audio buffer duration calculation の概念 facade です。
/// Old technical name: AudioBuffer.
/// Do not use this term for DTO, JSInterop, NativeBridge, Mapper, or Provider implementation names.
/// </summary>
public sealed class ChronosAudioBuffer
{
    /// <summary>
    /// [EN] Calculates duration from sample count and sample rate.
    /// [JA] sample count と sample rate から duration を計算します。
    /// </summary>
    public TimeSpan Duration(int sampleCount, int sampleRate)
        => sampleRate <= 0 || sampleCount <= 0
            ? TimeSpan.Zero
            : TimeSpan.FromSeconds((double)sampleCount / sampleRate);
}

/// <summary>
/// [Temporal layer - Chronos / クロノス]
/// [EN] Concept facade for deterministic playback timeline labels.
/// [JA] deterministic な playback timeline label の概念 facade です。
/// Old technical name: PlaybackTimeline.
/// Do not use this term for DTO, JSInterop, NativeBridge, Mapper, or Provider implementation names.
/// </summary>
public sealed class ChronosPlaybackTimeline
{
    /// <summary>
    /// [EN] Creates a stable playback segment label.
    /// [JA] 安定した playback segment label を作成します。
    /// </summary>
    public string Segment(string sourceId, TimeSpan offset)
        => $"{sourceId}:{offset.Ticks}";
}

/// <summary>
/// [Timing layer - Kairos / カイロス]
/// [EN] Concept facade for WASM timing triggers that remain outside Gate logic.
/// [JA] Gate logic の外側にある WASM timing trigger の概念 facade です。
/// Old technical name: RuntimeTrigger.
/// Do not use this term for DTO, JSInterop, NativeBridge, Mapper, or Provider implementation names.
/// </summary>
public sealed class KairosTrigger
{
    /// <summary>
    /// [EN] Returns whether the trigger is ready after a deterministic delay.
    /// [JA] deterministic delay の後に trigger が ready かどうかを返します。
    /// </summary>
    public bool IsReady(DateTimeOffset now, DateTimeOffset scheduledAt, TimeSpan delay)
        => now >= scheduledAt.Add(delay);
}

/// <summary>
/// [Timing layer - Kairos / カイロス]
/// [EN] Concept facade for frame-bound runtime event signals that do not decide Gate outcomes.
/// [JA] Gate outcome を判断しない frame-bound runtime event signal の概念 facade です。
/// Old technical name: FrameSignal.
/// Do not use this term for DTO, JSInterop, NativeBridge, Mapper, or Provider implementation names.
/// </summary>
public sealed class KairosFrameSignal
{
    /// <summary>
    /// [EN] Creates a stable signal label for a runtime frame.
    /// [JA] runtime frame 用の安定した signal label を作成します。
    /// </summary>
    public string Label(string eventName, long frameIndex)
        => $"{eventName}:{frameIndex}";
}

/// <summary>
/// [Timing layer - Kairos / カイロス]
/// [EN] Concept facade for meaningful action timing above virtual input providers.
/// [JA] virtual input Provider より上位にある meaningful action timing の概念 facade です。
/// Old technical name: ActionTiming.
/// Do not use this term for DTO, JSInterop, NativeBridge, Mapper, or Provider implementation names.
/// </summary>
public sealed class KairosActionTiming
{
    /// <summary>
    /// [EN] Returns whether an action timestamp is within the trigger tolerance.
    /// [JA] action timestamp が trigger tolerance 内にあるかどうかを返します。
    /// </summary>
    public bool IsWithinTolerance(DateTimeOffset actionAt, DateTimeOffset triggerAt, TimeSpan tolerance)
        => actionAt >= triggerAt.Subtract(tolerance) && actionAt <= triggerAt.Add(tolerance);
}

/// <summary>
/// [Timing layer - Kairos / カイロス]
/// [EN] Concept facade for meaningful input triggers above technical virtual input packets.
/// [JA] 技術的な virtual input packet より上位にある meaningful input trigger の概念 facade です。
/// Old technical name: InputTrigger.
/// Do not use this term for DTO, JSInterop, NativeBridge, Mapper, or Provider implementation names.
/// </summary>
public sealed class KairosInputTrigger
{
    /// <summary>
    /// [EN] Creates a stable input trigger label.
    /// [JA] 安定した input trigger label を作成します。
    /// </summary>
    public string Label(string inputId, DateTimeOffset observedAt)
        => $"{inputId}:{observedAt:O}";
}
