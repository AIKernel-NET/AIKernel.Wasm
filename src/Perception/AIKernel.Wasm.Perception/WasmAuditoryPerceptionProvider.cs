namespace AIKernel.Wasm.Perception;

using AIKernel.Dtos.Providers;
using AIKernel.Enums;
using AIKernel.Wasm.Runtime;

/// <summary>
/// [EN] Extracts provider-neutral auditory signals from WebAudio PCM frames inside the WASM boundary.
/// [JA] WASM 境界内で WebAudio PCM frame から provider-neutral な auditory signal を抽出します。
/// </summary>
public sealed class WasmAuditoryPerceptionProvider : WasmKernelProviderBase, IWasmAuditoryPerceptionProvider
{
    /// <summary>
    /// [EN] Initializes a WASM auditory perception provider.
    /// [JA] WASM auditory perception Provider を初期化します。
    /// </summary>
    public WasmAuditoryPerceptionProvider()
        : base(
            "wasm.auditory-perception",
            "WASM Auditory Perception Provider",
            ["wasm.perception.audio.analyze"],
            ["audio", "pcm"],
            Capabilities())
    {
    }

    /// <summary>
    /// [EN] Analyzes one WebAudio PCM capture frame without backend-specific audio types.
    /// [JA] backend-specific audio 型を使わず 1 つの WebAudio PCM capture frame を解析します。
    /// </summary>
    /// <param name="request">[EN] Auditory perception request. [JA] auditory perception request です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視するトークンです。</param>
    /// <returns>[EN] Auditory perception result. [JA] auditory perception result を返します。</returns>
    public ValueTask<WasmAuditoryPerceptionResult> AnalyzeAsync(
        WasmAuditoryPerceptionRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        var buffer = request.Frame.Buffer;
        var energy = AnalyzeStereoEnergy(buffer);
        var metadata = new Dictionary<string, string>(request.Metadata, StringComparer.Ordinal)
        {
            ["sampleRate"] = buffer.SampleRate.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["channels"] = buffer.Channels.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["sampleFormat"] = buffer.SampleFormat,
            ["byteLength"] = buffer.Payload.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["frameIndex"] = request.Frame.Timing.FrameIndex.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["leftEnergy"] = energy.LeftEnergy.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
            ["rightEnergy"] = energy.RightEnergy.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
            ["balance"] = energy.Balance.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
            ["dominantFreq"] = energy.DominantFrequency.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture),
            ["eventDetected"] = energy.EventDetected ? "true" : "false",
            ["eventType"] = energy.EventType,
            ["timestamp"] = request.Frame.Timing.Timestamp.ToString("O", System.Globalization.CultureInfo.InvariantCulture)
        };

        return ValueTask.FromResult(new WasmAuditoryPerceptionResult
        {
            Succeeded = buffer.Payload.Count > 0,
            ObservationId = string.IsNullOrWhiteSpace(request.ObservationId)
                ? $"audio.{request.Frame.Timing.FrameIndex}"
                : request.ObservationId,
            Signals =
            [
                CreateSignal("audio.sample_rate", "audio.sample_rate", buffer.SampleRate.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                CreateSignal("audio.channels", "audio.channels", buffer.Channels.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                CreateSignal("audio.sample_format", "audio.sample_format", buffer.SampleFormat),
                CreateSignal("audio.byte_length", "audio.byte_length", buffer.Payload.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                CreateSignal("audio.left_energy", "audio.energy.left", energy.LeftEnergy.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)),
                CreateSignal("audio.right_energy", "audio.energy.right", energy.RightEnergy.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)),
                CreateSignal("audio.balance", "audio.stereo.balance", energy.Balance.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)),
                CreateSignal("audio.dominant_freq", "audio.frequency.dominant", energy.DominantFrequency.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)),
                CreateSignal("audio.event_detected", "audio.event.detected", energy.EventDetected ? "true" : "false")
            ],
            ErrorCode = buffer.Payload.Count == 0 ? "WASM_AUDIO_EMPTY_PAYLOAD" : null,
            ErrorMessage = buffer.Payload.Count == 0 ? "Audio payload is empty." : null,
            Diagnostics = buffer.Payload.Count == 0 ? ["WASM_AUDIO_EMPTY_PAYLOAD"] : [],
            Metadata = metadata
        });
    }

    private static WasmAuditorySignal CreateSignal(string id, string kind, string value)
        => new()
        {
            SignalId = id,
            Kind = kind,
            Value = value,
            Confidence = 1
        };

    private static AuditoryEnergy AnalyzeStereoEnergy(AIKernel.Wasm.Audio.WebAudioPcmBuffer buffer)
    {
        if (buffer.Payload.Count == 0)
        {
            return new AuditoryEnergy(0, 0, 0, 0, false, "none");
        }

        var channels = Math.Max(1, buffer.Channels);
        var format = buffer.SampleFormat ?? string.Empty;
        var isFloat32 = format.Equals("f32", StringComparison.OrdinalIgnoreCase) && buffer.Payload.Count >= 4;
        var sampleCount = isFloat32 ? buffer.Payload.Count / 4 : buffer.Payload.Count;
        if (sampleCount == 0)
        {
            return new AuditoryEnergy(0, 0, 0, 0, false, "none");
        }

        double leftTotal = 0;
        double rightTotal = 0;
        var leftCount = 0;
        var rightCount = 0;
        for (var index = 0; index < sampleCount; index++)
        {
            var value = isFloat32
                ? Clamp01(Math.Abs(ReadSingle(buffer.Payload, index * 4)))
                : Clamp01(Math.Abs((buffer.Payload[index] - 128) / 128.0));
            if ((index % channels) == 0)
            {
                leftTotal += value;
                leftCount++;
            }
            else if ((index % channels) == 1)
            {
                rightTotal += value;
                rightCount++;
            }
        }

        var left = leftCount == 0 ? 0 : Clamp01(leftTotal / leftCount);
        var right = rightCount == 0 ? left : Clamp01(rightTotal / rightCount);
        var sum = left + right;
        var balance = sum <= 0 ? 0 : Math.Clamp((right - left) / sum, -1, 1);
        var energy = Math.Max(left, right);
        var dominant = energy <= 0 ? 0 : Math.Round(110 + (energy * 1760), 3);
        var detected = energy >= 0.05;
        return new AuditoryEnergy(left, right, balance, dominant, detected, detected ? "spatial-event" : "none");
    }

    private static float ReadSingle(IReadOnlyList<byte> payload, int offset)
    {
        if (payload is byte[] bytes)
        {
            return BitConverter.ToSingle(bytes, offset);
        }

        var bits = payload[offset]
            | (payload[offset + 1] << 8)
            | (payload[offset + 2] << 16)
            | (payload[offset + 3] << 24);
        return BitConverter.Int32BitsToSingle(bits);
    }

    private static double Clamp01(double value)
        => Math.Clamp(value, 0, 1);

    private readonly record struct AuditoryEnergy(
        double LeftEnergy,
        double RightEnergy,
        double Balance,
        double DominantFrequency,
        bool EventDetected,
        string EventType);

    private static IReadOnlyList<ProviderCapability> Capabilities()
        =>
        [
            new()
            {
                LogicalCapabilityId = "wasm.perception.auditory",
                ProviderCapabilityId = "wasm.perception.audio.analyze",
                Flags = ProviderCapabilityFlags.Observation,
                Kind = ProviderKind.Observer,
                InputModalities = InputModalities.Audio,
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
