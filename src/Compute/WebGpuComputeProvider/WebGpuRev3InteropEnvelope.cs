namespace AIKernel.Wasm.Compute;

using AIKernel.Dtos.Gpu;
using AIKernel.Enums;

/// <summary>
/// [EN] Canonical rev3 pass kind exposed at the browser/native WebGPU boundary.
/// [JA] browser/native WebGPU 境界で公開する canonical rev3 pass 種別です。
/// </summary>
public enum WebGpuRev3PassKind
{
    /// <summary>[EN] GPU Aisthesis over a raw framebuffer. [JA] raw framebuffer に対する GPU Aisthesis です。</summary>
    Aisthesis = 1,

    /// <summary>[EN] GPU spatial reasoning over Topos/Route/Threat/Zoe matrices. [JA] Topos/Route/Threat/Zoe 行列に対する GPU spatial reasoning です。</summary>
    SpatialReasoning = 2,

    /// <summary>[EN] GPU HUD composite pass. [JA] GPU HUD composite pass です。</summary>
    HudComposite = 3
}

/// <summary>
/// [EN] Stable frame identity passed to JavaScript or Dawn bridge code.
/// [JA] JavaScript または Dawn bridge code へ渡す安定した frame identity です。
/// </summary>
public sealed record WebGpuRev3FrameEnvelope
{
    /// <summary>[EN] Frame id. [JA] frame id です。</summary>
    public required string FrameId { get; init; }

    /// <summary>[EN] Monotonic frame index. [JA] monotonic frame index です。</summary>
    public long FrameIndex { get; init; }

    /// <summary>[EN] Deterministic sample ticks. [JA] deterministic sample tick です。</summary>
    public long SampleTicks { get; init; }
}

/// <summary>
/// [EN] Stable GPU target descriptor for bridge code.
/// [JA] bridge code 用の安定した GPU target descriptor です。
/// </summary>
public sealed record WebGpuRev3TargetEnvelope
{
    /// <summary>[EN] Target role in the pass, such as raw, hud, or mask. [JA] raw、hud、mask など pass 内の target role です。</summary>
    public required string Role { get; init; }

    /// <summary>[EN] Canonical target id. [JA] canonical target id です。</summary>
    public required string TargetId { get; init; }

    /// <summary>[EN] Canonical target kind. [JA] canonical target kind です。</summary>
    public GpuFrameTargetKind Kind { get; init; }

    /// <summary>[EN] Backend that owns the target. [JA] target を所有する backend です。</summary>
    public GpuBackend Backend { get; init; }

    /// <summary>[EN] Target width in pixels. [JA] target の pixel 幅です。</summary>
    public int Width { get; init; }

    /// <summary>[EN] Target height in pixels. [JA] target の pixel 高です。</summary>
    public int Height { get; init; }

    /// <summary>[EN] Pixel format. [JA] pixel format です。</summary>
    public FramePixelFormat PixelFormat { get; init; }

    /// <summary>[EN] True when bridge code may bind the target without CPU readback. [JA] bridge code が CPU readback なしで target を bind できる場合 true です。</summary>
    public bool ZeroCopy { get; init; }
}

/// <summary>
/// [EN] Flat buffer descriptor passed to WebGPU bridge code.
/// [JA] WebGPU bridge code へ渡す flat buffer descriptor です。
/// </summary>
public sealed record WebGpuRev3BufferEnvelope
{
    /// <summary>[EN] Buffer name inside the pass. [JA] pass 内の buffer 名です。</summary>
    public required string Name { get; init; }

    /// <summary>[EN] Canonical layout name. [JA] canonical layout 名です。</summary>
    public required string LayoutName { get; init; }

    /// <summary>[EN] Element stride in float values. [JA] float 値単位の element stride です。</summary>
    public int Stride { get; init; }

    /// <summary>[EN] Logical item count. [JA] 論理 item count です。</summary>
    public int Count { get; init; }

    /// <summary>[EN] Flattened float values. [JA] flatten 済み float 値です。</summary>
    public IReadOnlyList<float> Values { get; init; } = [];
}

/// <summary>
/// [EN] Stable canonical rev3 dispatch envelope for JS/Dawn bridge implementations.
/// [JA] JS/Dawn bridge 実装用の安定した canonical rev3 dispatch envelope です。
/// </summary>
public sealed record WebGpuRev3DispatchEnvelope
{
    /// <summary>[EN] Envelope schema id. [JA] envelope schema id です。</summary>
    public string Schema { get; init; } = "aikernel.gpu.rev3.dispatch";

