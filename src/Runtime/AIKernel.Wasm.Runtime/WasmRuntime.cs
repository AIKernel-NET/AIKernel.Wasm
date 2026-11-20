namespace AIKernel.Wasm.Runtime;

using AIKernel.Abstractions.Events;
using AIKernel.Abstractions.Processes;
using AIKernel.Common.Results;
using AIKernel.Wasm.Runtime.Abstractions;

/// <summary>
/// [EN] AIKernel browser/WebAssembly runtime boundary.
/// [JA] AIKernel browser / WebAssembly runtime 境界です。
/// </summary>
public sealed class WasmRuntime : WasmProviderBase, IWasmProcessRuntime, IDisposable
{
    private readonly IEventBus? _eventBus;
    private readonly HashSet<WasmRuntimeContext> _contexts = [];
    private bool _disposed;

    /// <summary>[EN] Initializes the WASM runtime. [JA] WASM runtime を初期化します。</summary>
    public WasmRuntime(IEventBus? eventBus = null)
        : base("wasm.runtime", "WASM Runtime", ["wasm.runtime.boot", "wasm.runtime.shutdown"], ["runtime"])
    {
        _eventBus = eventBus;
    }

    /// <summary>[EN] Boots the WASM runtime. [JA] WASM runtime を boot します。</summary>
    public Task BootAsync(CancellationToken cancellationToken = default)
        => RequireSuccessAsync(TryBootAsync(cancellationToken));

    /// <summary>[EN] Documents this public package API member. [JA] TryBootAsync を実行します。</summary>
    /// <inheritdoc />
    public Task<Result<bool>> TryBootAsync(CancellationToken cancellationToken = default)
    {
        return cancellationToken.IsCancellationRequested
            ? Result<bool>.Fail("WASM runtime boot was cancelled. ErrorCode=WASM_RUNTIME_BOOT_CANCELLED").AsTask()
            : Result<bool>.Success(true).AsTask();
    }

    /// <summary>[EN] Creates a WASM runtime context. [JA] WASM runtime context を作成します。</summary>
    public WasmRuntimeContext CreateContext(int initialMemoryBytes = 65536)
        => RequireSuccess(TryCreateContext(initialMemoryBytes));

    /// <summary>[EN] Documents this public package API member. [JA] TryCreateContext を実行します。</summary>
    /// <inheritdoc />
    public Result<WasmRuntimeContext> TryCreateContext(int initialMemoryBytes = 65536)
    {
        return Try.Run(() =>
        {
            lock (_contexts)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                var context = new WasmRuntimeContext(initialMemoryBytes, _eventBus);
                context.Released += Release;
                _contexts.Add(context);
                return context;
            }
        });
    }

    /// <summary>[EN] Creates a WASM process from optional process options. [JA] 任意の process option から WASM process を作成します。</summary>
    public async Task<IProcess> CreateProcessAsync(string name, object? args = null)
        => RequireSuccess(await TryCreateProcessAsync(name, args).ConfigureAwait(false));

    /// <summary>[EN] Documents this public package API member. [JA] TryCreateProcessAsync を実行します。</summary>
    /// <inheritdoc />
    public Task<Result<IProcess>> TryCreateProcessAsync(string name, object? args = null)
    {
        var options = args as WasmProcessOptions ?? new WasmProcessOptions();
        var moduleBytes = options.ModuleBytes ?? [];

        return
            from header in WasmRuntimeContext.ValidateModuleBytes(moduleBytes).AsTask()
            from context in TryCreateContext(options.InitialMemoryBytes).AsTask()
            from loaded in context.TryLoadModuleAsync(moduleBytes)
            select (IProcess)new WasmProcess(name, context, _eventBus);
    }

    /// <summary>[EN] Disposes all runtime contexts. [JA] すべての runtime context を dispose します。</summary>
    public void Dispose()
    {
        WasmRuntimeContext[] contexts;
        lock (_contexts)
        {
            _disposed = true;
            contexts = _contexts.ToArray();
            _contexts.Clear();
        }
        foreach (var context in contexts) context.Dispose();
    }

    private void Release(WasmRuntimeContext context)
    {
        lock (_contexts) _contexts.Remove(context);
    }

    private static T RequireSuccess<T>(Result<T> result)
        => result.Match(
            error => throw new InvalidOperationException(error.Message),
            value => value);

    private static async Task RequireSuccessAsync(Task<Result<bool>> task)
    {
        var result = await task.ConfigureAwait(false);
        result.Match(
            error => throw new InvalidOperationException(error.Message),
            value => value);
    }
}
