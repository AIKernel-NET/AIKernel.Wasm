namespace AIKernel.Wasm.Perception;

using AIKernel.Common.Results;
using System.Globalization;

/// <summary>
/// [EN] Provides WASM-local Phainesis and Nous synthetic sensor analysis.
/// [JA] WASM local な Phainesis / Nous 合成 sensor 解析を提供します。
/// </summary>
public sealed class WasmSyntheticSensorProvider : IWasmSyntheticSensorProvider, IWasmSyntheticSensorPipeline
{
    private readonly IWasmResidentPerceptionAlgorithmLibrary _residentAlgorithms;
    private readonly IWasmSyntheticSensorKernelPlanner _kernelPlanner;

    /// <summary>
    /// [EN] Initializes a synthetic sensor provider.
    /// [JA] 合成 sensor provider を初期化します。
    /// </summary>
    public WasmSyntheticSensorProvider()
        : this(new WasmResidentPerceptionAlgorithmLibrary(), new WasmSyntheticSensorKernelPlanner())
    {
    }

    /// <summary>
    /// [EN] Initializes a synthetic sensor provider with a resident algorithm library.
    /// [JA] resident algorithm library を指定して合成 sensor provider を初期化します。
    /// </summary>
    /// <param name="residentAlgorithms">[EN] Resident algorithm library. [JA] resident algorithm library です。</param>
    public WasmSyntheticSensorProvider(IWasmResidentPerceptionAlgorithmLibrary residentAlgorithms)
        : this(residentAlgorithms, new WasmSyntheticSensorKernelPlanner())
    {
    }

    /// <summary>
    /// [EN] Initializes a synthetic sensor provider with resident algorithms and kernel planning.
    /// [JA] resident algorithm と kernel planning を指定して合成 sensor provider を初期化します。
    /// </summary>
    /// <param name="residentAlgorithms">[EN] Resident algorithm library. [JA] resident algorithm library です。</param>
    /// <param name="kernelPlanner">[EN] Synthetic sensor kernel planner. [JA] 合成 sensor kernel planner です。</param>
    public WasmSyntheticSensorProvider(
        IWasmResidentPerceptionAlgorithmLibrary residentAlgorithms,
        IWasmSyntheticSensorKernelPlanner kernelPlanner)
    {
        ArgumentNullException.ThrowIfNull(residentAlgorithms);
        ArgumentNullException.ThrowIfNull(kernelPlanner);
        _residentAlgorithms = residentAlgorithms;
        _kernelPlanner = kernelPlanner;
    }

    /// <summary>
    /// [EN] Analyzes resident buffers into Phainesis phenomena and Nous vectors.
    /// [JA] resident buffer を Phainesis 現象と Nous ベクトルへ解析します。
    /// </summary>
    /// <param name="request">[EN] Synthetic sensor request. [JA] 合成 sensor request です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視する token です。</param>
    /// <returns>[EN] Synthetic sensor snapshot. [JA] 合成 sensor snapshot を返します。</returns>
    public async ValueTask<WasmSyntheticSensorSnapshot> AnalyzeAsync(
        WasmSyntheticSensorRequest request,
        CancellationToken cancellationToken)
    {
        var result = await TryAnalyzeAsync(request, cancellationToken).ConfigureAwait(false);

        return result.Match(
            error => FailureSnapshot(request, error),
            snapshot => snapshot);
    }

    /// <summary>
    /// [EN] Safely analyzes resident buffers as a Result for LINQ pipeline composition.
    /// [JA] LINQ pipeline 合成用に resident buffer を Result として安全に解析します。
    /// </summary>
    /// <param name="request">[EN] Synthetic sensor request. [JA] 合成 sensor request です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視する token です。</param>
    /// <returns>[EN] Result-wrapped synthetic sensor snapshot. [JA] Result で包まれた合成 sensor snapshot を返します。</returns>
    public Task<Result<WasmSyntheticSensorSnapshot>> TryAnalyzeAsync(
        WasmSyntheticSensorRequest request,
        CancellationToken cancellationToken)
        =>
            from validRequest in ValidateRequest(request, cancellationToken).AsTask()
            from flow in EstimateFlowOptionAsync(validRequest, cancellationToken)
            select BuildSnapshot(validRequest, flow);

