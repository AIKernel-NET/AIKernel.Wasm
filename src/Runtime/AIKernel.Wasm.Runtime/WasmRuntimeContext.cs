namespace AIKernel.Wasm.Runtime;

using AIKernel.Abstractions.Events;
using AIKernel.Abstractions.Processes;
using AIKernel.Common.Results;

/// <summary>
/// [EN] In-memory WASM runtime context that models module, memory, imports, exports, and lifecycle state.
/// [JA] module、memory、import、export、lifecycle state を model 化する in-memory WASM runtime context です。
/// </summary>
public sealed class WasmRuntimeContext : IDisposable
{
    private readonly IEventBus? _eventBus;
    private readonly Dictionary<string, Func<IReadOnlyList<object?>, object?>> _exports = new(StringComparer.Ordinal);
    private readonly Dictionary<string, object> _imports = new(StringComparer.Ordinal);
    private readonly Dictionary<string, byte[]> _files = new(StringComparer.Ordinal);
    private readonly List<string> _stdin = [];
    private byte[] _memory;
    private byte[] _framebuffer = [];
    private byte[] _audioBuffer = [];
    private DateTimeOffset _clock;
    private double _timeScale = 1.0;
    private bool _paused;
    private bool _disposed;

    /// <summary>
    /// [EN] Creates a runtime context with deterministic initial memory and optional EventBus.
    /// [JA] 決定論的な初期 memory と任意の EventBus で runtime context を作成します。
    /// </summary>
    public WasmRuntimeContext(int initialMemoryBytes = 65536, IEventBus? eventBus = null)
    {
        RequireSuccess(ValidateInitialMemory(initialMemoryBytes));

        _memory = new byte[initialMemoryBytes];
        _eventBus = eventBus;
        _clock = DateTimeOffset.UnixEpoch;
    }

    /// <summary>[EN] Returns whether a module is loaded. [JA] module が load 済みかどうかを返します。</summary>
    public bool IsModuleLoaded { get; private set; }

    /// <summary>[EN] Returns whether the runtime instance is running. [JA] runtime instance が running かどうかを返します。</summary>
    public bool IsRunning { get; private set; }

    /// <summary>[EN] Returns the current linear memory size. [JA] 現在の linear memory size を返します。</summary>
    public int MemorySize => _memory.Length;

    /// <summary>[EN] Returns the current deterministic clock value. [JA] 現在の deterministic clock 値を返します。</summary>
    public DateTimeOffset Clock => _clock;

    /// <summary>[EN] Returns the current time scale. [JA] 現在の time scale を返します。</summary>
    public double TimeScale => _timeScale;

    /// <summary>[EN] Returns whether the runtime clock is paused. [JA] runtime clock が paused かどうかを返します。</summary>
    public bool IsPaused => _paused;

    /// <summary>[EN] Loads a WASM module image into the context. [JA] WASM module image を context に load します。</summary>
    public Task LoadModuleAsync(ReadOnlyMemory<byte> moduleBytes, CancellationToken cancellationToken = default)
        => RequireSuccessAsync(TryLoadModuleAsync(moduleBytes, cancellationToken));

    /// <summary>[EN] Safely loads a WASM module image as a Result. [JA] WASM module image を Result として安全に load します。</summary>
    public Task<Result<bool>> TryLoadModuleAsync(ReadOnlyMemory<byte> moduleBytes, CancellationToken cancellationToken = default)
    {
        return
            from available in ValidateNotDisposed().AsTask()
            from cancellable in ValidateNotCancelled(cancellationToken).AsTask()
            from module in ValidateModuleBytes(moduleBytes).AsTask()
            from loaded in Try.RunAsync(() =>
            {
                IsModuleLoaded = true;
                return Task.FromResult(true);
            })
            select loaded;
    }

