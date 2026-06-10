using AIKernel.Abstractions.Processes;
using AIKernel.Common.Results;

namespace AIKernel.Wasm.Runtime.Abstractions;

/// <summary>
/// [EN] Stable WASM runtime contract for AIKernel browser and WebAssembly hosts.
/// [JA] AIKernel browser / WebAssembly host 向けの安定した WASM runtime contract です。
/// </summary>
public interface IWasmRuntime : IProcessHost
{
    /// <summary>
    /// [EN] Safely boots the runtime.
    /// [JA] runtime を安全に boot します。
    /// </summary>
    /// <param name="cancellationToken">
    /// [EN] Cancellation token.
    /// [JA] cancellation token です。
    /// </param>
    /// <returns>
    /// [EN] Boot result.
    /// [JA] boot result です。
    /// </returns>
    Task<Result<bool>> TryBootAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// [EN] Safely creates a runtime context.
    /// [JA] runtime context を安全に作成します。
    /// </summary>
    /// <param name="initialMemoryBytes">
    /// [EN] Initial linear memory size.
    /// [JA] 初期 linear memory size です。
    /// </param>
    /// <returns>
    /// [EN] Runtime context result.
    /// [JA] runtime context result です。
    /// </returns>
    Result<WasmRuntimeContext> TryCreateContext(int initialMemoryBytes = 65536);

    /// <summary>
    /// [EN] Safely creates a WASM process.
    /// [JA] WASM process を安全に作成します。
    /// </summary>
    /// <param name="name">
    /// [EN] Process name.
    /// [JA] process name です。
    /// </param>
    /// <param name="args">
    /// [EN] Optional process options.
    /// [JA] 任意の process option です。
    /// </param>
    /// <returns>
    /// [EN] Process result.
    /// [JA] process result です。
    /// </returns>
    Task<Result<IProcess>> TryCreateProcessAsync(string name, object? args = null);
}
