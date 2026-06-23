using AIKernel.Abstractions.Capabilities;
using AIKernel.Common.Results;
using AIKernel.Dtos.Capabilities;
using AIKernel.Dtos.Gpu;

namespace AIKernel.Wasm.Compute;

/// <summary>
/// [EN] Capability module invoker for WebGPU compute provider operations.
/// [JA] WebGPU compute Provider operation 用の capability module invoker です。
/// </summary>
public class WebGpuComputeInvoker : ICapabilityModuleInvoker
{
    private static readonly WebGpuComputeSettings DefaultSettings = new();

    /// <summary>
    /// [EN] Invokes a WebGPU compute capability operation.
    /// [JA] WebGPU compute capability operation を実行します。
    /// </summary>
    public ValueTask<CapabilityInvocationResult> InvokeAsync(
        CapabilityInvocationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        var supported = GpuOperationNames.WebGpuComputeProviderOperations
            .Contains(request.Operation, StringComparer.OrdinalIgnoreCase);
        var metadata = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in request.Metadata.OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            metadata[item.Key] = item.Value;
        }

        foreach (var item in DefaultSettings.ToMetadata())
        {
            metadata[item.Key] = item.Value;
        }

        metadata["provider"] = "WebGpuComputeProvider";
        metadata["operation"] = request.Operation;
        var unsupported = UnsupportedOperation(supported, request.Operation);

        return ValueTask.FromResult(new CapabilityInvocationResult(
            request.InvocationId,
            request.CapabilityId,
            Succeeded: supported,
            OutputHash: null,
            ErrorCode: unsupported.Match<string?>(() => null, error => error.Code),
            ErrorMessage: unsupported.Match<string?>(() => null, error => error.Message),
            ReplayLogHash: request.ReplayLogHash,
            Metadata: metadata));
    }

    private static Option<MonadicError> UnsupportedOperation(bool supported, string operation)
        => MonadicDecision.ErrorUnless(
            supported,
            "WEBGPU_OPERATION_NOT_SUPPORTED",
            $"Unsupported WebGPU compute operation: {operation}.");
}
