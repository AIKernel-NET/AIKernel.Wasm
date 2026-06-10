namespace AIKernel.Wasm.Comput;

/// <summary>
/// [EN] Python bridge that exposes the public WebGPU compute provider contract surface.
/// [JA] 公開 WebGPU compute Provider 契約 surface を公開する Python bridge です。
/// </summary>
public static class WebGpuComputePythonBridge
{
    /// <summary>
    /// [EN] Creates a deterministic capability descriptor for Python wrappers.
    /// [JA] Python wrapper 用の決定論的 capability descriptor を作成します。
    /// </summary>
    public static object ToContract(
        string providerId,
        string adapterProfile)
    {
        var settings = new WebGpuComputeSettings
        {
            ProviderId = providerId,
            AdapterProfile = adapterProfile
        };

        return WebGpuComputeCapabilityContracts.ToContract(
            new WebGpuComputeCapabilityDescriptor(
                settings.ProviderId,
                settings.AdapterProfile,
                settings.ToMetadata()));
    }

    /// <summary>
    /// [EN] Creates a provider with default settings.
    /// [JA] default settings の Provider を作成します。
    /// </summary>
    public static object CreateProvider()
        => new WebGpuComputeProvider();

    /// <summary>
    /// [EN] Creates a capability module invoker.
    /// [JA] capability module invoker を作成します。
    /// </summary>
    public static object CreateInvoker()
        => new WebGpuComputeInvoker();
}
