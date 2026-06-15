namespace AIKernel.Wasm.Compute;

/// <summary>
/// [EN] Public capability descriptor for the WebGPU compute external provider.
/// [JA] WebGPU compute 外部 Provider の公開 capability descriptor です。
/// </summary>
public record WebGpuComputeCapabilityDescriptor(
    string CapabilityId,
    string AdapterProfile,
    IReadOnlyDictionary<string, string> Metadata);
