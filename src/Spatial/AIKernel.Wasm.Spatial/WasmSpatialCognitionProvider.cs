namespace AIKernel.Wasm.Spatial;

using AIKernel.Dtos.Perception;
using AIKernel.Dtos.Providers;
using AIKernel.Enums;
using AIKernel.Wasm.Perception;
using AIKernel.Wasm.Runtime;

/// <summary>
/// [EN] Provides low-level projection math for WASM-owned spatial cognition.
/// [JA] WASM 所有 spatial cognition の low-level projection math を提供します。
/// </summary>
public static class WasmSpatialCognitionKernel
{
    private const double DefaultCorrectionGain = 0.6108652381980153;
    private const double DefaultHudCenterX = 0.5;
    private const double DefaultHudCenterY = 0.45;
    private const double DefaultHudRadius = 0.34;

    /// <summary>
    /// [EN] Projects visual and stereo auditory input into a fused HUD position.
    /// [JA] visual / stereo auditory input を fused HUD position に投影します。
    /// </summary>
    /// <param name="input">[EN] Projection input. [JA] projection input です。</param>
    /// <returns>[EN] Calculated spatial projection. [JA] 計算済み spatial projection を返します。</returns>
    public static WasmSpatialProjection Project(in WasmSpatialProjectionInput input)
    {
        var dx = input.Centroid.X - input.Player.X;
        var dy = input.Centroid.Y - input.Player.Y;
        var hasVisualVector = Math.Abs(dx) > double.Epsilon || Math.Abs(dy) > double.Epsilon;
        var visualDirection = hasVisualVector ? Math.Atan2(dx, dy) : 0;
        var leftEnergy = Clamp01(input.LeftEnergy);
        var rightEnergy = Clamp01(input.RightEnergy);
        var energySum = leftEnergy + rightEnergy;
        var balance = energySum <= double.Epsilon
            ? 0
            : Math.Clamp((rightEnergy - leftEnergy) / energySum, -1, 1);
        var gain = Math.Abs(input.CorrectionGain) <= double.Epsilon
            ? DefaultCorrectionGain
            : input.CorrectionGain;
        var auditoryCorrection = balance * gain;
        var fusedDirection = visualDirection + auditoryCorrection;
        var centerX = Math.Abs(input.HudCenter.X) <= double.Epsilon ? DefaultHudCenterX : input.HudCenter.X;
        var centerY = Math.Abs(input.HudCenter.Y) <= double.Epsilon ? DefaultHudCenterY : input.HudCenter.Y;
        var radius = input.HudRadius > double.Epsilon ? input.HudRadius : DefaultHudRadius;
        var hudX = Clamp01(centerX + (Math.Cos(fusedDirection) * radius));
        var hudY = Clamp01(centerY + (Math.Sin(fusedDirection) * radius));
        var confidence = Clamp01((hasVisualVector ? 0.6 : 0.2) + (energySum > double.Epsilon ? 0.35 : 0));

        return new WasmSpatialProjection
        {
            VisualDirection = visualDirection,
            AuditoryCorrection = auditoryCorrection,
            FusedDirection = fusedDirection,
            HudX = hudX,
            HudY = hudY,
            Confidence = confidence
        };
    }

    private static double Clamp01(double value)
        => Math.Clamp(value, 0, 1);
}

/// <summary>
/// [EN] Composes frame and auditory perception into scenario-independent spatial cognition signals.
/// [JA] frame / auditory perception を scenario 非依存の spatial cognition signal に合成します。
/// </summary>
public sealed class WasmSpatialCognitionProvider : WasmKernelProviderBase, IWasmSpatialCognitionProvider
{
    /// <summary>
    /// [EN] Initializes a WASM spatial cognition provider.
    /// [JA] WASM spatial cognition Provider を初期化します。
    /// </summary>
    public WasmSpatialCognitionProvider()
        : base(
            "wasm.spatial-cognition",
            "WASM Spatial Cognition Provider",
            ["wasm.spatial.compose"],
            ["frame", "audio", "spatial-snapshot"],
            Capabilities())
    {
    }

    /// <summary>
    /// [EN] Builds a deterministic spatial cognition snapshot.
    /// [JA] deterministic な spatial cognition snapshot を構築します。
    /// </summary>
    /// <param name="request">[EN] Spatial cognition request. [JA] spatial cognition request です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視するトークンです。</param>
    /// <returns>[EN] Spatial cognition snapshot. [JA] spatial cognition snapshot を返します。</returns>
    public ValueTask<WasmSpatialCognitionSnapshot> BuildSnapshotAsync(
        WasmSpatialCognitionRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        var signals = new List<WasmSpatialSignal>();
        var visualPerceptions = CopySortedVisual(request.VisualPerceptions);
        for (var index = 0; index < visualPerceptions.Count; index++)
        {
            var perception = visualPerceptions[index];
            signals.Add(CreateSignal(
                $"visual.{perception.ObservationId}",
                "spatial.visual.presence",
                perception.Signals.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["frameId"] = perception.FrameId,
                    ["frameIndex"] = perception.FrameIndex.ToString(System.Globalization.CultureInfo.InvariantCulture)
                }));
        }

