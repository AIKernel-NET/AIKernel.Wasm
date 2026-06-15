namespace AIKernel.Wasm.Perception;

using AIKernel.Abstractions.Perception;
using AIKernel.Dtos.Frame;
using AIKernel.Dtos.Perception;
using AIKernel.Dtos.Providers;
using AIKernel.Enums;
using AIKernel.Wasm.Runtime;

/// <summary>
/// [EN] Converts WASM framebuffer snapshots into bounded symbolic perception signals.
/// [JA] WASM framebuffer snapshot を bounded symbolic perception signal に変換します。
/// </summary>
public sealed class WasmFramePerceptionProvider : WasmKernelProviderBase, IFramePerceptionProvider
{
    /// <summary>
    /// [EN] Initializes a WASM frame perception provider.
    /// [JA] WASM frame perception Provider を初期化します。
    /// </summary>
    public WasmFramePerceptionProvider()
        : base(
            "wasm.frame-perception",
            "WASM Frame Perception Provider",
            ["wasm.perception.frame.analyze"],
            ["frame", "image"],
            Capabilities())
    {
    }

    /// <summary>
    /// [EN] Analyzes one frame snapshot without scenario-specific semantics.
    /// [JA] scenario 固有 semantics を使わず 1 つの frame snapshot を解析します。
    /// </summary>
    /// <param name="frame">[EN] Frame snapshot. [JA] frame snapshot です。</param>
    /// <param name="options">[EN] Perception options. [JA] perception options です。</param>
    /// <param name="context">[EN] Provider execution context. [JA] Provider execution context です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視するトークンです。</param>
    /// <returns>[EN] Frame perception result. [JA] frame perception result を返します。</returns>
    public ValueTask<FramePerceptionResult> AnalyzeAsync(
        FrameSnapshot frame,
        FramePerceptionOptions options,
        ProviderExecutionContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(context);

        var metadata = MergeMetadata(frame.Metadata, options.Metadata, context.Metadata);
        var surfaceId = MetadataValue(metadata, "surfaceId") ?? frame.SourceId;
        var diagnostics = new List<ProviderDiagnostic>();
        if (string.IsNullOrWhiteSpace(frame.FrameHash))
        {
            diagnostics.Add(new ProviderDiagnostic
            {
                Code = "WASM_FRAME_HASH_MISSING",
                Message = "Frame perception can proceed, but deterministic frame hash metadata is missing.",
                IsRetryable = false
            });
        }

        var signals = new List<PerceptionSignal>
        {
            CreateSignal("frame.width", "frame.dimension.width", frame.Buffer.Width.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            CreateSignal("frame.height", "frame.dimension.height", frame.Buffer.Height.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            CreateSignal("frame.zero_copy_hint", "frame.surface.zero_copy_hint", frame.Buffer.ZeroCopyHint ? "true" : "false")
        };

        if (MetadataValue(metadata, "byteLength") is { } byteLength)
        {
            signals.Add(CreateSignal("frame.byte_length", "frame.buffer.byte_length", byteLength));
        }

        if (!string.IsNullOrWhiteSpace(frame.FrameHash))
        {
            signals.Add(CreateSignal("frame.hash", "frame.hash.sha256", frame.FrameHash));
        }

        return ValueTask.FromResult(new FramePerceptionResult
        {
            ObservationId = $"{context.ExecutionId}.frame.{frame.FrameIndex}",
            SurfaceId = surfaceId,
            FrameId = frame.FrameId,
            FrameIndex = frame.FrameIndex,
            Regions =
            [
                new PerceptionRegion
                {
                    RegionId = "frame.full",
                    X = 0,
                    Y = 0,
                    Width = 1,
                    Height = 1,
                    Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["source"] = "wasm.framebuffer"
                    }
                }
            ],
            Signals = signals,
            Signatures = string.IsNullOrWhiteSpace(frame.FrameHash)
                ? []
                :
                [
                    new SemanticSignature
                    {
                        SignatureId = "frame.hash",
                        Hash = frame.FrameHash,
                        Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                        {
                            ["algorithm"] = "sha256"
                        }
                    }
                ],
            Motion = options.IncludeMotion
                ? new MotionSignature
                {
                    Confidence = 0,
                    Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["source"] = "single-frame"
                    }
                }
                : null,
            Diagnostics = diagnostics,
            Metadata = metadata
        });
    }

    private static PerceptionSignal CreateSignal(string id, string kind, string value)
        => new()
        {
            SignalId = id,
            Kind = kind,
            Confidence = 1,
            Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["value"] = value
            }
        };

    private static string? MetadataValue(IReadOnlyDictionary<string, string> metadata, string key)
        => metadata.TryGetValue(key, out var value) ? value : null;

    private static IReadOnlyDictionary<string, string> MergeMetadata(params IReadOnlyDictionary<string, string>[] maps)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var map in maps)
        {
            foreach (var item in map.OrderBy(item => item.Key, StringComparer.Ordinal))
            {
                metadata[item.Key] = item.Value;
            }
        }

        return metadata;
    }

    private static IReadOnlyList<ProviderCapability> Capabilities()
        =>
        [
            new()
            {
                LogicalCapabilityId = "wasm.perception.frame",
                ProviderCapabilityId = "wasm.perception.frame.analyze",
                Flags = ProviderCapabilityFlags.FramePerception,
                Kind = ProviderKind.Observer,
                InputModalities = InputModalities.Frame,
                OutputModalities = OutputModalities.Evidence | OutputModalities.Telemetry,
                RiskLevel = ProviderRiskLevel.ReadOnly,
                Availability = new CapabilityAvailability
                {
                    IsAvailable = true,
                    Reason = ProviderAvailabilityReason.Available
                }
            }
        ];
}
