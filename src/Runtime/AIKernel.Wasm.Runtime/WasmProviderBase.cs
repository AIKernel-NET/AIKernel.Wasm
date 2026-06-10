namespace AIKernel.Wasm.Runtime;

using AIKernel.Abstractions.Providers;
using AIKernel.Common.Results;
using AIKernel.Dtos.Core;

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