    /// <summary>[EN] Safely registers a deterministic import object as a Result. [JA] deterministic import object を Result として安全に登録します。</summary>
    public Result<bool> TryRegisterImport(string name, object? import)
        =>
            from available in ValidateNotDisposed()
            from validName in ValidateName(name, "import")
            from validImport in ValidateObject(import, "import")
            select RegisterImportCore(validName, validImport);

    /// <summary>[EN] Safely registers a deterministic exported function as a Result. [JA] deterministic exported function を Result として安全に登録します。</summary>
    public Result<bool> TryRegisterExport(string name, Func<IReadOnlyList<object?>, object?>? function)
        =>
            from available in ValidateNotDisposed()
            from validName in ValidateName(name, "export")
            from validFunction in ValidateFunction(function)
            select RegisterExportCore(validName, validFunction);

    /// <summary>[EN] Safely resolves an exported function by name. [JA] 名前で exported function を安全に解決します。</summary>
    public Result<Func<IReadOnlyList<object?>, object?>> TryResolveExport(string name)
        =>
            from available in ValidateNotDisposed()
            from validName in ValidateName(name, "export")
            from function in _exports.TryGetValue(validName, out var export)
                ? Result<Func<IReadOnlyList<object?>, object?>>.Success(export)
                : Result<Func<IReadOnlyList<object?>, object?>>.Fail($"WASM export not found: {validName}. ErrorCode=WASM_EXPORT_NOT_FOUND")
            select function;

    /// <summary>[EN] Safely starts the runtime instance as a Result. [JA] runtime instance を Result として安全に開始します。</summary>
    public Task<Result<bool>> TryRunAsync(CancellationToken cancellationToken = default)
    {
        return
            from available in ValidateNotDisposed().AsTask()
            from cancellable in ValidateNotCancelled(cancellationToken).AsTask()
            from loaded in ValidateModuleLoaded().AsTask()
            from running in Try.RunAsync(() =>
            {
                IsRunning = true;
                return Task.FromResult(true);
            })
            select running;
    }

    /// <summary>[EN] Safely stops the runtime instance as a Result. [JA] runtime instance を Result として安全に停止します。</summary>
    public Task<Result<bool>> TryStopAsync(CancellationToken cancellationToken = default)
    {
        return
            from available in ValidateNotDisposed().AsTask()
            from cancellable in ValidateNotCancelled(cancellationToken).AsTask()
            from stopped in Try.RunAsync(() =>
            {
                IsRunning = false;
                return Task.FromResult(true);
            })
            select stopped;
    }

    /// <summary>[EN] Safely reads linear memory with range checks. [JA] 範囲 check 付きで linear memory を安全に読み取ります。</summary>
    public Result<byte[]> TryReadMemory(int offset, int length)
        =>
            from range in ValidateRangeResult(offset, length)
            from data in Try.Run(() => _memory.AsSpan(offset, length).ToArray())
                .Tap(_ => Publish("MemoryAccessed", new Dictionary<string, string>
                {
                    ["operation"] = "read",
                    ["offset"] = offset.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["length"] = length.ToString(System.Globalization.CultureInfo.InvariantCulture)
                }))
            select data;

    /// <summary>[EN] Safely writes linear memory with range checks. [JA] 範囲 check 付きで linear memory に安全に書き込みます。</summary>
    public Result<bool> TryWriteMemory(int offset, ReadOnlyMemory<byte> data)
        =>
            from range in ValidateRangeResult(offset, data.Length)
            from written in Try.Run(() =>
            {
                data.Span.CopyTo(_memory.AsSpan(offset, data.Length));
                Publish("MemoryAccessed", new Dictionary<string, string>
                {
                    ["operation"] = "write",
                    ["offset"] = offset.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["length"] = data.Length.ToString(System.Globalization.CultureInfo.InvariantCulture)
                });
                return true;
            })
            select written;

