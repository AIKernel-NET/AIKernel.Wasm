using AIKernel.Abstractions.Processes;
using AIKernel.Common.Results;

namespace AIKernel.Wasm.Runtime.Abstractions;

/// <summary>
/// [EN] Stable WASM process-provider contract.
/// [JA] 安定した WASM process-provider contract です。
/// </summary>
public interface IWasmProcessProvider : IProcessHost
{
    /// <summary>
    /// [EN] Safely creates a WASM process handle.
    /// [JA] WASM process handle を安全に作成します。
    /// </summary>
    /// <param name="name">EN:  JA: name パラメーターです。
    /// [EN] Process name.
    /// [JA] process name です。
    /// </param>
    /// <param name="args">EN:  JA: args パラメーターです。
    /// [EN] Optional process options.
    /// [JA] 任意の process option です。
    /// </param>
    /// <returns>EN:  JA: 結果を返します。
    /// [EN] Process result.
    /// [JA] process result です。
    /// </returns>
    Task<Result<IProcess>> TryCreateProcessAsync(string name, object? args = null);

    /// <summary>
    /// [EN] Safely starts a named WASM process.
    /// [JA] 指定した WASM process を安全に開始します。
    /// </summary>
    /// <param name="processName">EN:  JA: processName パラメーターです。
    /// [EN] Process name.
    /// [JA] process name です。
    /// </param>
    /// <param name="cancellationToken">EN:  JA: cancellationToken パラメーターです。
    /// [EN] Cancellation token.
    /// [JA] cancellation token です。
    /// </param>
    /// <returns>EN:  JA: 結果を返します。
    /// [EN] Start result.
    /// [JA] start result です。
    /// </returns>
    Task<Result<bool>> TryStartAsync(string processName, CancellationToken cancellationToken = default);

    /// <summary>
    /// [EN] Returns currently created WASM processes.
    /// [JA] 現在作成済みの WASM process を返します。
    /// </summary>
    /// <returns>EN:  JA: 結果を返します。
    /// [EN] WASM process list.
    /// [JA] WASM process list です。
    /// </returns>
    IReadOnlyList<WasmProcess> ListProcesses();
}