    /// <summary>[EN] Canonical schema version. [JA] canonical schema version です。</summary>
    public string Version { get; init; } = "0.1.3";

    /// <summary>[EN] Stable pass id. [JA] 安定した pass id です。</summary>
    public required string PassId { get; init; }

    /// <summary>[EN] Pass kind. [JA] pass 種別です。</summary>
    public WebGpuRev3PassKind PassKind { get; init; }

    /// <summary>[EN] Frame identity. [JA] frame identity です。</summary>
    public required WebGpuRev3FrameEnvelope Frame { get; init; }

    /// <summary>[EN] Target descriptors used by this pass. [JA] この pass が使用する target descriptor です。</summary>
    public IReadOnlyList<WebGpuRev3TargetEnvelope> Targets { get; init; } = [];

    /// <summary>[EN] Flat buffers used by this pass. [JA] この pass が使用する flat buffer です。</summary>
    public IReadOnlyList<WebGpuRev3BufferEnvelope> Buffers { get; init; } = [];

    /// <summary>[EN] Feature flags in deterministic key order. [JA] deterministic key order の feature flag です。</summary>
    public IReadOnlyDictionary<string, bool> FeatureFlags { get; init; } = new Dictionary<string, bool>(StringComparer.Ordinal);

    /// <summary>[EN] Scalar values consumed by shaders or bridge code. [JA] shader または bridge code が消費する scalar 値です。</summary>
    public IReadOnlyDictionary<string, float> Scalars { get; init; } = new Dictionary<string, float>(StringComparer.Ordinal);

    /// <summary>[EN] Text labels retained for the lightweight DOM/text layer. [JA] lightweight DOM/text layer 用に保持する text label です。</summary>
    public IReadOnlyList<GpuHudLabel> Labels { get; init; } = [];

    /// <summary>[EN] Canonical rev3 diagnostics metadata carried with the dispatch. [JA] dispatch と一緒に渡す canonical rev3 diagnostics metadata です。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>[EN] Deterministic metadata tags for diagnostics. [JA] diagnostics 用の deterministic metadata tag です。</summary>
    public IReadOnlyDictionary<string, string> Tags { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// [EN] Creates JS/Dawn friendly rev3 envelopes from canonical GPU DTOs.
/// [JA] canonical GPU DTO から JS/Dawn で扱いやすい rev3 envelope を作成します。
/// </summary>
public static class WebGpuRev3InteropEnvelope
{
    private static readonly string[] MatrixNames = ["topos", "route", "threat", "zoe"];

    /// <summary>
    /// [EN] Creates an Aisthesis dispatch envelope that preserves the raw-framebuffer capture split.
    /// [JA] raw-framebuffer capture split を保持した Aisthesis dispatch envelope を作成します。
    /// </summary>
    public static WebGpuRev3DispatchEnvelope ForAisthesis(GpuAisthesisInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ThrowIfInvalid(GpuCanonicalValidation.ValidateAisthesisInput(input));

        return new WebGpuRev3DispatchEnvelope
        {
            PassId = GpuOperationNames.GpuAisthesisRawFrame,
            PassKind = WebGpuRev3PassKind.Aisthesis,
            Frame = ToFrame(input.Frame),
            Targets = [ToTarget("raw", input.RawFramebuffer)],
            FeatureFlags = Sort(input.Features),
            Metadata = DispatchMetadata(
                GpuOperationNames.GpuAisthesisRawFrame,
                input.Frame,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [GpuProviderMetadataKeys.RawCaptureSource] = "raw-framebuffer",
                    ["zero_copy"] = input.RawFramebuffer.ZeroCopy ? "true" : "false"
                }),
            Tags = Sort(new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [GpuProviderMetadataKeys.RawCaptureSource] = "raw-framebuffer",
                ["zero_copy"] = input.RawFramebuffer.ZeroCopy ? "true" : "false"
            })
        };
    }

