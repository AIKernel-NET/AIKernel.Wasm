namespace AIKernel.Wasm.Runtime;

using AIKernel.Abstractions.Events;
using AIKernel.Abstractions.Processes;
using AIKernel.Common.Results;
using AIKernel.Wasm.Runtime.Abstractions;

/// <summary>
/// [EN] WASM process implementation backed by a runtime context.
/// [JA] runtime context に支えられた WASM process implementation です。
/// </summary>
public sealed class WasmProcess : IProcess
{
    private readonly WasmRuntimeContext _context;
    private readonly IEventBus? _eventBus;

    /// <summary>[EN] Initializes a WASM process handle. [JA] WASM process handle を初期化します。</summary>
    public WasmProcess(string name, WasmRuntimeContext? context = null, IEventBus? eventBus = null)
    {
        Name = NormalizeProcessName(name);
        _context = context ?? new WasmRuntimeContext(eventBus: eventBus);
        _eventBus = eventBus;
    }

    /// <summary>[EN] Process name. [JA] process 名です。</summary>
    public string Name { get; }

    /// <inheritdoc />
    public ProcessId Id { get; } = new(Guid.NewGuid());

    /// <inheritdoc />
    public ProcessState State { get; private set; } = ProcessState.Starting;

    /// <inheritdoc />
    public Task StartAsync()
    {
        return RequireSuccessAsync(TryStartAsync());
    }

    /// <inheritdoc />
    public Task StopAsync()
    {
        return RequireSuccessAsync(TryStopAsync());
    }

    /// <summary>[EN] Marks this process as failed and publishes a crash event. [JA] この process を失敗として mark し crash event を発行します。</summary>
    public Task CrashAsync(string reason)
    {
        State = ProcessState.Error;
        return PublishAsync("ProcessCrashed", reason);
    }

    /// <summary>[EN] Safely starts this WASM process as a Result. [JA] この WASM process を Result として安全に開始します。</summary>
    public Task<Result<bool>> TryStartAsync()
    {
        State = ProcessState.Starting;
        return
            from loaded in EnsureModuleLoadedAsync()
            from running in _context.TryRunAsync()
            from state in Result<bool>.Success(SetState(ProcessState.Running)).AsTask()
            from published in PublishResultAsync("ProcessStarted")
            select state;
    }

    /// <summary>[EN] Safely stops this WASM process as a Result. [JA] この WASM process を Result として安全に停止します。</summary>
    public Task<Result<bool>> TryStopAsync()
    {
        return
            from stopped in _context.TryStopAsync()
            from state in Result<bool>.Success(SetState(ProcessState.Stopped)).AsTask()
            from published in PublishResultAsync("ProcessStopped")
            select state;
    }

    /// <summary>[EN] Runtime context backing this process. [JA] この process を支える runtime context です。</summary>
    public WasmRuntimeContext Context => _context;

    private Task<Result<bool>> EnsureModuleLoadedAsync()
        => ModuleLoadDecision(_context.IsModuleLoaded)
            .Match(
                _ => _context.TryLoadModuleAsync(new byte[] { 0x00 }),
                loaded => Result<bool>.Success(loaded).AsTask());

    private static string NormalizeProcessName(string name)
        => MonadicDecision.TextOrDefault(name, "wasm-process");

    private static Either<string, bool> ModuleLoadDecision(bool isModuleLoaded)
        => isModuleLoaded
            ? Either<string, bool>.FromRight(true)
            : Either<string, bool>.FromLeft("load-default-module");

    private bool SetState(ProcessState state)
    {
        State = state;
        return true;
    }

    private Task<Result<bool>> PublishResultAsync(string eventName, string? reason = null)
        => Try.RunAsync(async () =>
        {
            await PublishAsync(eventName, reason).ConfigureAwait(false);
            return true;
        });

    private Task PublishAsync(string eventName, string? reason = null)
        => _eventBus is null
            ? Task.CompletedTask
            : _eventBus.PublishAsync(
                eventName,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["processId"] = Id.Value.ToString("D"),
                    ["processName"] = Name,
                    ["state"] = State.ToString(),
                    ["reason"] = reason ?? string.Empty
                });

    private static async Task RequireSuccessAsync(Task<Result<bool>> task)
    {
        var result = await task.ConfigureAwait(false);
        result.Match(
            error => throw new InvalidOperationException(error.Message),
            value => value);
    }
}

