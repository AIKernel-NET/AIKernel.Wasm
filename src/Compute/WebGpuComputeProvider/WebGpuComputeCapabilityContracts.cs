using AIKernel.Dtos.Capabilities;
using AIKernel.Enums;

namespace AIKernel.Wasm.Compute;

/// <summary>
/// [EN] Contract mapper for WebGPU compute provider capabilities.
/// [JA] WebGPU compute Provider capability の contract mapper です。
/// </summary>
public static class WebGpuComputeCapabilityContracts
{
    /// <summary>
    /// [EN] Converts a WebGPU provider descriptor into the shared capability module contract.
    /// [JA] WebGPU Provider descriptor を共有 capability module contract へ変換します。
    /// </summary>
    public static CapabilityModuleDescriptor ToContract(
        WebGpuComputeCapabilityDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        return new CapabilityModuleDescriptor(
            descriptor.CapabilityId,
            "WebGPU Compute Provider",
            CapabilityModuleKind.NativeLibrary,
            CapabilityInvocationMode.Direct,
            GetMetadataValue(descriptor.Metadata, "version", "0.1.0"),
            "webgpu_dispatch",
            null,
            null,
            ["compute.dispatch", "compute.vector_add"],
            ["compute.execute", "buffer.read", "buffer.write"],
            descriptor.Metadata);
    }

    private static string GetMetadataValue(
        IReadOnlyDictionary<string, string> metadata,
        string key,
        string fallback)
        => metadata.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
}