    /// <summary>
    /// [EN] Creates a spatial reasoning dispatch envelope with Topos/Route/Threat/Zoe matrices in canonical order.
    /// [JA] Topos/Route/Threat/Zoe 行列を canonical order で持つ spatial reasoning dispatch envelope を作成します。
    /// </summary>
    public static WebGpuRev3DispatchEnvelope ForSpatialReasoning(GpuSpatialReasoningInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ThrowIfInvalid(GpuCanonicalValidation.ValidateSpatialReasoningInput(input));

        var buffers = new List<WebGpuRev3BufferEnvelope>(GpuCanonicalLayouts.AisMatrix.MaxItems + 1);
        for (var index = 0; index < input.AisMatrices.Count; index++)
        {
            buffers.Add(new WebGpuRev3BufferEnvelope
            {
                Name = $"ais-matrix:{MatrixNames[index]}",
                LayoutName = GpuCanonicalLayouts.AisMatrix.Name,
                Stride = GpuCanonicalLayouts.AisMatrix.Stride,
                Count = 1,
                Values = input.AisMatrices[index].ToArray()
            });
        }

        buffers.Add(new WebGpuRev3BufferEnvelope
        {
            Name = "state-vector",
            LayoutName = GpuCanonicalLayouts.StateVector.Name,
            Stride = GpuCanonicalLayouts.StateVector.Stride,
            Count = 1,
            Values = Pad(input.StateVector, GpuCanonicalLayouts.StateVector.Stride)
        });

        return new WebGpuRev3DispatchEnvelope
        {
            PassId = GpuOperationNames.GpuSpatialReasoning,
            PassKind = WebGpuRev3PassKind.SpatialReasoning,
            Frame = ToFrame(input.Frame),
            Targets = TargetsForFrame(input.Frame),
            Buffers = buffers,
            Metadata = DispatchMetadata(
                GpuOperationNames.GpuSpatialReasoning,
                input.Frame,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    [GpuProviderMetadataKeys.AisMatrixOrder] = "topos,route,threat,zoe",
                    ["state_vector_stride"] = GpuCanonicalLayouts.StateVector.Stride.ToString(System.Globalization.CultureInfo.InvariantCulture)
                }),
            Tags = Sort(new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [GpuProviderMetadataKeys.AisMatrixOrder] = "topos,route,threat,zoe",
                ["state_vector_stride"] = GpuCanonicalLayouts.StateVector.Stride.ToString(System.Globalization.CultureInfo.InvariantCulture)
            })
        };
    }

    /// <summary>
    /// [EN] Creates a HUD composite dispatch envelope with flat panel buffers and final ego-radar scalars.
    /// [JA] flat panel buffer と最終 ego-radar scalar を持つ HUD composite dispatch envelope を作成します。
    /// </summary>
    public static WebGpuRev3DispatchEnvelope ForHudComposite(GpuHudInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ThrowIfInvalid(GpuCanonicalValidation.ValidateHudInput(input));

        return new WebGpuRev3DispatchEnvelope
        {
            PassId = GpuOperationNames.GpuHudComposite,
            PassKind = WebGpuRev3PassKind.HudComposite,
            Frame = ToFrame(input.Frame),
            Targets = TargetsForFrame(input.Frame),
            Buffers =
            [
                ToBuffer("hud-panel-rects", GpuCanonicalLayouts.HudPanelRect, input.HudPanelRects),
                ToBuffer("hud-panel-state-vectors", GpuCanonicalLayouts.HudPanelStateVector, input.HudPanelStateVectors)
            ],
            Scalars = EgoRadarScalars(input.EgoRadar),
            Labels = input.Labels
                .OrderBy(static label => label.Id, StringComparer.Ordinal)
                .ToArray(),
            Metadata = DispatchMetadata(
                GpuOperationNames.GpuHudComposite,
                input.Frame,
                SortOptional(new Dictionary<string, string?>(StringComparer.Ordinal)
                {
                    ["ego_radar_mode"] = input.EgoRadar.RadarMode.ToString(),
                    ["summary"] = input.EgoRadar.Summary
                })),
            Tags = SortOptional(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["ego_radar_mode"] = input.EgoRadar.RadarMode.ToString(),
                ["summary"] = input.EgoRadar.Summary
            })
        };
    }

    /// <summary>
    /// [EN] Validates a stable rev3 dispatch envelope before bridge handoff.
    /// [JA] bridge handoff 前に安定した rev3 dispatch envelope を検証します。
    /// </summary>
    public static GpuValidationResult ValidateDispatchEnvelope(WebGpuRev3DispatchEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        var issues = new List<GpuValidationIssue>();
        if (!string.Equals(envelope.Schema, WebGpuRev3BrowserBridgeAssets.Schema, StringComparison.Ordinal))
        {
            issues.Add(Error(
                "WEBGPU_REV3_ENVELOPE_SCHEMA_MISMATCH",
                $"Rev3 dispatch envelope schema must be {WebGpuRev3BrowserBridgeAssets.Schema}.",
                "envelope.schema"));
        }

        if (!string.Equals(envelope.Version, WebGpuRev3BrowserBridgeAssets.Version, StringComparison.Ordinal))
        {
            issues.Add(Error(
                "WEBGPU_REV3_ENVELOPE_VERSION_MISMATCH",
                $"Rev3 dispatch envelope version must be {WebGpuRev3BrowserBridgeAssets.Version}.",
                "envelope.version"));
        }

        ValidatePassIdentity(envelope, issues);
        ValidateFrame(envelope, issues);
        ValidateTargets(envelope, issues);
        ValidateBuffers(envelope, issues);
        ValidateMetadata(envelope, issues);

        return GpuValidationResult.FromIssues(issues);
    }

    private static IReadOnlyDictionary<string, string> DispatchMetadata(
        string passId,
        GpuFrameToken frame,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [GpuDiagnosticsMetadataKeys.Rev3CandidateStreak] = "1",
            [GpuDiagnosticsMetadataKeys.Rev3DiagnosticReady] = "true",
            [GpuDiagnosticsMetadataKeys.Rev3DiagnosticStreak] = "1",
            [GpuDiagnosticsMetadataKeys.Rev3ExecutionMode] = GpuRev3ExecutionModes.BrowserWebGpuCompute,
            [GpuDiagnosticsMetadataKeys.Rev3PilotState] = GpuRev3PilotStates.BrowserBuiltinCompute,
            [GpuDiagnosticsMetadataKeys.Rev3PromotionGate] = GpuRev3PromotionGates.TraceCandidate,
            [GpuDiagnosticsMetadataKeys.Rev3RequiredStreak] = "1"
        };

        if (metadata is not null)
        {
            foreach (var pair in metadata)
            {
                values[pair.Key] = pair.Value;
            }
        }

        return WebGpuRev3DiagnosticsMetadata.Create(passId, usingCpuFallback: false, values, frame);
    }

    private static WebGpuRev3FrameEnvelope ToFrame(GpuFrameToken frame)
        => new()
        {
            FrameId = frame.FrameId,
            FrameIndex = frame.FrameIndex,
            SampleTicks = frame.SampleTicks
        };

    private static IReadOnlyList<WebGpuRev3TargetEnvelope> TargetsForFrame(GpuFrameToken frame)
    {
        var targets = new List<WebGpuRev3TargetEnvelope> { ToTarget("raw", frame.RawTarget) };
        if (frame.HudTarget is not null)
        {
            targets.Add(ToTarget("hud", frame.HudTarget));
        }

        return targets;
    }

    private static WebGpuRev3TargetEnvelope ToTarget(string role, GpuFrameTarget target)
        => new()
        {
            Role = role,
            TargetId = target.TargetId,
            Kind = target.Kind,
            Backend = target.Backend,
            Width = target.Width,
            Height = target.Height,
            PixelFormat = target.PixelFormat,
            ZeroCopy = target.ZeroCopy
        };

    private static WebGpuRev3BufferEnvelope ToBuffer(string name, GpuFlatBufferLayout layout, IReadOnlyList<float> values)
        => new()
        {
            Name = name,
            LayoutName = layout.Name,
            Stride = layout.Stride,
            Count = values.Count / layout.Stride,
            Values = values.ToArray()
        };

    private static IReadOnlyDictionary<string, float> EgoRadarScalars(GpuHudEgoRadarInput radar)
        => Sort(new Dictionary<string, float>(StringComparer.Ordinal)
        {
            ["audioEnemyConfidence"] = radar.AudioEnemyConfidence,
            ["audioEnemyYawDegrees"] = radar.AudioEnemyYawDegrees ?? float.NaN,
            ["compassConfidence"] = radar.CompassConfidence,
            ["compassHeadingDegrees"] = radar.CompassHeadingDegrees,
            ["compassUsable"] = radar.CompassUsable ? 1.0f : 0.0f,
            ["decay"] = radar.Decay,
            ["flicker"] = radar.Flicker,
            ["fusedEnemyConfidence"] = radar.FusedEnemyConfidence,
            ["fusedEnemyYawDegrees"] = radar.FusedEnemyYawDegrees ?? float.NaN,
            ["hold"] = radar.Hold,
            ["kinesisX"] = radar.Kinesis.X,
            ["kinesisY"] = radar.Kinesis.Y,
            ["radarMode"] = (float)radar.RadarMode,
            ["sdfLink"] = radar.SdfLink ? 1.0f : 0.0f,
            ["visualEnemyConfidence"] = radar.VisualEnemyConfidence,
            ["visualEnemyYawDegrees"] = radar.VisualEnemyYawDegrees ?? float.NaN
        });

    private static void ValidatePassIdentity(
        WebGpuRev3DispatchEnvelope envelope,
        ICollection<GpuValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(envelope.PassId))
        {
            issues.Add(Error(
                "WEBGPU_REV3_ENVELOPE_PASS_ID_REQUIRED",
                "Rev3 dispatch envelope pass id is required.",
                "envelope.passId"));
            return;
        }

        var expectedPassId = envelope.PassKind switch
        {
            WebGpuRev3PassKind.Aisthesis => GpuOperationNames.GpuAisthesisRawFrame,
            WebGpuRev3PassKind.SpatialReasoning => GpuOperationNames.GpuSpatialReasoning,
            WebGpuRev3PassKind.HudComposite => GpuOperationNames.GpuHudComposite,
            _ => ""
        };
        if (string.IsNullOrWhiteSpace(expectedPassId))
        {
            issues.Add(Error(
                "WEBGPU_REV3_ENVELOPE_PASS_KIND_INVALID",
                $"Rev3 dispatch envelope pass kind is invalid: {envelope.PassKind}.",
                "envelope.passKind"));
            return;
        }

        if (!string.Equals(envelope.PassId, expectedPassId, StringComparison.Ordinal))
        {
            issues.Add(Error(
                "WEBGPU_REV3_ENVELOPE_PASS_ID_KIND_MISMATCH",
                $"Rev3 dispatch envelope pass kind {envelope.PassKind} requires pass id '{expectedPassId}', but received '{envelope.PassId}'.",
                "envelope.passId"));
        }

        if (!GpuRev3PathRoles.TryResolveFromPassId(envelope.PassId, out _))
        {
            issues.Add(Error(
                "WEBGPU_REV3_ENVELOPE_PATH_ROLE_UNKNOWN",
                $"Rev3 dispatch envelope pass id '{envelope.PassId}' does not resolve to a canonical path role.",
                "envelope.passId"));
        }
    }

    private static void ValidateFrame(
        WebGpuRev3DispatchEnvelope envelope,
        ICollection<GpuValidationIssue> issues)
    {
        if (envelope.Frame is null)
        {
            issues.Add(Error(
                "WEBGPU_REV3_ENVELOPE_FRAME_REQUIRED",
                "Rev3 dispatch envelope frame identity is required.",
                "envelope.frame"));
            return;
        }

        if (string.IsNullOrWhiteSpace(envelope.Frame.FrameId))
        {
            issues.Add(Error(
                "WEBGPU_REV3_ENVELOPE_FRAME_ID_REQUIRED",
                "Rev3 dispatch envelope frame id is required.",
                "envelope.frame.frameId"));
        }

        if (envelope.Frame.FrameIndex < 0)
        {
            issues.Add(Error(
                "WEBGPU_REV3_ENVELOPE_FRAME_INDEX_INVALID",
                $"Rev3 dispatch envelope frame index must be non-negative, but was {envelope.Frame.FrameIndex}.",
                "envelope.frame.frameIndex"));
        }

        if (envelope.Frame.SampleTicks < 0)
        {
            issues.Add(Error(
                "WEBGPU_REV3_ENVELOPE_SAMPLE_TICKS_INVALID",
                $"Rev3 dispatch envelope sample ticks must be non-negative, but was {envelope.Frame.SampleTicks}.",
                "envelope.frame.sampleTicks"));
        }
    }

    private static void ValidateTargets(
        WebGpuRev3DispatchEnvelope envelope,
        ICollection<GpuValidationIssue> issues)
    {
        var raw = envelope.Targets.FirstOrDefault(static target => string.Equals(target.Role, "raw", StringComparison.Ordinal));
        if (raw is null)
        {
            issues.Add(Error(
                "WEBGPU_REV3_ENVELOPE_RAW_TARGET_REQUIRED",
                "Rev3 dispatch envelope must carry a raw target descriptor.",
                "envelope.targets.raw"));
            return;
        }

        if (raw.Kind != GpuFrameTargetKind.RawFramebuffer)
        {
            issues.Add(Error(
                "WEBGPU_REV3_ENVELOPE_RAW_TARGET_KIND_INVALID",
                $"Rev3 dispatch envelope raw target must be RawFramebuffer, but was {raw.Kind}.",
                "envelope.targets.raw.kind"));
        }

        if (raw.Width <= 0 || raw.Height <= 0)
        {
            issues.Add(Error(
                "WEBGPU_REV3_ENVELOPE_RAW_TARGET_SIZE_INVALID",
                $"Rev3 dispatch envelope raw target size must be positive, but was {raw.Width}x{raw.Height}.",
                "envelope.targets.raw.size"));
        }

        if (string.IsNullOrWhiteSpace(raw.TargetId))
        {
            issues.Add(Error(
                "WEBGPU_REV3_ENVELOPE_RAW_TARGET_ID_REQUIRED",
                "Rev3 dispatch envelope raw target id is required.",
                "envelope.targets.raw.targetId"));
        }
    }

    private static void ValidateBuffers(
        WebGpuRev3DispatchEnvelope envelope,
        ICollection<GpuValidationIssue> issues)
    {
        foreach (var buffer in envelope.Buffers)
        {
            if (string.IsNullOrWhiteSpace(buffer.Name))
            {
                issues.Add(Error(
                    "WEBGPU_REV3_ENVELOPE_BUFFER_NAME_REQUIRED",
                    "Rev3 dispatch envelope buffer name is required.",
                    "envelope.buffers.name"));
            }

            if (buffer.Stride <= 0)
            {
                issues.Add(Error(
                    "WEBGPU_REV3_ENVELOPE_BUFFER_STRIDE_INVALID",
                    $"Rev3 dispatch envelope buffer '{buffer.Name}' has invalid stride {buffer.Stride}.",
                    $"envelope.buffers.{buffer.Name}.stride"));
                continue;
            }

            if (buffer.Values.Count % buffer.Stride != 0)
            {
                issues.Add(Error(
                    "WEBGPU_REV3_ENVELOPE_BUFFER_VALUES_STRIDE_MISMATCH",
                    $"Rev3 dispatch envelope buffer '{buffer.Name}' value count {buffer.Values.Count} is not divisible by stride {buffer.Stride}.",
                    $"envelope.buffers.{buffer.Name}.values"));
            }
        }
    }

    private static void ValidateMetadata(
        WebGpuRev3DispatchEnvelope envelope,
        ICollection<GpuValidationIssue> issues)
    {
        if (envelope.Metadata.Count == 0)
        {
            issues.Add(Error(
                "WEBGPU_REV3_ENVELOPE_METADATA_REQUIRED",
                "Rev3 dispatch envelope must carry canonical diagnostics metadata.",
                "envelope.metadata"));
            return;
        }

        var validation = GpuCanonicalValidation.ValidateRev3DiagnosticsMetadata(envelope.Metadata, "envelope");
        foreach (var issue in validation.Errors)
        {
            issues.Add(issue with
            {
                Code = $"WEBGPU_REV3_ENVELOPE_{issue.Code}",
                Path = issue.Path ?? "envelope.metadata"
            });
        }
    }

    private static IReadOnlyDictionary<string, bool> Sort(IReadOnlyDictionary<string, bool> values)
        => values
            .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(static pair => pair.Key, static pair => pair.Value, StringComparer.Ordinal);

    private static IReadOnlyDictionary<string, float> Sort(IReadOnlyDictionary<string, float> values)
        => values
            .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(static pair => pair.Key, static pair => pair.Value, StringComparer.Ordinal);

    private static IReadOnlyDictionary<string, string> Sort(IReadOnlyDictionary<string, string> values)
        => values
            .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(static pair => pair.Key, static pair => pair.Value, StringComparer.Ordinal);

    private static IReadOnlyDictionary<string, string> SortOptional(IReadOnlyDictionary<string, string?> values)
        => values
            .Where(static pair => !string.IsNullOrWhiteSpace(pair.Value))
            .OrderBy(static pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(static pair => pair.Key, static pair => pair.Value!, StringComparer.Ordinal);

    private static IReadOnlyList<float> Pad(IReadOnlyList<float> values, int length)
    {
        var output = new float[length];
        for (var index = 0; index < Math.Min(values.Count, length); index++)
        {
            output[index] = values[index];
        }

        return output;
    }

    private static void ThrowIfInvalid(GpuValidationResult validation)
    {
        if (validation.IsValid)
        {
            return;
        }

        var message = string.Join(
            "; ",
            validation.Errors.Select(static issue => $"{issue.Code}:{issue.Path}"));
        throw new ArgumentException($"Canonical GPU rev3 envelope validation failed. {message}");
    }

    private static GpuValidationIssue Error(string code, string message, string path)
        => new()
        {
            Code = code,
            Message = message,
            Path = path,
            Severity = GpuValidationSeverity.Error
        };
}