/// <summary>[EN] Provides WASM process lifecycle services. [JA] WASM process lifecycle service を提供します。</summary>
public sealed class WasmProcessProvider(IEventBus? eventBus = null)
    : WasmProviderBase("wasm.process", "WASM Process Provider", ["wasm.process.start", "wasm.process.stop"], ["process"]),
      IWasmProcessProvider
{
    private readonly List<WasmProcess> _processes = [];

    /// <summary>[EN] Creates a WASM process handle. [JA] WASM process handle を作成します。</summary>
    public async Task<IProcess> CreateProcessAsync(string name, object? args = null)
        => RequireSuccess(await TryCreateProcessAsync(name, args).ConfigureAwait(false));

    /// <summary>[EN] Safely creates a WASM process handle as a Result. [JA] WASM process handle を Result として安全に作成します。</summary>
    public Task<Result<IProcess>> TryCreateProcessAsync(string name, object? args = null)
    {
        var options = args as WasmProcessOptions ?? new WasmProcessOptions();
        var moduleBytes = options.ModuleBytes is { Length: > 0 }
            ? options.ModuleBytes
            : new byte[] { 0x00 };

        return
            from context in Try.Run(() => new WasmRuntimeContext(options.InitialMemoryBytes, eventBus)).AsTask()
            from loaded in context.TryLoadModuleAsync(moduleBytes)
            select AddProcess(name, context);
    }

    /// <summary>[EN] Starts a WASM process. [JA] WASM process を開始します。</summary>
    public Task StartAsync(string processName, CancellationToken cancellationToken = default)
        => RequireSuccessAsync(TryStartAsync(processName, cancellationToken));

    /// <summary>[EN] Safely starts a WASM process as a Result. [JA] WASM process を Result として安全に開始します。</summary>
    public Task<Result<bool>> TryStartAsync(string processName, CancellationToken cancellationToken = default)
    {
        return
            from cancellable in ValidateNotCancelled(cancellationToken).AsTask()
            from process in ProcessOptionToResult(FindProcess(processName), processName).AsTask()
            from started in process.TryStartAsync()
            select started;
    }

    /// <summary>[EN] Returns currently created WASM process handles. [JA] 現在作成済みの WASM process handle を返します。</summary>
    public IReadOnlyList<WasmProcess> ListProcesses()
        => _processes.ToArray();

    private IProcess AddProcess(string name, WasmRuntimeContext context)
    {
        var process = new WasmProcess(name, context, eventBus);
        _processes.Add(process);
        return process;
    }

    private Option<WasmProcess> FindProcess(string processName)
    {
        var process = _processes.FirstOrDefault(item => string.Equals(item.Name, processName, StringComparison.Ordinal));
        return MonadicDecision.Optional(process);
    }

    private static Result<WasmProcess> ProcessOptionToResult(Option<WasmProcess> process, string processName)
        => process.Match(
            () => Result<WasmProcess>.Fail($"WASM process not found: {processName}. ErrorCode=WASM_PROCESS_NOT_FOUND"),
            Result<WasmProcess>.Success);

    private static Result<bool> ValidateNotCancelled(CancellationToken cancellationToken)
        => cancellationToken.IsCancellationRequested
            ? Result<bool>.Fail("WASM process operation was cancelled. ErrorCode=WASM_PROCESS_CANCELLED")
            : Result<bool>.Success(true);

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

/// <summary>[EN] Provides checked WASM linear-memory services. [JA] 範囲 check 付き WASM linear memory service を提供します。</summary>
public sealed class WasmMemoryProvider(WasmRuntimeContext? context = null)
    : WasmProviderBase("wasm.memory", "WASM Memory Provider", ["wasm.memory.allocate", "wasm.memory.free"], ["memory"])
{
    /// <summary>[EN] Allocates WASM memory. [JA] WASM memory を確保します。</summary>
    private readonly WasmRuntimeContext _context = context ?? new WasmRuntimeContext();

    /// <summary>[EN] Allocates WASM memory. [JA] WASM memory を確保します。</summary>
    public ValueTask<int> AllocateAsync(int byteCount, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (byteCount < 0 || byteCount > _context.MemorySize)
        {
            throw new ArgumentOutOfRangeException(nameof(byteCount));
        }

        return ValueTask.FromResult(0);
    }

    /// <summary>[EN] Reads linear memory. [JA] linear memory を読み取ります。</summary>
    public byte[] Read(int offset, int length) => _context.ReadMemory(offset, length);

    /// <summary>[EN] Writes linear memory. [JA] linear memory に書き込みます。</summary>
    public void Write(int offset, ReadOnlyMemory<byte> data) => _context.WriteMemory(offset, data.Span);

    /// <summary>[EN] Reads one byte from linear memory. [JA] linear memory から 1 byte を読み取ります。</summary>
    public byte ReadByte(int offset) => _context.ReadByte(offset);

    /// <summary>[EN] Writes one byte to linear memory. [JA] linear memory に 1 byte を書き込みます。</summary>
    public void WriteByte(int offset, byte value) => _context.WriteByte(offset, value);
}

/// <summary>[EN] Provides WASM stdin forwarding services. [JA] WASM stdin forwarding service を提供します。</summary>
public sealed class WasmStdinProvider(WasmRuntimeContext? context = null)
    : WasmProviderBase("wasm.stdin", "WASM Stdin Provider", ["wasm.stdin.write"], ["stdin", "text"])
{
    /// <summary>[EN] Writes stdin text to a WASM process. [JA] WASM process へ stdin text を書き込みます。</summary>
    private readonly WasmRuntimeContext _context = context ?? new WasmRuntimeContext();

    /// <summary>[EN] Writes stdin text to a WASM process. [JA] WASM process へ stdin text を書き込みます。</summary>
    public Task WriteLineAsync(string line, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _context.SendStdin(line);
        return Task.CompletedTask;
    }
}

/// <summary>[EN] Provides deterministic WASI-style file system services. [JA] deterministic な WASI style file system service を提供します。</summary>
public sealed class WasmFileSystemProvider(WasmRuntimeContext? context = null)
    : WasmProviderBase("wasm.filesystem", "WASM File System Provider", ["wasm.fs.mount", "wasm.fs.read", "wasm.fs.write"], ["file", "directory"])
{
    /// <summary>[EN] Mounts a WASM file system root. [JA] WASM file system root を mount します。</summary>
    private readonly WasmRuntimeContext _context = context ?? new WasmRuntimeContext();

    /// <summary>[EN] Mounts a WASM file system root. [JA] WASM file system root を mount します。</summary>
    public Task MountAsync(string mountPoint, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentException.ThrowIfNullOrWhiteSpace(mountPoint);
        return Task.CompletedTask;
    }

    /// <summary>[EN] Writes a WASI-style file. [JA] WASI style file を書き込みます。</summary>
    public void WriteFile(string path, ReadOnlyMemory<byte> content) => _context.WriteFile(path, content);

    /// <summary>[EN] Reads a WASI-style file. [JA] WASI style file を読み取ります。</summary>
    public byte[] ReadFile(string path) => _context.ReadFile(path);

    /// <summary>[EN] Lists WASI-style files. [JA] WASI style file を列挙します。</summary>
    public IReadOnlyList<string> ListFiles() => _context.ListFiles();
}

/// <summary>[EN] Provides WASM-to-EventBus event bridge services. [JA] WASM から EventBus への event bridge service を提供します。</summary>
public sealed class WasmEventProvider(WasmRuntimeContext? context = null)
    : WasmProviderBase("wasm.event", "WASM Event Provider", ["wasm.event.publish", "wasm.event.subscribe"], ["event"])
{
    /// <summary>[EN] Publishes a WASM event. [JA] WASM event を発行します。</summary>
    private readonly WasmRuntimeContext _context = context ?? new WasmRuntimeContext();

    /// <summary>[EN] Publishes a WASM event. [JA] WASM event を発行します。</summary>
    public Task PublishAsync(string eventName, object payload, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _context.PublishEvent(eventName, payload);
        return Task.CompletedTask;
    }
}

/// <summary>[EN] Provides WASM audio buffer output services. [JA] WASM audio buffer output service を提供します。</summary>
public sealed class WasmAudioProvider(WasmRuntimeContext? context = null)
    : WasmProviderBase("wasm.audio", "WASM Audio Provider", ["wasm.audio.play", "wasm.audio.stop"], ["audio"])
{
    /// <summary>[EN] Plays audio bytes. [JA] audio byte を再生します。</summary>
    private readonly WasmRuntimeContext _context = context ?? new WasmRuntimeContext();

    /// <summary>[EN] Plays audio bytes. [JA] audio byte を再生します。</summary>
    public Task PlayAsync(ReadOnlyMemory<byte> audio, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _context.SetAudioBuffer(audio);
        return Task.CompletedTask;
    }

    /// <summary>[EN] Returns the latest audio buffer. [JA] 最新 audio buffer を返します。</summary>
    public byte[] LatestAudioBuffer() => _context.GetAudioBuffer();
}

/// <summary>[EN] Provides WASM framebuffer screenshot capture services. [JA] WASM framebuffer screenshot capture service を提供します。</summary>
public sealed class WasmScreenshotProvider(WasmRuntimeContext? context = null)
    : WasmProviderBase("wasm.screenshot", "WASM Screenshot Provider", ["wasm.screenshot.capture"], ["image"])
{
    /// <summary>[EN] Captures a screenshot. [JA] screenshot を取得します。</summary>
    private readonly WasmRuntimeContext _context = context ?? new WasmRuntimeContext();

    /// <summary>[EN] Captures a screenshot. [JA] screenshot を取得します。</summary>
    public ValueTask<byte[]> CaptureAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(_context.GetFramebuffer());
    }

    /// <summary>[EN] Sets framebuffer bytes for capture. [JA] capture 用 framebuffer byte を設定します。</summary>
    public void SetFramebuffer(ReadOnlyMemory<byte> framebuffer) => _context.SetFramebuffer(framebuffer);
}

