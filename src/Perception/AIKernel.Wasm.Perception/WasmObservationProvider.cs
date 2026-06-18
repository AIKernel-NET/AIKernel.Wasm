namespace AIKernel.Wasm.Perception;

using AIKernel.Abstractions.Perception;
using AIKernel.Dtos.Frame;
using AIKernel.Dtos.Perception;
using AIKernel.Dtos.Providers;
using AIKernel.Enums;
using AIKernel.Wasm.Display;
using AIKernel.Wasm.Runtime;

/// <summary>
/// [EN] Captures a WASM frame and produces a read-only observation snapshot.
/// [JA] WASM frame を capture し read-only observation snapshot を生成します。
/// </summary>
public sealed class WasmObservationProvider : WasmKernelProviderBase, IObservationProvider
{
    private readonly WasmFrameSourceProvider _frameSourceProvider;
    private readonly WasmFramePerceptionProvider _framePerceptionProvider;

    /// <summary>
    /// [EN] Initializes a WASM observation provider.
    /// [JA] WASM observation Provider を初期化します。
    /// </summary>
    /// <param name="frameSourceProvider">[EN] Optional frame source provider. [JA] 任意の frame source Provider です。</param>
    /// <param name="framePerceptionProvider">[EN] Optional frame perception provider. [JA] 任意の frame perception Provider です。</param>
    public WasmObservationProvider(
        WasmFrameSourceProvider? frameSourceProvider = null,
        WasmFramePerceptionProvider? framePerceptionProvider = null)
        : base(
            "wasm.observation",
            "WASM Observation Provider",
            ["wasm.observation.capture"],
            ["frame", "observation"],
            Capabilities())
    {
        _frameSourceProvider = frameSourceProvider ?? new WasmFrameSourceProvider();
        _framePerceptionProvider = framePerceptionProvider ?? new WasmFramePerceptionProvider();
    }

    /// <summary>
    /// [EN] Captures one WASM observation without selecting actions.
    /// [JA] action を選択せず 1 つの WASM observation を capture します。
    /// </summary>
    /// <param name="request">[EN] Observation request. [JA] observation request です。</param>
    /// <param name="context">[EN] Provider execution context. [JA] Provider execution context です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視するトークンです。</param>
    /// <returns>[EN] Observation snapshot. [JA] observation snapshot を返します。</returns>
    public async ValueTask<ObservationSnapshot> ObserveAsync(
        ObservationRequest request,
        ProviderExecutionContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        FrameSnapshot? frame = null;
        await foreach (var captured in _frameSourceProvider.CaptureAsync(
            new FrameCaptureRequest
            {
                SourceId = request.ObservationId,
                MaxFrames = 1,
                Metadata = request.Metadata
            },
            context,
            cancellationToken).ConfigureAwait(false))
        {
            frame = captured;
            break;
        }

        if (frame is null)
        {
            return new ObservationSnapshot
            {
                ObservationId = request.ObservationId,
                Metadata = new Dictionary<string, string>(request.Metadata, StringComparer.Ordinal)
                {
                    ["errorCode"] = "WASM_FRAME_CAPTURE_EMPTY"
                }
            };
        }

        var perception = await _framePerceptionProvider.AnalyzeAsync(
            frame,
            new FramePerceptionOptions
            {
                Metadata = request.Metadata
            },
            context,
            cancellationToken).ConfigureAwait(false);

        return new ObservationSnapshot
        {
            ObservationId = request.ObservationId,
            Perceptions = [perception],
            Metadata = request.Metadata
        };
    }

    private static IReadOnlyList<ProviderCapability> Capabilities()
        =>
        [
            new()
            {
                LogicalCapabilityId = "wasm.observation",
                ProviderCapabilityId = "wasm.observation.capture",
                Flags = ProviderCapabilityFlags.Observation | ProviderCapabilityFlags.FrameSource | ProviderCapabilityFlags.FramePerception,
                Kind = ProviderKind.Observer,
                InputModalities = InputModalities.None,
                OutputModalities = OutputModalities.Evidence | OutputModalities.Frame,
                RiskLevel = ProviderRiskLevel.ReadOnly,
                Availability = new CapabilityAvailability
                {
                    IsAvailable = true,
                    Reason = ProviderAvailabilityReason.Available
                }
            }
        ];
}