    /// <summary>[EN] Safely reads a runtime file. [JA] runtime file を安全に読み取ります。</summary>
    public Result<byte[]> TryReadFile(string path)
        =>
            from available in ValidateNotDisposed()
            from validPath in ValidateName(path, "file path")
            from published in Try.Run(() =>
            {
                Publish("FileAccessed", new Dictionary<string, string> { ["operation"] = "read", ["path"] = validPath });
                return true;
            })
            from content in _files.TryGetValue(validPath, out var bytes)
                ? Result<byte[]>.Success(bytes.ToArray())
                : Result<byte[]>.Fail($"WASM runtime file not found: {validPath}. ErrorCode=WASM_FILE_NOT_FOUND")
            select content;

    /// <summary>[EN] Safely restores linear-memory state. [JA] linear-memory state を安全に復元します。</summary>
    public Result<bool> TryRestoreState(ReadOnlyMemory<byte> state)
        =>
            from validState in state.IsEmpty
                ? Result<ReadOnlyMemory<byte>>.Fail("Save state bytes are required. ErrorCode=WASM_SAVE_STATE_REQUIRED")
                : Result<ReadOnlyMemory<byte>>.Success(state)
            from restored in Try.Run(() =>
            {
                _memory = validState.ToArray();
                return true;
            })
            select restored;

    /// <summary>[EN] Safely sets deterministic runtime time scale. [JA] deterministic runtime time scale を安全に設定します。</summary>
    public Result<bool> TrySetTimeScale(double scale)
        =>
            from validScale in scale >= 0
                ? Result<double>.Success(scale)
                : Result<double>.Fail("Time scale cannot be negative. ErrorCode=WASM_TIME_SCALE_NEGATIVE")
            select SetTimeScaleCore(validScale);

    private static Result<bool> ValidateInitialMemory(int initialMemoryBytes)
        => initialMemoryBytes > 0
            ? Result<bool>.Success(true)
            : Result<bool>.Fail("Initial memory size must be positive. ErrorCode=WASM_INITIAL_MEMORY_INVALID");

    private Result<bool> ValidateNotDisposed()
        => !_disposed
            ? Result<bool>.Success(true)
            : Result<bool>.Fail("WASM runtime context is disposed. ErrorCode=WASM_CONTEXT_DISPOSED");

    private static Result<bool> ValidateNotCancelled(CancellationToken cancellationToken)
        => cancellationToken.IsCancellationRequested
            ? Result<bool>.Fail("WASM runtime operation was cancelled. ErrorCode=WASM_OPERATION_CANCELLED")
            : Result<bool>.Success(true);

    private static Result<ReadOnlyMemory<byte>> ValidateModuleBytes(ReadOnlyMemory<byte> moduleBytes)
        => moduleBytes.IsEmpty
            ? Result<ReadOnlyMemory<byte>>.Fail("WASM module bytes are required. ErrorCode=WASM_MODULE_BYTES_REQUIRED")
            : Result<ReadOnlyMemory<byte>>.Success(moduleBytes);

    private Result<bool> ValidateModuleLoaded()
        => IsModuleLoaded
            ? Result<bool>.Success(true)
            : Result<bool>.Fail("A WASM module must be loaded before running. ErrorCode=WASM_MODULE_NOT_LOADED");

    private static Result<string> ValidateName(string name, string label)
        => string.IsNullOrWhiteSpace(name)
            ? Result<string>.Fail($"WASM {label} is required. ErrorCode=WASM_NAME_REQUIRED")
            : Result<string>.Success(name);

    private static Result<object> ValidateObject(object? value, string label)
        => value is null
            ? Result<object>.Fail($"WASM {label} object is required. ErrorCode=WASM_OBJECT_REQUIRED")
            : Result<object>.Success(value);

    private static Result<Func<IReadOnlyList<object?>, object?>> ValidateFunction(Func<IReadOnlyList<object?>, object?>? function)
        => function is null
            ? Result<Func<IReadOnlyList<object?>, object?>>.Fail("WASM export function is required. ErrorCode=WASM_EXPORT_FUNCTION_REQUIRED")
            : Result<Func<IReadOnlyList<object?>, object?>>.Success(function);

