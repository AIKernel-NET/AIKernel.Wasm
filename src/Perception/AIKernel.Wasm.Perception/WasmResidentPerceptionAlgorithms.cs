namespace AIKernel.Wasm.Perception;

using System.Globalization;

/// <summary>
/// [EN] Provides WASM-local resident perception algorithms with WebGPU dispatch descriptors and CPU fallbacks.
/// [JA] WebGPU dispatch descriptor と CPU fallback を持つ WASM local resident perception algorithm を提供します。
/// </summary>
public interface IWasmResidentPerceptionAlgorithmLibrary
{
    /// <summary>
    /// [EN] Creates a HSV threshold mask from RGB input.
    /// [JA] RGB input から HSV threshold mask を作成します。
    /// </summary>
    /// <param name="request">[EN] HSV mask request. [JA] HSV mask request です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視するトークンです。</param>
    /// <returns>[EN] HSV mask result. [JA] HSV mask result を返します。</returns>
    ValueTask<WasmHsvThresholdMaskResult> CreateHsvThresholdMaskAsync(
        WasmHsvThresholdMaskRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// [EN] Performs max-pooling downsampling.
    /// [JA] max-pooling downsampling を実行します。
    /// </summary>
    /// <param name="request">[EN] Max pooling request. [JA] max pooling request です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視するトークンです。</param>
    /// <returns>[EN] Max pooling result. [JA] max pooling result を返します。</returns>
    ValueTask<WasmScalarBufferResult> MaxPoolAsync(
        WasmMaxPoolingRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// [EN] Applies morphology dilation or erosion.
    /// [JA] morphology dilation または erosion を適用します。
    /// </summary>
    /// <param name="request">[EN] Morphology request. [JA] morphology request です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視するトークンです。</param>
    /// <returns>[EN] Morphology result. [JA] morphology result を返します。</returns>
    ValueTask<WasmScalarBufferResult> ApplyMorphologyAsync(
        WasmMorphologyRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// [EN] Estimates dense optical flow.
    /// [JA] dense optical flow を推定します。
    /// </summary>
    /// <param name="request">[EN] Optical flow request. [JA] optical flow request です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視するトークンです。</param>
    /// <returns>[EN] Optical flow result. [JA] optical flow result を返します。</returns>
    ValueTask<WasmDenseOpticalFlowResult> EstimateDenseOpticalFlowAsync(
        WasmDenseOpticalFlowRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// [EN] Computes audio spectrum magnitudes.
    /// [JA] audio spectrum magnitude を計算します。
    /// </summary>
    /// <param name="request">[EN] Audio spectrum request. [JA] audio spectrum request です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視するトークンです。</param>
    /// <returns>[EN] Audio spectrum result. [JA] audio spectrum result を返します。</returns>
    ValueTask<WasmAudioSpectrumResult> ComputeAudioSpectrumAsync(
        WasmAudioSpectrumRequest request,
        CancellationToken cancellationToken);
}

/// <summary>
/// [EN] Describes a WebGPU-ready resident perception kernel.
/// [JA] WebGPU ready な resident perception kernel を記述します。
/// </summary>
public sealed record WasmResidentKernelDescriptor
{
    /// <summary>[EN] Gets the algorithm name. [JA] algorithm 名を取得します。</summary>
    public string AlgorithmName { get; init; } = string.Empty;

    /// <summary>[EN] Gets the shader name. [JA] shader 名を取得します。</summary>
    public string ShaderName { get; init; } = string.Empty;

    /// <summary>[EN] Gets the shader entry point. [JA] shader entry point を取得します。</summary>
    public string EntryPoint { get; init; } = "main";

    /// <summary>[EN] Gets workgroup size X. [JA] workgroup size X を取得します。</summary>
    public int WorkgroupSizeX { get; init; } = 8;

    /// <summary>[EN] Gets workgroup size Y. [JA] workgroup size Y を取得します。</summary>
    public int WorkgroupSizeY { get; init; } = 8;

    /// <summary>[EN] Gets workgroup size Z. [JA] workgroup size Z を取得します。</summary>
    public int WorkgroupSizeZ { get; init; } = 1;

    /// <summary>[EN] Gets dispatch size X. [JA] dispatch size X を取得します。</summary>
    public int DispatchSizeX { get; init; }

    /// <summary>[EN] Gets dispatch size Y. [JA] dispatch size Y を取得します。</summary>
    public int DispatchSizeY { get; init; }

    /// <summary>[EN] Gets dispatch size Z. [JA] dispatch size Z を取得します。</summary>
    public int DispatchSizeZ { get; init; } = 1;

    /// <summary>[EN] Gets input buffer layouts. [JA] input buffer layout を取得します。</summary>
    public IReadOnlyList<WasmResidentBufferLayout> Inputs { get; init; } = [];

    /// <summary>[EN] Gets output buffer layouts. [JA] output buffer layout を取得します。</summary>
    public IReadOnlyList<WasmResidentBufferLayout> Outputs { get; init; } = [];

    /// <summary>[EN] Gets deterministic descriptor metadata. [JA] deterministic descriptor metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// [EN] Describes a resident buffer layout for WebGPU dispatch.
/// [JA] WebGPU dispatch 用の resident buffer layout を記述します。
/// </summary>
public sealed record WasmResidentBufferLayout
{
    /// <summary>[EN] Gets the buffer name. [JA] buffer 名を取得します。</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>[EN] Gets the element type. [JA] element type を取得します。</summary>
    public string ElementType { get; init; } = "f32";

    /// <summary>[EN] Gets the shape string. [JA] shape string を取得します。</summary>
    public string Shape { get; init; } = string.Empty;

    /// <summary>[EN] Gets the stride string. [JA] stride string を取得します。</summary>
    public string? Stride { get; init; }

    /// <summary>[EN] Gets whether this buffer can remain resident in VRAM. [JA] この buffer が VRAM resident のままでよいかを取得します。</summary>
    public bool Resident { get; init; } = true;
}

/// <summary>
/// [EN] Carries scalar buffer data for resident perception algorithms.
/// [JA] resident perception algorithm 用の scalar buffer data を保持します。
/// </summary>
public sealed record WasmScalarBuffer
{
    /// <summary>[EN] Gets scalar values. [JA] scalar value を取得します。</summary>
    public IReadOnlyList<double> Values { get; init; } = [];

    /// <summary>[EN] Gets buffer width. [JA] buffer width を取得します。</summary>
    public int Width { get; init; }

    /// <summary>[EN] Gets buffer height. [JA] buffer height を取得します。</summary>
    public int Height { get; init; }

    /// <summary>[EN] Gets optional buffer stride. [JA] 任意の buffer stride を取得します。</summary>
    public int? Stride { get; init; }
}

/// <summary>
/// [EN] Carries RGB buffer data for resident perception algorithms.
/// [JA] resident perception algorithm 用の RGB buffer data を保持します。
/// </summary>
public sealed record WasmRgbBuffer
{
    /// <summary>[EN] Gets packed RGB bytes. [JA] packed RGB byte を取得します。</summary>
    public IReadOnlyList<byte> RgbBytes { get; init; } = [];

    /// <summary>[EN] Gets buffer width. [JA] buffer width を取得します。</summary>
    public int Width { get; init; }

    /// <summary>[EN] Gets buffer height. [JA] buffer height を取得します。</summary>
    public int Height { get; init; }

    /// <summary>[EN] Gets row stride in bytes. [JA] byte 単位の row stride を取得します。</summary>
    public int? StrideBytes { get; init; }
}

/// <summary>
/// [EN] Carries PCM buffer data for resident perception algorithms.
/// [JA] resident perception algorithm 用の PCM buffer data を保持します。
/// </summary>
public sealed record WasmPcmBuffer
{
    /// <summary>[EN] Gets PCM samples. [JA] PCM sample を取得します。</summary>
    public IReadOnlyList<float> Samples { get; init; } = [];

    /// <summary>[EN] Gets sample rate in hertz. [JA] hertz 単位の sample rate を取得します。</summary>
    public int SampleRate { get; init; } = 48_000;

    /// <summary>[EN] Gets channel count. [JA] channel count を取得します。</summary>
    public int Channels { get; init; } = 1;
}

/// <summary>
/// [EN] Carries HSV threshold mask input.
/// [JA] HSV threshold mask input を保持します。
/// </summary>
public sealed record WasmHsvThresholdMaskRequest
{
    /// <summary>[EN] Gets RGB buffer input. [JA] RGB buffer input を取得します。</summary>
    public WasmRgbBuffer Buffer { get; init; } = new();

    /// <summary>[EN] Gets minimum hue. [JA] minimum hue を取得します。</summary>
    public double MinHue { get; init; }

    /// <summary>[EN] Gets maximum hue. [JA] maximum hue を取得します。</summary>
    public double MaxHue { get; init; } = 360;

    /// <summary>[EN] Gets minimum saturation. [JA] minimum saturation を取得します。</summary>
    public double MinSaturation { get; init; }

    /// <summary>[EN] Gets minimum value. [JA] minimum value を取得します。</summary>
    public double MinValue { get; init; }

    /// <summary>[EN] Gets whether value should be ignored. [JA] value を無視するかどうかを取得します。</summary>
    public bool IgnoreValue { get; init; }
}

/// <summary>
/// [EN] Carries HSV threshold mask output.
/// [JA] HSV threshold mask output を保持します。
/// </summary>
public sealed record WasmHsvThresholdMaskResult
{
    /// <summary>[EN] Gets whether mask creation succeeded. [JA] mask creation が成功したかを取得します。</summary>
    public bool Succeeded { get; init; }

    /// <summary>[EN] Gets binary mask values. [JA] binary mask value を取得します。</summary>
    public IReadOnlyList<byte> Mask { get; init; } = [];

    /// <summary>[EN] Gets matched pixel ratio. [JA] matched pixel ratio を取得します。</summary>
    public double MatchRatio { get; init; }

    /// <summary>[EN] Gets WebGPU kernel descriptor. [JA] WebGPU kernel descriptor を取得します。</summary>
    public WasmResidentKernelDescriptor Kernel { get; init; } = new();

    /// <summary>[EN] Gets stable failure code. [JA] stable failure code を取得します。</summary>
    public string? ErrorCode { get; init; }

    /// <summary>[EN] Gets human-readable failure message. [JA] 人間可読 failure message を取得します。</summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// [EN] Carries max-pooling input.
/// [JA] max-pooling input を保持します。
/// </summary>
public sealed record WasmMaxPoolingRequest
{
    /// <summary>[EN] Gets scalar buffer input. [JA] scalar buffer input を取得します。</summary>
    public WasmScalarBuffer Buffer { get; init; } = new();

    /// <summary>[EN] Gets output width. [JA] output width を取得します。</summary>
    public int OutputWidth { get; init; }

    /// <summary>[EN] Gets output height. [JA] output height を取得します。</summary>
    public int OutputHeight { get; init; }
}

/// <summary>
/// [EN] Carries morphology input.
/// [JA] morphology input を保持します。
/// </summary>
public sealed record WasmMorphologyRequest
{
    /// <summary>[EN] Gets scalar buffer input. [JA] scalar buffer input を取得します。</summary>
    public WasmScalarBuffer Buffer { get; init; } = new();

    /// <summary>[EN] Gets operation name. [JA] operation 名を取得します。</summary>
    public string Operation { get; init; } = "dilation";

    /// <summary>[EN] Gets morphology radius. [JA] morphology radius を取得します。</summary>
    public int Radius { get; init; } = 1;
}

/// <summary>
/// [EN] Carries scalar buffer output.
/// [JA] scalar buffer output を保持します。
/// </summary>
public sealed record WasmScalarBufferResult
{
    /// <summary>[EN] Gets whether operation succeeded. [JA] operation が成功したかを取得します。</summary>
    public bool Succeeded { get; init; }

    /// <summary>[EN] Gets output buffer. [JA] output buffer を取得します。</summary>
    public WasmScalarBuffer Buffer { get; init; } = new();

    /// <summary>[EN] Gets WebGPU kernel descriptor. [JA] WebGPU kernel descriptor を取得します。</summary>
    public WasmResidentKernelDescriptor Kernel { get; init; } = new();

    /// <summary>[EN] Gets stable failure code. [JA] stable failure code を取得します。</summary>
    public string? ErrorCode { get; init; }

    /// <summary>[EN] Gets human-readable failure message. [JA] 人間可読 failure message を取得します。</summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// [EN] Carries dense optical flow input.
/// [JA] dense optical flow input を保持します。
/// </summary>
public sealed record WasmDenseOpticalFlowRequest
{
    /// <summary>[EN] Gets previous scalar buffer. [JA] previous scalar buffer を取得します。</summary>
    public WasmScalarBuffer Previous { get; init; } = new();

    /// <summary>[EN] Gets current scalar buffer. [JA] current scalar buffer を取得します。</summary>
    public WasmScalarBuffer Current { get; init; } = new();

    /// <summary>[EN] Gets local search radius. [JA] local search radius を取得します。</summary>
    public int SearchRadius { get; init; } = 1;
}

/// <summary>
/// [EN] Carries one WASM optical flow vector.
/// [JA] 1 つの WASM optical flow vector を保持します。
/// </summary>
public readonly record struct WasmOpticalFlowVector
{
    /// <summary>[EN] Gets X displacement. [JA] X displacement を取得します。</summary>
    public double X { get; init; }

    /// <summary>[EN] Gets Y displacement. [JA] Y displacement を取得します。</summary>
    public double Y { get; init; }

    /// <summary>[EN] Gets vector confidence. [JA] vector confidence を取得します。</summary>
    public double Confidence { get; init; }
}

/// <summary>
/// [EN] Carries dense optical flow output.
/// [JA] dense optical flow output を保持します。
/// </summary>
public sealed record WasmDenseOpticalFlowResult
{
    /// <summary>[EN] Gets whether optical flow succeeded. [JA] optical flow が成功したかを取得します。</summary>
    public bool Succeeded { get; init; }

    /// <summary>[EN] Gets vectors per pixel. [JA] pixel ごとの vector を取得します。</summary>
    public IReadOnlyList<WasmOpticalFlowVector> Vectors { get; init; } = [];

    /// <summary>[EN] Gets average X displacement. [JA] average X displacement を取得します。</summary>
    public double AverageX { get; init; }

    /// <summary>[EN] Gets average Y displacement. [JA] average Y displacement を取得します。</summary>
    public double AverageY { get; init; }

    /// <summary>[EN] Gets WebGPU kernel descriptor. [JA] WebGPU kernel descriptor を取得します。</summary>
    public WasmResidentKernelDescriptor Kernel { get; init; } = new();

    /// <summary>[EN] Gets stable failure code. [JA] stable failure code を取得します。</summary>
    public string? ErrorCode { get; init; }

    /// <summary>[EN] Gets human-readable failure message. [JA] 人間可読 failure message を取得します。</summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// [EN] Carries audio spectrum input.
/// [JA] audio spectrum input を保持します。
/// </summary>
public sealed record WasmAudioSpectrumRequest
{
    /// <summary>[EN] Gets PCM buffer input. [JA] PCM buffer input を取得します。</summary>
    public WasmPcmBuffer Buffer { get; init; } = new();

    /// <summary>[EN] Gets maximum FFT size. [JA] maximum FFT size を取得します。</summary>
    public int MaxFftSize { get; init; } = 1024;
}

/// <summary>
/// [EN] Carries audio spectrum output.
/// [JA] audio spectrum output を保持します。
/// </summary>
public sealed record WasmAudioSpectrumResult
{
    /// <summary>[EN] Gets whether spectrum calculation succeeded. [JA] spectrum calculation が成功したかを取得します。</summary>
    public bool Succeeded { get; init; }

    /// <summary>[EN] Gets magnitude values. [JA] magnitude value を取得します。</summary>
    public IReadOnlyList<double> Magnitudes { get; init; } = [];

    /// <summary>[EN] Gets FFT size. [JA] FFT size を取得します。</summary>
    public int FftSize { get; init; }

    /// <summary>[EN] Gets WebGPU kernel descriptor. [JA] WebGPU kernel descriptor を取得します。</summary>
    public WasmResidentKernelDescriptor Kernel { get; init; } = new();

    /// <summary>[EN] Gets stable failure code. [JA] stable failure code を取得します。</summary>
    public string? ErrorCode { get; init; }

    /// <summary>[EN] Gets human-readable failure message. [JA] 人間可読 failure message を取得します。</summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// [EN] Provides WebGPU-descriptor-first resident perception algorithms with CPU fallbacks.
/// [JA] CPU fallback を持つ WebGPU descriptor first の resident perception algorithm を提供します。
/// </summary>
public sealed class WasmResidentPerceptionAlgorithmLibrary : IWasmResidentPerceptionAlgorithmLibrary
{
    /// <summary>
    /// [EN] Initializes a WASM resident perception algorithm library.
    /// [JA] WASM resident perception algorithm library を初期化します。
    /// </summary>
    public WasmResidentPerceptionAlgorithmLibrary()
    {
    }

    /// <inheritdoc />
    public ValueTask<WasmHsvThresholdMaskResult> CreateHsvThresholdMaskAsync(
        WasmHsvThresholdMaskRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);
        var result = WasmResidentPerceptionAlgorithms.CreateHsvThresholdMask(
            CopyBytes(request.Buffer.RgbBytes),
            request.Buffer.Width,
            request.Buffer.Height,
            request.MinHue,
            request.MaxHue,
            request.MinSaturation,
            request.MinValue,
            request.IgnoreValue);
        return ValueTask.FromResult(result with { Kernel = Descriptor("hsv-threshold-mask", "resident_perception_hsv_threshold", request.Buffer.Width, request.Buffer.Height, "rgb:u8", "mask:u8") });
    }

    /// <inheritdoc />
    public ValueTask<WasmScalarBufferResult> MaxPoolAsync(
        WasmMaxPoolingRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);
        var result = WasmResidentPerceptionAlgorithms.MaxPool(
            CopyDoubles(request.Buffer.Values),
            request.Buffer.Width,
            request.Buffer.Height,
            request.OutputWidth,
            request.OutputHeight);
        return ValueTask.FromResult(result with { Kernel = Descriptor("max-pooling-downsample", "resident_perception_max_pool", request.OutputWidth, request.OutputHeight, "input:f32", "output:f32") });
    }

    /// <inheritdoc />
    public ValueTask<WasmScalarBufferResult> ApplyMorphologyAsync(
        WasmMorphologyRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);
        var result = WasmResidentPerceptionAlgorithms.ApplyMorphology(
            CopyDoubles(request.Buffer.Values),
            request.Buffer.Width,
            request.Buffer.Height,
            request.Operation,
            request.Radius);
        return ValueTask.FromResult(result with { Kernel = Descriptor("morphology", "resident_perception_morphology", request.Buffer.Width, request.Buffer.Height, "mask:f32", "output:f32") });
    }

    /// <inheritdoc />
    public ValueTask<WasmDenseOpticalFlowResult> EstimateDenseOpticalFlowAsync(
        WasmDenseOpticalFlowRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);
        var result = WasmResidentPerceptionAlgorithms.EstimateDenseOpticalFlow(
            CopyDoubles(request.Previous.Values),
            CopyDoubles(request.Current.Values),
            request.Current.Width,
            request.Current.Height,
            request.SearchRadius);
        return ValueTask.FromResult(result with { Kernel = Descriptor("dense-optical-flow", "resident_perception_dense_flow", request.Current.Width, request.Current.Height, "previous:f32,current:f32", "flow:vec2f") });
    }

    /// <inheritdoc />
    public ValueTask<WasmAudioSpectrumResult> ComputeAudioSpectrumAsync(
        WasmAudioSpectrumRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);
        var result = WasmResidentPerceptionAlgorithms.ComputeAudioSpectrum(
            CopyFloats(request.Buffer.Samples),
            request.Buffer.Channels,
            request.MaxFftSize);
        return ValueTask.FromResult(result with { Kernel = Descriptor("audio-fft-spectrum", "resident_perception_audio_fft", Math.Max(1, result.FftSize / 64), 1, "pcm:f32", "spectrum:f32") });
    }

    private static WasmResidentKernelDescriptor Descriptor(
        string algorithmName,
        string shaderName,
        int dispatchX,
        int dispatchY,
        string inputShape,
        string outputShape)
        => new()
        {
            AlgorithmName = algorithmName,
            ShaderName = shaderName,
            EntryPoint = "main",
            WorkgroupSizeX = 8,
            WorkgroupSizeY = 8,
            WorkgroupSizeZ = 1,
            DispatchSizeX = Math.Max(1, dispatchX),
            DispatchSizeY = Math.Max(1, dispatchY),
            DispatchSizeZ = 1,
            Inputs = [Layout("input", inputShape)],
            Outputs = [Layout("output", outputShape)],
            Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["resident"] = "true",
                ["fallback"] = "cpu"
            }
        };

    private static WasmResidentBufferLayout Layout(string name, string shape)
        => new()
        {
            Name = name,
            Shape = shape,
            ElementType = shape.Contains("u8", StringComparison.Ordinal) ? "u8" : "f32",
            Resident = true
        };

    private static byte[] CopyBytes(IReadOnlyList<byte> values)
    {
        var copy = new byte[values.Count];
        for (var index = 0; index < values.Count; index++)
        {
            copy[index] = values[index];
        }

        return copy;
    }

    private static double[] CopyDoubles(IReadOnlyList<double> values)
    {
        var copy = new double[values.Count];
        for (var index = 0; index < values.Count; index++)
        {
            copy[index] = values[index];
        }

        return copy;
    }

    private static float[] CopyFloats(IReadOnlyList<float> values)
    {
        var copy = new float[values.Count];
        for (var index = 0; index < values.Count; index++)
        {
            copy[index] = values[index];
        }

        return copy;
    }
}

/// <summary>
/// [EN] Provides CPU fallback implementations with the same meaning as resident kernels.
/// [JA] resident kernel と同じ意味論を持つ CPU fallback implementation を提供します。
/// </summary>
public static class WasmResidentPerceptionAlgorithms
{
    /// <summary>
    /// [EN] Creates a HSV threshold mask.
    /// [JA] HSV threshold mask を作成します。
    /// </summary>
    public static WasmHsvThresholdMaskResult CreateHsvThresholdMask(ReadOnlySpan<byte> rgbBytes, int width, int height, double minHue, double maxHue, double minSaturation, double minValue, bool ignoreValue)
    {
        var count = width * height;
        if (width <= 0 || height <= 0 || rgbBytes.Length < count * 3)
        {
            return new WasmHsvThresholdMaskResult { ErrorCode = "WASM_PERCEPTION_HSV_INPUT_INVALID", ErrorMessage = "RGB input and dimensions are required." };
        }

        var mask = new byte[count];
        var matched = 0;
        for (var pixel = 0; pixel < count; pixel++)
        {
            var offset = pixel * 3;
            var hsv = RgbToHsv(rgbBytes[offset], rgbBytes[offset + 1], rgbBytes[offset + 2]);
            var valueMatch = ignoreValue || hsv.Value >= minValue;
            if (HueInRange(hsv.Hue, minHue, maxHue) && hsv.Saturation >= minSaturation && valueMatch)
            {
                mask[pixel] = 1;
                matched += 1;
            }
        }

        return new WasmHsvThresholdMaskResult { Succeeded = true, Mask = mask, MatchRatio = Round4((double)matched / count) };
    }

    /// <summary>
    /// [EN] Performs max-pooling downsampling.
    /// [JA] max-pooling downsampling を実行します。
    /// </summary>
    public static WasmScalarBufferResult MaxPool(ReadOnlySpan<double> values, int width, int height, int outputWidth, int outputHeight)
    {
        if (width <= 0 || height <= 0 || outputWidth <= 0 || outputHeight <= 0 || values.Length < width * height)
        {
            return new WasmScalarBufferResult { ErrorCode = "WASM_PERCEPTION_MAX_POOL_INPUT_INVALID", ErrorMessage = "Input/output dimensions and values are required." };
        }

        var output = new double[outputWidth * outputHeight];
        for (var oy = 0; oy < outputHeight; oy++)
        {
            var y0 = oy * height / outputHeight;
            var y1 = Math.Max(y0 + 1, (oy + 1) * height / outputHeight);
            for (var ox = 0; ox < outputWidth; ox++)
            {
                var x0 = ox * width / outputWidth;
                var x1 = Math.Max(x0 + 1, (ox + 1) * width / outputWidth);
                var max = double.MinValue;
                for (var y = y0; y < y1; y++)
                {
                    for (var x = x0; x < x1; x++)
                    {
                        max = Math.Max(max, values[(y * width) + x]);
                    }
                }

                output[(oy * outputWidth) + ox] = Round4(max == double.MinValue ? 0 : max);
            }
        }

        return new WasmScalarBufferResult { Succeeded = true, Buffer = new WasmScalarBuffer { Values = output, Width = outputWidth, Height = outputHeight } };
    }

    /// <summary>
    /// [EN] Applies morphology dilation or erosion.
    /// [JA] morphology dilation または erosion を適用します。
    /// </summary>
    public static WasmScalarBufferResult ApplyMorphology(ReadOnlySpan<double> values, int width, int height, string operation, int radius)
    {
        if (width <= 0 || height <= 0 || values.Length < width * height)
        {
            return new WasmScalarBufferResult { ErrorCode = "WASM_PERCEPTION_MORPHOLOGY_INPUT_INVALID", ErrorMessage = "Mask values and dimensions are required." };
        }

        var erosion = string.Equals(operation, "erosion", StringComparison.OrdinalIgnoreCase);
        var safeRadius = Math.Max(1, radius);
        var output = new double[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var value = erosion ? double.MaxValue : double.MinValue;
                for (var dy = -safeRadius; dy <= safeRadius; dy++)
                {
                    var yy = Math.Clamp(y + dy, 0, height - 1);
                    for (var dx = -safeRadius; dx <= safeRadius; dx++)
                    {
                        var xx = Math.Clamp(x + dx, 0, width - 1);
                        var candidate = values[(yy * width) + xx];
                        value = erosion ? Math.Min(value, candidate) : Math.Max(value, candidate);
                    }
                }

                output[(y * width) + x] = Round4(value);
            }
        }

        return new WasmScalarBufferResult { Succeeded = true, Buffer = new WasmScalarBuffer { Values = output, Width = width, Height = height } };
    }

    /// <summary>
    /// [EN] Estimates dense optical flow using local matching.
    /// [JA] local matching により dense optical flow を推定します。
    /// </summary>
    public static WasmDenseOpticalFlowResult EstimateDenseOpticalFlow(ReadOnlySpan<double> previous, ReadOnlySpan<double> current, int width, int height, int searchRadius)
    {
        if (width <= 1 || height <= 1 || previous.Length < width * height || current.Length < width * height)
        {
            return new WasmDenseOpticalFlowResult { ErrorCode = "WASM_PERCEPTION_FLOW_INPUT_INVALID", ErrorMessage = "Previous/current frames and dimensions are required." };
        }

        var radius = Math.Clamp(searchRadius, 1, 4);
        var vectors = new WasmOpticalFlowVector[width * height];
        var totalX = 0.0;
        var totalY = 0.0;
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var index = (y * width) + x;
                var target = current[index];
                var bestError = double.MaxValue;
                var bestDx = 0;
                var bestDy = 0;
                for (var dy = -radius; dy <= radius; dy++)
                {
                    var yy = Math.Clamp(y + dy, 0, height - 1);
                    for (var dx = -radius; dx <= radius; dx++)
                    {
                        var xx = Math.Clamp(x + dx, 0, width - 1);
                        var error = Math.Abs(target - previous[(yy * width) + xx]);
                        if (error < bestError)
                        {
                            bestError = error;
                            bestDx = dx;
                            bestDy = dy;
                        }
                    }
                }

                vectors[index] = new WasmOpticalFlowVector { X = bestDx, Y = bestDy, Confidence = Round4(1 / (1 + bestError)) };
                totalX += bestDx;
                totalY += bestDy;
            }
        }

        return new WasmDenseOpticalFlowResult { Succeeded = true, Vectors = vectors, AverageX = Round4(totalX / vectors.Length), AverageY = Round4(totalY / vectors.Length) };
    }

