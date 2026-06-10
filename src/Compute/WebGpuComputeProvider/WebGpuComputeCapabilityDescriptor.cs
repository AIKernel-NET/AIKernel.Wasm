namespace AIKernel.Wasm.Comput;

/// <summary>
/// [EN] Public capability descriptor for the WebGPU compute external provider.
/// [JA] WebGPU compute 外部 Provider の公開 capability descriptor です。
/// </summary>
public sealed record WebGpuComputeCapabilityDescriptor(
    string CapabilityId,
    string AdapterProfile,
    IReadOnlyDictionary<string, string> Metadata);