    private bool RegisterImportCore(string name, object import)
    {
        _imports[name] = import;
        return true;
    }

    private bool RegisterExportCore(string name, Func<IReadOnlyList<object?>, object?> function)
    {
        _exports[name] = function;
        return true;
    }

    private bool SetTimeScaleCore(double scale)
    {
        _timeScale = scale;
        return true;
    }

    private static async Task RequireSuccessAsync(Task<Result<bool>> task)
    {
        var result = await task.ConfigureAwait(false);
        RequireSuccess(result);
    }

    private static T RequireSuccess<T>(Result<T> result)
        => result.Match(
            error => throw new InvalidOperationException(error.Message),
            value => value);

    /// <summary>[EN] Registers a deterministic import object. [JA] deterministic import object を登録します。</summary>
    public void RegisterImport(string name, object import)
    {
        RequireSuccess(TryRegisterImport(name, import));
    }

    /// <summary>[EN] Registers a deterministic exported function. [JA] deterministic exported function を登録します。</summary>
    public void RegisterExport(string name, Func<IReadOnlyList<object?>, object?> function)
    {
        RequireSuccess(TryRegisterExport(name, function));
    }

    /// <summary>[EN] Resolves an exported function by name. [JA] 名前で exported function を解決します。</summary>
    public Func<IReadOnlyList<object?>, object?> ResolveExport(string name)
        => RequireSuccess(TryResolveExport(name));

    /// <summary>[EN] Starts the runtime instance. [JA] runtime instance を開始します。</summary>
    public Task RunAsync(CancellationToken cancellationToken = default)
        => RequireSuccessAsync(TryRunAsync(cancellationToken));

    /// <summary>[EN] Stops the runtime instance. [JA] runtime instance を停止します。</summary>
    public Task StopAsync(CancellationToken cancellationToken = default)
        => RequireSuccessAsync(TryStopAsync(cancellationToken));

    /// <summary>[EN] Reads linear memory with range checks. [JA] 範囲 check 付きで linear memory を読み取ります。</summary>
    public byte[] ReadMemory(int offset, int length)
        => RequireSuccess(TryReadMemory(offset, length));

    /// <summary>[EN] Writes linear memory with range checks. [JA] 範囲 check 付きで linear memory に書き込みます。</summary>
    public void WriteMemory(int offset, ReadOnlySpan<byte> data)
        => RequireSuccess(TryWriteMemory(offset, data.ToArray()));

    /// <summary>[EN] Writes a single byte to linear memory. [JA] linear memory に 1 byte を書き込みます。</summary>
    public void WriteByte(int offset, byte value)
        => WriteMemory(offset, new byte[] { value });

    /// <summary>[EN] Reads a single byte from linear memory. [JA] linear memory から 1 byte を読み取ります。</summary>
    public byte ReadByte(int offset)
        => ReadMemory(offset, 1)[0];

    /// <summary>[EN] Sends a stdin line into the runtime. [JA] runtime に stdin line を送信します。</summary>
    public void SendStdin(string line)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _stdin.Add(line ?? string.Empty);
        if (_exports.TryGetValue("stdin", out var stdin))
        {
            stdin([line ?? string.Empty]);
        }