    /// <summary>
    /// [EN] Computes audio spectrum magnitudes using a deterministic DFT fallback.
    /// [JA] deterministic DFT fallback により audio spectrum magnitude を計算します。
    /// </summary>
    public static WasmAudioSpectrumResult ComputeAudioSpectrum(ReadOnlySpan<float> samples, int channels, int maxFftSize)
    {
        if (channels <= 0 || samples.Length < channels * 2)
        {
            return new WasmAudioSpectrumResult { ErrorCode = "WASM_PERCEPTION_SPECTRUM_INPUT_INVALID", ErrorMessage = "PCM samples and channel count are required." };
        }

        var frames = samples.Length / channels;
        var fftSize = PreviousPowerOfTwo(Math.Min(Math.Max(2, maxFftSize), frames));
        var bins = fftSize / 2;
        var magnitudes = new double[bins];
        var max = 0.0;
        for (var bin = 0; bin < bins; bin++)
        {
            var real = 0.0;
            var imaginary = 0.0;
            for (var frame = 0; frame < fftSize; frame++)
            {
                var value = 0.0;
                for (var channel = 0; channel < channels; channel++)
                {
                    value += samples[(frame * channels) + channel];
                }

                value /= channels;
                var angle = -2 * Math.PI * bin * frame / fftSize;
                real += value * Math.Cos(angle);
                imaginary += value * Math.Sin(angle);
            }

            var magnitude = Math.Sqrt((real * real) + (imaginary * imaginary));
            magnitudes[bin] = magnitude;
            max = Math.Max(max, magnitude);
        }

        if (max > 0)
        {
            for (var index = 0; index < magnitudes.Length; index++)
            {
                magnitudes[index] = Round4(magnitudes[index] / max);
            }
        }

        return new WasmAudioSpectrumResult { Succeeded = true, Magnitudes = magnitudes, FftSize = fftSize };
    }