/// <summary>[EN] Provides WASM linear-memory save-state services. [JA] WASM linear memory save-state service を提供します。</summary>
public sealed class WasmSaveStateProvider(WasmRuntimeContext? context = null)
    : WasmProviderBase("wasm.savestate", "WASM Save State Provider", ["wasm.savestate.save", "wasm.savestate.load"], ["savestate"])
{
    /// <summary>[EN] Saves runtime state. [JA] runtime state を保存します。</summary>
    private readonly WasmRuntimeContext _context = context ?? new WasmRuntimeContext();

    /// <summary>[EN] Saves runtime state. [JA] runtime state を保存します。</summary>
    public ValueTask<byte[]> SaveAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(_context.SaveState());
    }

    /// <summary>[EN] Restores runtime state. [JA] runtime state を復元します。</summary>
    public ValueTask RestoreAsync(ReadOnlyMemory<byte> state, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _context.RestoreState(state);
        return ValueTask.CompletedTask;
    }
}

/// <summary>[EN] Provides deterministic WASM time-control services. [JA] deterministic な WASM time-control service を提供します。</summary>
public sealed class WasmTimeProvider(WasmRuntimeContext? context = null)
    : WasmProviderBase("wasm.time", "WASM Time Provider", ["wasm.time.now", "wasm.time.tick"], ["time"])
{
    /// <summary>[EN] Returns the current WASM runtime time. [JA] 現在の WASM runtime time を返します。</summary>
    private readonly WasmRuntimeContext _context = context ?? new WasmRuntimeContext();

    /// <summary>[EN] Returns the current WASM runtime time. [JA] 現在の WASM runtime time を返します。</summary>
    public DateTimeOffset Now()
    {
        return _context.Clock;
    }

    /// <summary>[EN] Advances WASM runtime time. [JA] WASM runtime time を進めます。</summary>
    public void Advance(TimeSpan delta) => _context.AdvanceTime(delta);

    /// <summary>[EN] Sets WASM runtime time scale. [JA] WASM runtime time scale を設定します。</summary>
    public void SetScale(double scale) => _context.SetTimeScale(scale);

    /// <summary>[EN] Pauses WASM runtime time. [JA] WASM runtime time を pause します。</summary>
    public void Pause() => _context.PauseTime();

    /// <summary>[EN] Resumes WASM runtime time. [JA] WASM runtime time を resume します。</summary>
    public void Resume() => _context.ResumeTime();
}