    private WasmSyntheticSensorSnapshot BuildSnapshot(
        WasmSyntheticSensorRequest request,
        Option<(WasmDenseOpticalFlowResult Result, WasmResidentKernelDescriptor Kernel)> flow)
    {
        var phenomena = new List<WasmPhenomenonSignal>(8);
        var vectors = new List<WasmMeaningVector>(8);
        var metadata = new Dictionary<string, string>(request.Metadata, StringComparer.Ordinal);

        var navigable = AnalyzeMask(request.NavigableMask);
        if (navigable.Valid)
        {
            var gap = Phenomenon(
                "gap",
                "Logos",
                navigable.Score >= 0.18,
                navigable.Score,
                navigable.DirectionX,
                1,
                navigable.Confidence,
                ("source", "navigable-mask"));
            phenomena.Add(gap);
            vectors.Add(Vector("gapVector", "Logos", gap.DirectionX, gap.DirectionY, gap.Score, gap.Confidence));

            var corridorScore = CorridorScore(request.NavigableMask);
            var corridor = Phenomenon(
                "corridorFlow",
                "Logos",
                corridorScore >= 0.22,
                corridorScore,
                navigable.DirectionX * 0.62,
                1,
                Math.Max(navigable.Confidence, corridorScore),
                ("source", "navigable-mask"));
            phenomena.Add(corridor);
            vectors.Add(Vector("corridorVector", "Logos", corridor.DirectionX, corridor.DirectionY, corridor.Score, corridor.Confidence));
        }

        var threat = AnalyzeMask(request.ThreatMask);
        if (threat.Valid)
        {
            var threatField = Phenomenon(
                "threatField",
                "Pathos",
                threat.Score >= 0.24,
                threat.Score,
                -threat.DirectionX,
                -Math.Max(0.24, threat.DirectionY),
                threat.Confidence,
                ("source", "threat-mask"));
            phenomena.Add(threatField);
            vectors.Add(Vector("threatVector", "Pathos", threatField.DirectionX, threatField.DirectionY, threatField.Score, threatField.Confidence));
        }

        flow.Tap(estimate =>
        {
            var wallFlowScore = Clamp01(Math.Abs(estimate.Result.AverageX) + Math.Abs(estimate.Result.AverageY));
            var wallFlow = Phenomenon(
                "wallFlow",
                "Logos",
                wallFlowScore >= 0.08,
                wallFlowScore,
                ClampSigned(estimate.Result.AverageX),
                ClampSigned(estimate.Result.AverageY),
                wallFlowScore,
                ("source", "dense-optical-flow"));
            phenomena.Add(wallFlow);
            vectors.Add(Vector("wallFlowVector", "Logos", wallFlow.DirectionX, wallFlow.DirectionY, wallFlow.Score, wallFlow.Confidence));
        });

        var stuckScore = Clamp01(Math.Max(
            Scalar(request, "motionStallScore"),
            Math.Max(Scalar(request, "stuck"), Scalar(request, "quantizedStall"))));
        if (stuckScore > 0)
        {
            var stuck = Phenomenon(
                "stuck",
                "Logos",
                stuckScore >= 0.55,
                stuckScore,
                -Math.Sign(Scalar(request, "motorX")),
                -1,
                stuckScore,
                ("source", "sensor-scalars"));
            phenomena.Add(stuck);
            vectors.Add(Vector("stuckVector", "Logos", stuck.DirectionX, stuck.DirectionY, stuck.Score, stuck.Confidence));
        }

        var confidence = ConfidenceFusion(request, phenomena);
        var confidenceFusion = Phenomenon(
            "confidenceFusion",
            "Meta",
            confidence >= 0.34,
            confidence,
            0,
            0,
            confidence,
            ("source", "sensor-scalars"));
        phenomena.Add(confidenceFusion);
        vectors.Add(Vector("confidenceVector", "Meta", 0, 0, confidenceFusion.Score, confidenceFusion.Confidence));

        metadata["phenomenaCount"] = phenomena.Count.ToString(CultureInfo.InvariantCulture);
        metadata["vectorsCount"] = vectors.Count.ToString(CultureInfo.InvariantCulture);
        metadata["confidence"] = Round2(confidence).ToString("0.##", CultureInfo.InvariantCulture);

        return new WasmSyntheticSensorSnapshot
        {
            SnapshotId = string.IsNullOrWhiteSpace(request.RequestId)
                ? "wasm.synthetic.sensor.snapshot"
                : request.RequestId,
            Succeeded = true,
            Phenomena = phenomena,
            Vectors = vectors,
            FlowKernel = flow.Match(
                () => new WasmResidentKernelDescriptor(),
                estimate => estimate.Kernel),
            SyntheticKernel = _kernelPlanner.CreateKernelDescriptor(request, phenomena.Count, vectors.Count),
            Metadata = metadata
        };
    }