        var auditoryPerceptions = CopySortedAuditory(request.AuditoryPerceptions);
        double leftTotal = 0;
        double rightTotal = 0;
        var energyCount = 0;
        for (var index = 0; index < auditoryPerceptions.Count; index++)
        {
            var auditory = auditoryPerceptions[index];
            signals.Add(CreateSignal(
                $"auditory.{auditory.ObservationId}",
                "spatial.auditory.presence",
                auditory.Signals.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                auditory.Metadata));
            if (TryMetadataDouble(auditory.Metadata, "leftEnergy", out var left)
                && TryMetadataDouble(auditory.Metadata, "rightEnergy", out var right))
            {
                leftTotal += Math.Clamp(left, 0, 1);
                rightTotal += Math.Clamp(right, 0, 1);
                energyCount++;
            }
        }

        var projectionInput = MergeAuditoryEnergy(request.ProjectionInput, leftTotal, rightTotal, energyCount);
        var projection = WasmSpatialCognitionKernel.Project(in projectionInput);
        var sensorInputs = CopySortedSensors(request.SensorInputs);
        var retryIntent = ResolveRetryIntent(sensorInputs);
        signals.Add(CreateSignal(
            "spatial.visual_direction",
            "spatial.direction.visual",
            projection.VisualDirection.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["unit"] = "radians"
            }));
        signals.Add(CreateSignal(
            "spatial.auditory_correction",
            "spatial.correction.auditory",
            projection.AuditoryCorrection.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["unit"] = "radians"
            }));
        signals.Add(CreateSignal(
            "spatial.fused_direction",
            "spatial.direction.fused",
            projection.FusedDirection.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["unit"] = "radians"
            },
            projection.Confidence));
        signals.Add(CreateSignal(
            "spatial.hud_position",
            "spatial.hud.position",
            $"{projection.HudX:0.###},{projection.HudY:0.###}",
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["unit"] = "normalized"
            },
            projection.Confidence));

        var metadata = new Dictionary<string, string>(request.Metadata, StringComparer.Ordinal)
        {
            ["visualCount"] = request.VisualPerceptions.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["auditoryCount"] = request.AuditoryPerceptions.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["visualDirection"] = projection.VisualDirection.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
            ["auditoryCorrection"] = projection.AuditoryCorrection.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
            ["fusedDirection"] = projection.FusedDirection.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
            ["hudX"] = projection.HudX.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
            ["hudY"] = projection.HudY.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
            ["confidence"] = projection.Confidence.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
            ["sensorCount"] = sensorInputs.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["retryRequested"] = (retryIntent?.Requested == true).ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
        var hasPerception = request.VisualPerceptions.Count > 0 || request.AuditoryPerceptions.Count > 0;

        return ValueTask.FromResult(new WasmSpatialCognitionSnapshot
        {
            SnapshotId = string.IsNullOrWhiteSpace(request.RequestId)
                ? "wasm.spatial.snapshot"
                : request.RequestId,
            Succeeded = hasPerception,
            Signals = signals,
            SensorInputs = sensorInputs,
            RetryIntent = retryIntent,
            ErrorCode = hasPerception ? null : "WASM_SPATIAL_NO_PERCEPTION",
            ErrorMessage = hasPerception ? null : "Spatial cognition requires at least one perception result.",
            Diagnostics = hasPerception ? [] : ["WASM_SPATIAL_NO_PERCEPTION"],
            Metadata = metadata
        });
    }

    private static WasmSpatialSignal CreateSignal(
        string id,
        string kind,
        string value,
        IReadOnlyDictionary<string, string> metadata,
        double confidence = 1)
        => new()
        {
            SignalId = id,
            Kind = kind,
            Value = value,
            Confidence = confidence,
            Metadata = new Dictionary<string, string>(metadata, StringComparer.Ordinal)
        };

    private static List<FramePerceptionResult> CopySortedVisual(IReadOnlyList<FramePerceptionResult> source)
    {
        var items = new List<FramePerceptionResult>(source.Count);
        for (var index = 0; index < source.Count; index++)
        {
            items.Add(source[index]);
        }

        items.Sort(static (left, right) => string.Compare(left.ObservationId, right.ObservationId, StringComparison.Ordinal));
        return items;
    }

    private static IReadOnlyDictionary<string, WasmSensorStateDescriptor> CopySortedSensors(
        IReadOnlyDictionary<string, WasmSensorStateDescriptor> source)
    {
        var copy = new SortedDictionary<string, WasmSensorStateDescriptor>(StringComparer.Ordinal);
        foreach (var item in source.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            var name = NormalizeSensorName(item.Key);
            var sensor = item.Value;
            copy[name] = sensor with
            {
                Name = name,
                ConceptName = string.IsNullOrWhiteSpace(sensor.ConceptName) ? ConceptName(name) : sensor.ConceptName,
                EnglishName = string.IsNullOrWhiteSpace(sensor.EnglishName) ? name : sensor.EnglishName,
                Category = string.IsNullOrWhiteSpace(sensor.Category)
                    ? (name is "movement" or "spatial" ? "derived" : "primary")
                    : sensor.Category,
                Metadata = new Dictionary<string, string>(sensor.Metadata, StringComparer.Ordinal)
            };
        }

        return new Dictionary<string, WasmSensorStateDescriptor>(copy, StringComparer.Ordinal);
    }

    private static WasmRetryIntentCarrier? ResolveRetryIntent(
        IReadOnlyDictionary<string, WasmSensorStateDescriptor> sensorInputs)
    {
        if (!sensorInputs.TryGetValue("health", out var health) || !health.Enabled)
        {
            return null;
        }

        var likelyDead = TryMetadataBool(health.Metadata, "likelyDead", out var parsedLikelyDead) && parsedLikelyDead;
        var zeroScore = TryMetadataDouble(health.Metadata, "zeroScore", out var parsedZeroScore)
            ? Math.Clamp(parsedZeroScore, 0, 1)
            : 0;
        if (!likelyDead && zeroScore < 0.78)
        {
            return null;
        }

        return new WasmRetryIntentCarrier
        {
            Requested = true,
            ReasonCode = "health-death",
            Priority = 100,
            Confidence = Math.Round(Math.Max(zeroScore, Math.Clamp(health.Confidence ?? 0, 0, 1)), 2, MidpointRounding.AwayFromZero),
            SourceSensor = "health",
            Metadata = new Dictionary<string, string>(health.Metadata, StringComparer.Ordinal)
        };
    }

    private static string NormalizeSensorName(string name)
    {
        var normalized = StringComparer.OrdinalIgnoreCase.Equals(name, "vision")
            ? "visual"
            : StringComparer.OrdinalIgnoreCase.Equals(name, "auditory")
                ? "audio"
                : StringComparer.OrdinalIgnoreCase.Equals(name, "heading")
                    || StringComparer.OrdinalIgnoreCase.Equals(name, "bearing")
                        ? "compass"
                        : name;
        return normalized.Trim().ToLowerInvariant();
    }

    private static string ConceptName(string name)
        => name switch
        {
            "visual" or "audio" => "Aisthesis",
            "motor" or "movement" => "Kinesis",
            "compass" or "spatial" => "Phantasia",
            "health" => "Aisthesis",
            _ => string.Empty
        };

    private static List<WasmAuditoryPerceptionResult> CopySortedAuditory(IReadOnlyList<WasmAuditoryPerceptionResult> source)
    {
        var items = new List<WasmAuditoryPerceptionResult>(source.Count);
        for (var index = 0; index < source.Count; index++)
        {
            items.Add(source[index]);
        }

        items.Sort(static (left, right) => string.Compare(left.ObservationId, right.ObservationId, StringComparison.Ordinal));
        return items;
    }

    private static WasmSpatialProjectionInput MergeAuditoryEnergy(
        WasmSpatialProjectionInput input,
        double leftTotal,
        double rightTotal,
        int energyCount)
    {
        if (energyCount <= 0 || Math.Abs(input.LeftEnergy) > double.Epsilon || Math.Abs(input.RightEnergy) > double.Epsilon)
        {
            return input;
        }

        return input with
        {
            LeftEnergy = leftTotal / energyCount,
            RightEnergy = rightTotal / energyCount
        };
    }

    private static bool TryMetadataDouble(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        out double value)
    {
        value = 0;
        return metadata.TryGetValue(key, out var text)
            && double.TryParse(
                text,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out value);
    }

    private static bool TryMetadataBool(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        out bool value)
    {
        value = false;
        return metadata.TryGetValue(key, out var text)
            && bool.TryParse(text, out value);
    }

    private static IReadOnlyList<ProviderCapability> Capabilities()
        =>
        [
            new()
            {
                LogicalCapabilityId = "wasm.spatial-cognition",
                ProviderCapabilityId = "wasm.spatial.compose",
                Flags = ProviderCapabilityFlags.Observation,
                Kind = ProviderKind.Observer,
                InputModalities = InputModalities.Frame | InputModalities.Audio,
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