        Publish("StdinSent", new Dictionary<string, string> { ["line"] = line ?? string.Empty });
    }

    /// <summary>[EN] Returns stdin lines sent to the runtime. [JA] runtime へ送信された stdin line を返します。</summary>
    public IReadOnlyList<string> StdinLines()
        => _stdin.ToArray();

    /// <summary>[EN] Writes a runtime file. [JA] runtime file を書き込みます。</summary>
    public void WriteFile(string path, ReadOnlyMemory<byte> content)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _files[path] = content.ToArray();
        Publish("FileAccessed", new Dictionary<string, string> { ["operation"] = "write", ["path"] = path });
    }

    /// <summary>[EN] Reads a runtime file. [JA] runtime file を読み取ります。</summary>
    public byte[] ReadFile(string path)
        => RequireSuccess(TryReadFile(path));

    /// <summary>[EN] Lists runtime file paths. [JA] runtime file path を列挙します。</summary>
    public IReadOnlyList<string> ListFiles()
        => _files.Keys.Order(StringComparer.Ordinal).ToArray();

    /// <summary>[EN] Publishes a runtime event through EventBus. [JA] EventBus 経由で runtime event を publish します。</summary>
    public void PublishEvent(string eventName, object payload)
        => Publish(eventName, payload);

    /// <summary>[EN] Sets the latest audio buffer. [JA] 最新 audio buffer を設定します。</summary>
    public void SetAudioBuffer(ReadOnlyMemory<byte> audio)
        => _audioBuffer = audio.ToArray();

    /// <summary>[EN] Gets the latest audio buffer. [JA] 最新 audio buffer を取得します。</summary>
    public byte[] GetAudioBuffer()
        => _audioBuffer.ToArray();

    /// <summary>[EN] Sets the latest framebuffer. [JA] 最新 framebuffer を設定します。</summary>
    public void SetFramebuffer(ReadOnlyMemory<byte> framebuffer)
        => _framebuffer = framebuffer.ToArray();

    /// <summary>[EN] Gets the latest framebuffer. [JA] 最新 framebuffer を取得します。</summary>
    public byte[] GetFramebuffer()
        => _framebuffer.ToArray();

    /// <summary>[EN] Saves linear-memory state. [JA] linear-memory state を保存します。</summary>
    public byte[] SaveState()
        => _memory.ToArray();

    /// <summary>[EN] Restores linear-memory state. [JA] linear-memory state を復元します。</summary>
    public void RestoreState(ReadOnlyMemory<byte> state)
        => RequireSuccess(TryRestoreState(state));

    /// <summary>[EN] Advances deterministic runtime time. [JA] deterministic runtime time を進めます。</summary>
    public void AdvanceTime(TimeSpan delta)
    {
        if (!_paused)
        {
            _clock = _clock.Add(TimeSpan.FromTicks((long)(delta.Ticks * _timeScale)));
        }
    }

    /// <summary>[EN] Sets deterministic runtime time scale. [JA] deterministic runtime time scale を設定します。</summary>
    public void SetTimeScale(double scale)
        => RequireSuccess(TrySetTimeScale(scale));

    /// <summary>[EN] Pauses deterministic runtime time. [JA] deterministic runtime time を pause します。</summary>
    public void PauseTime() => _paused = true;

    /// <summary>[EN] Resumes deterministic runtime time. [JA] deterministic runtime time を resume します。</summary>
    public void ResumeTime() => _paused = false;

    /// <summary>[EN] Disposes runtime state. [JA] runtime state を dispose します。</summary>
    public void Dispose()
    {
        _disposed = true;
        IsRunning = false;
        _exports.Clear();
        _imports.Clear();
        _files.Clear();
        _stdin.Clear();
        _memory = [];
    }

    private void ValidateRange(int offset, int length)
    {
        RequireSuccess(ValidateRangeResult(offset, length));
    }

    private Result<bool> ValidateRangeResult(int offset, int length)
        =>
            from available in ValidateNotDisposed()
            from validRange in offset >= 0 && length >= 0 && offset <= _memory.Length - length
                ? Result<bool>.Success(true)
                : Result<bool>.Fail("WASM memory access is out of range. ErrorCode=WASM_MEMORY_RANGE_INVALID")
            select validRange;

    private void Publish(string eventName, object payload)
        => _eventBus?.PublishAsync(eventName, payload).GetAwaiter().GetResult();
}

/// <summary>
/// [EN] WASM process creation options.
/// [JA] WASM process 作成 option です。
/// </summary>
public sealed record WasmProcessOptions(byte[]? ModuleBytes = null, int InitialMemoryBytes = 65536);
