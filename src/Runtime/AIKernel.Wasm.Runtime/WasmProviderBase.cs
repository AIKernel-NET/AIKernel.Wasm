namespace AIKernel.Wasm.Runtime;

using AIKernel.Abstractions.Providers;
using AIKernel.Common.Results;
using AIKernel.Dtos.Core;
using AIKernel.Dtos.Providers;
using AIKernel.Enums;

/// <summary>
/// [EN] Base provider for AIKernel WASM runtime services.
/// [JA] AIKernel WASM runtime service 向けの base Provider です。
/// </summary>
public abstract class WasmProviderBase : IProvider
{
    private readonly IProviderCapabilities _capabilities;
    private volatile bool _initialized;

    /// <summary>
    /// [EN] Initializes a WASM provider service.
    /// [JA] WASM Provider service を初期化します。
    /// </summary>
    protected WasmProviderBase(
        string providerId,
        string name,
        IEnumerable<string> operations,
        IEnumerable<string> dataTypes)
    {
        ProviderId = providerId;
        Name = name;
        _capabilities = new WasmProviderCapabilities(operations, dataTypes);
    }

    /// <summary>[EN] Provider identifier. [JA] Provider 識別子です。</summary>
    public string ProviderId { get; }

    /// <summary>[EN] Provider display name. [JA] Provider 表示名です。</summary>
    public string Name { get; }

    /// <summary>[EN] Provider version. [JA] Provider version です。</summary>
    public string Version => "0.1.0";

    /// <summary>[EN] Gets provider capabilities. [JA] Provider capability を取得します。</summary>
    public IProviderCapabilities GetCapabilities() => _capabilities;

    /// <summary>[EN] Returns whether the provider service was initialized. [JA] Provider service が初期化済みかどうかを返します。</summary>
    public Task<bool> IsAvailableAsync() => Task.FromResult(_initialized);

    /// <summary>[EN] Initializes the provider service. [JA] Provider service を初期化します。</summary>
    public virtual Task InitializeAsync()
    {
        _initialized = true;
        return Task.CompletedTask;
    }

    /// <summary>[EN] Shuts down the provider service. [JA] Provider service を終了します。</summary>
    public virtual Task ShutdownAsync()
    {
        _initialized = false;
        return Task.CompletedTask;
    }

    /// <summary>[EN] Gets provider health information. [JA] Provider health 情報を取得します。</summary>
    public Task<ProviderHealthStatus> GetHealthAsync()
        => Task.FromResult(new ProviderHealthStatus(
            _initialized,
            MonadicDecision.SelectText(_initialized, "WASM provider is not initialized.", "WASM provider initialized."),
            DateTime.UtcNow,
            0));
}

/// <summary>
/// [EN] Base provider for WASM services that implement the AIKernel capability-aware provider contract.
/// [JA] AIKernel の capability-aware provider contract を実装する WASM service 向け base Provider です。
/// </summary>
public abstract class WasmKernelProviderBase : WasmProviderBase, IKernelProvider
{
    private readonly IReadOnlyList<ProviderCapability> _providerCapabilities;

    /// <summary>
    /// [EN] Initializes a capability-aware WASM provider service.
    /// [JA] capability-aware な WASM Provider service を初期化します。
    /// </summary>
    protected WasmKernelProviderBase(
        string providerId,
        string name,
        IEnumerable<string> operations,
        IEnumerable<string> dataTypes,
        IReadOnlyList<ProviderCapability> providerCapabilities)
        : base(providerId, name, operations, dataTypes)
    {
        _providerCapabilities = providerCapabilities;
    }

    /// <summary>
    /// [EN] Gets WASM provider capabilities for routing and admission.
    /// [JA] routing / admission 用の WASM Provider capability を取得します。
    /// </summary>
    public ValueTask<IReadOnlyList<ProviderCapability>> GetCapabilitiesAsync(
        ProviderPreparationContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(_providerCapabilities);
    }

    /// <summary>
    /// [EN] Probes WASM capability availability without performing side effects.
    /// [JA] side effect を実行せずに WASM capability availability を probe します。
    /// </summary>
    public ValueTask<CapabilityAvailability> ProbeAsync(
        CapabilityQuery request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);

        var capability = _providerCapabilities.FirstOrDefault(item =>
            string.Equals(item.LogicalCapabilityId, request.LogicalCapabilityId, StringComparison.Ordinal) ||
            string.Equals(item.ProviderCapabilityId, request.LogicalCapabilityId, StringComparison.Ordinal));

        if (capability is null)
        {
            return ValueTask.FromResult(new CapabilityAvailability
            {
                IsAvailable = false,
                Reason = ProviderAvailabilityReason.HostCapabilityMissing,
                Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["logicalCapabilityId"] = request.LogicalCapabilityId
                }
            });
        }

        var inputMatches = request.RequiredInputs == InputModalities.None ||
            (capability.InputModalities & request.RequiredInputs) == request.RequiredInputs;
        var outputMatches = request.RequiredOutputs == OutputModalities.None ||
            (capability.OutputModalities & request.RequiredOutputs) == request.RequiredOutputs;
        var riskMatches = capability.RiskLevel == ProviderRiskLevel.Unknown ||
            capability.RiskLevel <= request.MaximumRiskLevel;
        var privilegeMatches = !capability.PrivilegedAction || request.AllowPrivilegedAction;
        var available = inputMatches && outputMatches && riskMatches && privilegeMatches && capability.Availability.IsAvailable;

        return ValueTask.FromResult(capability.Availability with
        {
            IsAvailable = available,
            Reason = available ? ProviderAvailabilityReason.Available : ProviderAvailabilityReason.PermissionDenied
        });
    }

    /// <summary>
    /// [EN] Returns a deterministic unsupported-operation result for generic execution calls.
    /// [JA] generic execution call に対して deterministic な unsupported-operation result を返します。
    /// </summary>
    public virtual ValueTask<ProviderResult<T>> ExecuteAsync<T>(
        string operationId,
        ProviderExecutionContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(new ProviderResult<T>
        {
            Success = false,
            FailureCode = "WASM_OPERATION_NOT_SUPPORTED",
            FailureMessage = $"WASM provider operation is not supported by this surface: {operationId}",
            Diagnostics =
            [
                new ProviderDiagnostic
                {
                    Code = "WASM_OPERATION_NOT_SUPPORTED",
                    Message = "Use the strongly typed WASM provider API for this package.",
                    IsRetryable = false
                }
            ]
        });
    }
}
