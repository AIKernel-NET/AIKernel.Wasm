namespace AIKernel.Wasm.Perception;

using System.Globalization;

/// <summary>
/// [EN] Creates descriptor-driven WebGPU plans for synthetic sensor fused passes.
/// [JA] 合成 sensor fused pass 用の descriptor-driven WebGPU plan を作成します。
/// </summary>
public sealed class WasmSyntheticSensorKernelPlanner : IWasmSyntheticSensorKernelPlanner
{
    /// <summary>
    /// [EN] Creates a fused resident kernel descriptor for Phainesis and Nous extraction.
    /// [JA] Phainesis / Nous 抽出用の fused resident kernel descriptor を作成します。
    /// </summary>
    /// <param name="request">[EN] Synthetic sensor request. [JA] 合成 sensor request です。</param>
    /// <param name="phenomenonCount">[EN] Number of phenomenon outputs. [JA] phenomenon output 数です。</param>
    /// <param name="vectorCount">[EN] Number of vector outputs. [JA] vector output 数です。</param>
    /// <returns>[EN] Resident kernel descriptor. [JA] resident kernel descriptor を返します。</returns>
    public WasmResidentKernelDescriptor CreateKernelDescriptor(
        WasmSyntheticSensorRequest request,
        int phenomenonCount,
        int vectorCount)
    {
        ArgumentNullException.ThrowIfNull(request);

        var width = Math.Max(
            Math.Max(request.CurrentFrame.Width, request.NavigableMask.Width),
            request.ThreatMask.Width);
        var height = Math.Max(
            Math.Max(request.CurrentFrame.Height, request.NavigableMask.Height),
            request.ThreatMask.Height);
        var dispatchX = Math.Max(1, (width + 15) / 16);
        var dispatchY = Math.Max(1, (height + 15) / 16);
        var scalarCount = Math.Max(1, request.SensorScalars.Count);

        return new WasmResidentKernelDescriptor
        {
            AlgorithmName = "synthetic-sensor-fusion",
            ShaderName = "resident_perception_synthetic_sensor_fusion",
            EntryPoint = "main",
            WorkgroupSizeX = 16,
            WorkgroupSizeY = 16,
            WorkgroupSizeZ = 1,
            DispatchSizeX = dispatchX,
            DispatchSizeY = dispatchY,
            DispatchSizeZ = 1,
            Inputs =
            [
                Layout(0, "currentFrame", Shape(request.CurrentFrame), "storage", "read", ByteLength(request.CurrentFrame.Values.Count, sizeof(float))),
                Layout(1, "previousFrame", Shape(request.PreviousFrame), "storage", "read", ByteLength(request.PreviousFrame.Values.Count, sizeof(float))),
                Layout(2, "navigableMask", Shape(request.NavigableMask), "storage", "read", ByteLength(request.NavigableMask.Values.Count, sizeof(float))),
                Layout(3, "threatMask", Shape(request.ThreatMask), "storage", "read", ByteLength(request.ThreatMask.Values.Count, sizeof(float))),
                Layout(4, "sensorScalars", scalarCount.ToString(CultureInfo.InvariantCulture), "uniform", "read", scalarCount * sizeof(float))
            ],
            Outputs =
            [
                Layout(5, "phenomena", Math.Max(1, phenomenonCount).ToString(CultureInfo.InvariantCulture), "storage", "read_write", Math.Max(1, phenomenonCount) * 8 * sizeof(float)),
                Layout(6, "vectors", Math.Max(1, vectorCount).ToString(CultureInfo.InvariantCulture), "storage", "read_write", Math.Max(1, vectorCount) * 6 * sizeof(float))
            ],
            Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["resident"] = "true",
                ["fallback"] = "cpu",
                ["fusedPass"] = "true",
                ["zeroCopyPreferred"] = "true",
                ["purpose"] = "phainesis-nous-synthetic-sensors"
            }
        };
    }

    private static WasmResidentBufferLayout Layout(
        int binding,
        string name,
        string shape,
        string usage,
        string access,
        int byteLength)
        => new()
        {
            Binding = binding,
            Name = name,
            ElementType = "f32",
            Shape = shape,
            Usage = usage,
            Access = access,
            ByteLength = byteLength,
            Resident = true
        };

    private static string Shape(WasmScalarBuffer buffer)
        => buffer.Width > 0 && buffer.Height > 0
            ? $"{buffer.Height.ToString(CultureInfo.InvariantCulture)},{buffer.Width.ToString(CultureInfo.InvariantCulture)}"
            : Math.Max(1, buffer.Values.Count).ToString(CultureInfo.InvariantCulture);

    private static int ByteLength(int elementCount, int elementSize)
        => Math.Max(1, elementCount) * elementSize;
}