    private readonly record struct Hsv(double Hue, double Saturation, double Value);

    private static Hsv RgbToHsv(byte red, byte green, byte blue)
    {
        var r = red / 255.0;
        var g = green / 255.0;
        var b = blue / 255.0;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;
        var hue = 0.0;
        if (delta > 0)
        {
            hue = Math.Abs(max - r) <= double.Epsilon
                ? 60 * (((g - b) / delta) % 6)
                : Math.Abs(max - g) <= double.Epsilon
                    ? 60 * (((b - r) / delta) + 2)
                    : 60 * (((r - g) / delta) + 4);
        }

        if (hue < 0)
        {
            hue += 360;
        }

        return new Hsv(hue, max <= double.Epsilon ? 0 : delta / max, max);
    }

    private static bool HueInRange(double hue, double minHue, double maxHue)
    {
        var min = NormalizeHue(minHue);
        var max = NormalizeHue(maxHue);
        return min <= max ? hue >= min && hue <= max : hue >= min || hue <= max;
    }

    private static double NormalizeHue(double hue)
        => ((hue % 360) + 360) % 360;

    private static int PreviousPowerOfTwo(int value)
    {
        var power = 1;
        while (power * 2 <= value)
        {
            power *= 2;
        }

        return power;
    }

    private static double Round4(double value)
        => Math.Round(value, 4, MidpointRounding.AwayFromZero);
}