    private static Result<WasmSyntheticSensorRequest> ValidateRequest(
        WasmSyntheticSensorRequest? request,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Result<WasmSyntheticSensorRequest>.Fail(
                new ErrorContext(
                    "Synthetic sensor analysis was cancelled.",
                    "WASM_SYNTHETIC_SENSOR_CANCELLED",
                    false));
        }

        return MonadicDecision.Optional(request)
            .AsResult("Synthetic sensor request is required. ErrorCode=WASM_SYNTHETIC_SENSOR_REQUEST_REQUIRED")
            .Bind(ValidateHasInput);
    }

    private static Result<WasmSyntheticSensorRequest> ValidateHasInput(WasmSyntheticSensorRequest request)
        => HasInput(request)
            ? Result<WasmSyntheticSensorRequest>.Success(request)
            : Result<WasmSyntheticSensorRequest>.Fail(
                new ErrorContext(
                    "Synthetic sensor analysis requires at least one resident buffer or scalar.",
                    "WASM_SYNTHETIC_SENSOR_INPUT_REQUIRED",
                    false));

    private static bool HasInput(WasmSyntheticSensorRequest request)
        => request.CurrentFrame.Values.Count > 0
            || request.NavigableMask.Values.Count > 0
            || request.ThreatMask.Values.Count > 0
            || request.SensorScalars.Count > 0;

    private Task<Result<Option<(WasmDenseOpticalFlowResult Result, WasmResidentKernelDescriptor Kernel)>>> EstimateFlowOptionAsync(
        WasmSyntheticSensorRequest request,
        CancellationToken cancellationToken)
    {
        if (request.CurrentFrame.Values.Count == 0
            || request.PreviousFrame.Values.Count == 0
            || request.CurrentFrame.Width <= 1
            || request.CurrentFrame.Height <= 1)
        {
            return Result<Option<(WasmDenseOpticalFlowResult Result, WasmResidentKernelDescriptor Kernel)>>
                .Success(Option<(WasmDenseOpticalFlowResult Result, WasmResidentKernelDescriptor Kernel)>.None())
                .AsTask();
        }

        return Try.RunAsync(async () =>
        {
            var result = await _residentAlgorithms.EstimateDenseOpticalFlowAsync(
                new WasmDenseOpticalFlowRequest
                {
                    Current = request.CurrentFrame,
                    Previous = request.PreviousFrame,
                    SearchRadius = 1
                },
                cancellationToken).ConfigureAwait(false);

            return result.Succeeded
                ? Option<(WasmDenseOpticalFlowResult Result, WasmResidentKernelDescriptor Kernel)>.Some((result, result.Kernel))
                : Option<(WasmDenseOpticalFlowResult Result, WasmResidentKernelDescriptor Kernel)>.None();
        });
    }

    private WasmSyntheticSensorSnapshot FailureSnapshot(
        WasmSyntheticSensorRequest? request,
        ErrorContext error)
    {
        var fallbackRequest = request ?? new WasmSyntheticSensorRequest();
        var metadata = request is null
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            : new Dictionary<string, string>(request.Metadata, StringComparer.Ordinal);
        metadata["failureCode"] = error.Code;

        return new WasmSyntheticSensorSnapshot
        {
            SnapshotId = string.IsNullOrWhiteSpace(fallbackRequest.RequestId)
                ? "wasm.synthetic.sensor.snapshot"
                : fallbackRequest.RequestId,
            Succeeded = false,
            FlowKernel = new WasmResidentKernelDescriptor(),
            SyntheticKernel = _kernelPlanner.CreateKernelDescriptor(fallbackRequest, 0, 0),
            ErrorCode = error.Code,
            ErrorMessage = error.Message,
            Metadata = metadata
        };
    }

    private static (bool Valid, double Score, double DirectionX, double DirectionY, double Confidence) AnalyzeMask(WasmScalarBuffer buffer)
    {
        if (buffer.Width <= 0 || buffer.Height <= 0 || buffer.Values.Count < buffer.Width * buffer.Height)
        {
            return (false, 0, 0, 0, 0);
        }

        var total = 0.0;
        var weightedX = 0.0;
        var weightedY = 0.0;
        var max = 0.0;
        for (var y = 0; y < buffer.Height; y++)
        {
            for (var x = 0; x < buffer.Width; x++)
            {
                var value = Clamp01(buffer.Values[(y * buffer.Width) + x]);
                total += value;
                max = Math.Max(max, value);
                weightedX += value * (((x + 0.5) / buffer.Width) - 0.5) * 2;
                weightedY += value * (1 - ((y + 0.5) / buffer.Height));
            }
        }

        var count = buffer.Width * buffer.Height;
        if (total <= double.Epsilon)
        {
            return (true, 0, 0, 0, 0);
        }

        var score = Clamp01(total / count);
        return (
            true,
            Round2(Math.Max(score, max * 0.42)),
            Round2(ClampSigned(weightedX / total)),
            Round2(ClampSigned(weightedY / total)),
            Round2(Math.Max(max, score)));
    }

    private static double CorridorScore(WasmScalarBuffer buffer)
    {
        if (buffer.Width <= 0 || buffer.Height <= 0 || buffer.Values.Count < buffer.Width * buffer.Height)
        {
            return 0;
        }

        var centerStart = buffer.Width / 3;
        var centerEnd = Math.Max(centerStart + 1, buffer.Width - centerStart);
        var total = 0.0;
        var count = 0;
        for (var y = 0; y < buffer.Height; y++)
        {
            var rowWeight = 1 + ((buffer.Height - 1 - y) / Math.Max(1.0, buffer.Height - 1));
            for (var x = centerStart; x < centerEnd; x++)
            {
                total += Clamp01(buffer.Values[(y * buffer.Width) + x]) * rowWeight;
                count++;
            }
        }

        return count == 0 ? 0 : Round2(Clamp01(total / (count * 2)));
    }

    private static double ConfidenceFusion(WasmSyntheticSensorRequest request, IReadOnlyList<WasmPhenomenonSignal> phenomena)
    {
        var total = 0.0;
        var count = 0;
        foreach (var item in request.SensorScalars)
        {
            if (item.Key.EndsWith("confidence", StringComparison.OrdinalIgnoreCase)
                || item.Key.Contains("reliability", StringComparison.OrdinalIgnoreCase))
            {
                total += Clamp01(item.Value);
                count++;
            }
        }

        for (var index = 0; index < phenomena.Count; index++)
        {
            total += Clamp01(phenomena[index].Confidence);
            count++;
        }

        return count == 0 ? 0 : Round2(total / count);
    }

    private static double Scalar(WasmSyntheticSensorRequest request, string key)
        => request.SensorScalars.TryGetValue(key, out var value) ? Clamp01(value) : 0;

    private static WasmPhenomenonSignal Phenomenon(
        string name,
        string axis,
        bool active,
        double score,
        double directionX,
        double directionY,
        double confidence,
        params (string Key, string Value)[] metadata)
        => new()
        {
            Name = name,
            Axis = axis,
            Active = active,
            Score = Round2(Clamp01(score)),
            DirectionX = Round2(ClampSigned(directionX)),
            DirectionY = Round2(ClampSigned(directionY)),
            Confidence = Round2(Clamp01(confidence)),
            Metadata = Metadata(metadata)
        };

    private static WasmMeaningVector Vector(
        string name,
        string axis,
        double x,
        double y,
        double strength,
        double confidence)
        => new()
        {
            Name = name,
            Axis = axis,
            X = Round2(ClampSigned(x)),
            Y = Round2(ClampSigned(y)),
            Strength = Round2(Clamp01(strength)),
            Confidence = Round2(Clamp01(confidence))
        };

    private static IReadOnlyDictionary<string, string> Metadata(params (string Key, string Value)[] values)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var index = 0; index < values.Length; index++)
        {
            metadata[values[index].Key] = values[index].Value;
        }

        return metadata;
    }

    private static double Clamp01(double value)
        => Math.Clamp(value, 0, 1);

    private static double ClampSigned(double value)
        => Math.Clamp(value, -1, 1);

    private static double Round2(double value)
        => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
