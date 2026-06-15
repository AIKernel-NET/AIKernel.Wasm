namespace AIKernel.Wasm.Models;

using AIKernel.Abstractions.Compute;
using AIKernel.Common.Results;
using AIKernel.Dtos.Providers;
using AIKernel.Enums;
using AIKernel.Wasm.Compute;
using AIKernel.Wasm.Runtime;

/// <summary>
/// [EN] Executes descriptor-driven resident model kernels through the WebGPU compute boundary.
/// [JA] WebGPU compute 境界を通じて descriptor-driven resident model kernel を実行します。
/// </summary>
public sealed class WebGpuResidentModelProvider : WasmKernelProviderBase
{
    private readonly WebGpuComputeProvider _computeProvider;

    /// <summary>
    /// [EN] Initializes a WebGPU resident model provider.
    /// [JA] WebGPU resident model Provider を初期化します。
    /// </summary>
    /// <param name="computeProvider">[EN] Optional WebGPU compute provider. [JA] 任意の WebGPU compute Provider です。</param>
    public WebGpuResidentModelProvider(WebGpuComputeProvider? computeProvider = null)
        : base(
            "wasm.webgpu.model",
            "WASM WebGPU Resident Model Provider",
            ["wasm.model.execute", "wasm.webgpu.dispatch"],
            ["buffer", "uint8", "float32"],
            Capabilities())
    {
        _computeProvider = computeProvider ?? new WebGpuComputeProvider();
    }

    /// <summary>
    /// [EN] Executes a descriptor-driven model dispatch without binding to a specific model family.
    /// [JA] 特定の model family に binding せず descriptor-driven model dispatch を実行します。
    /// </summary>
    /// <param name="request">[EN] Model execution request. [JA] model execution request です。</param>
    /// <param name="cancellationToken">[EN] Cancellation token. [JA] キャンセル通知を監視するトークンです。</param>
    /// <returns>[EN] Model execution result. [JA] model execution result を返します。</returns>
    public async ValueTask<Result<WebGpuResidentModelExecutionResult>> ExecuteModelAsync(
        WebGpuResidentModelExecutionRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.KernelSource))
        {
            return Result<WebGpuResidentModelExecutionResult>.Ok(Failure(
                "WASM_MODEL_KERNEL_REQUIRED",
                "A WGSL kernel source is required for descriptor-driven model execution."));
        }

        if (request.DispatchX <= 0 || request.DispatchY <= 0 || request.DispatchZ <= 0)
        {
            return Result<WebGpuResidentModelExecutionResult>.Ok(Failure(
                "WASM_MODEL_DISPATCH_INVALID",
                "Dispatch dimensions must be greater than zero."));
        }

        var buffers = new List<ComputeBuffer>();
        try
        {
            await _computeProvider.InitializeAsync().ConfigureAwait(false);
            foreach (var binding in request.Bindings.OrderBy(item => item.Binding).ThenBy(item => item.Name, StringComparer.Ordinal))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var size = Math.Max(binding.ByteLength, binding.Payload.Count);
                var buffer = await _computeProvider.CreateBufferAsync(size).ConfigureAwait(false);
                buffers.Add(buffer);

                if (binding.Payload.Count > 0)
                {
                    await _computeProvider.WriteBufferAsync(buffer, binding.Payload.ToArray()).ConfigureAwait(false);
                }
            }

            var kernel = new ComputeKernel(request.KernelSource, request.DispatchX, request.DispatchY, request.DispatchZ);
            await _computeProvider.ExecuteKernelAsync(kernel, buffers.ToArray()).ConfigureAwait(false);

            var outputs = new Dictionary<string, IReadOnlyList<byte>>(StringComparer.Ordinal);
            foreach (var pair in request.Bindings
                .OrderBy(item => item.Binding)
                .ThenBy(item => item.Name, StringComparer.Ordinal)
                .Zip(buffers, (binding, buffer) => new { binding, buffer })
                .Where(item => item.binding.ReadBack))
            {
                var destination = new byte[pair.buffer.Size];
                await _computeProvider.ReadBufferAsync(pair.buffer, destination).ConfigureAwait(false);
                outputs[pair.binding.Name] = destination;
            }

            return Result<WebGpuResidentModelExecutionResult>.Ok(new WebGpuResidentModelExecutionResult
            {
                Succeeded = true,
                Outputs = outputs,
                ZeroCopyHint = request.Descriptor.PreferZeroCopy && buffers.Count > 0 && buffers.All(buffer => buffer.NativeBuffer is not null),
                Metadata = MergeMetadata(request)
            });
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result<WebGpuResidentModelExecutionResult>.Ok(Failure(
                "WASM_MODEL_EXECUTION_FAILED",
                ex.Message));
        }
        finally
        {
            foreach (var buffer in buffers)
            {
                buffer.Dispose();
            }
        }
    }

    private static WebGpuResidentModelExecutionResult Failure(string code, string message)
        => new()
        {
            Succeeded = false,
            ErrorCode = code,
            ErrorMessage = message,
            Diagnostics = [code]
        };

    private static IReadOnlyDictionary<string, string> MergeMetadata(WebGpuResidentModelExecutionRequest request)
    {
        var metadata = new Dictionary<string, string>(request.Descriptor.Metadata, StringComparer.Ordinal)
        {
            ["modelId"] = request.Descriptor.ModelId,
            ["modelFamily"] = request.Descriptor.ModelFamily,
            ["manifestRef"] = request.Descriptor.ManifestRef,
            ["entryPointId"] = request.Descriptor.EntryPointId,
            ["bindingCount"] = request.Bindings.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };

        foreach (var item in request.Metadata.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            metadata[item.Key] = item.Value;
        }

        return metadata;
    }

    private static IReadOnlyList<ProviderCapability> Capabilities()
        =>
        [
            new()
            {
                LogicalCapabilityId = "wasm.model.execution",
                ProviderCapabilityId = "wasm.webgpu.model.execute",
                Flags = ProviderCapabilityFlags.None,
                Kind = ProviderKind.Provider,
                InputModalities = InputModalities.Frame | InputModalities.Image | InputModalities.Text,
                OutputModalities = OutputModalities.Text | OutputModalities.Telemetry,
                RiskLevel = ProviderRiskLevel.ReadOnly,
                Availability = new CapabilityAvailability
                {
                    IsAvailable = true,
                    Reason = ProviderAvailabilityReason.Available
                }
            }
        ];
}
