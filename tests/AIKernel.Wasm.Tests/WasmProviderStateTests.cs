namespace AIKernel.Wasm.Tests;

using AIKernel.Providers.Standard.EventBus;
using AIKernel.Wasm.Runtime;

/// <summary>
/// [EN] Tests WASM runtime state-oriented providers.
/// [JA] WASM runtime state-oriented Provider を検証します。
/// </summary>
public sealed class WasmProviderStateTests
{
    /// <summary>
    /// [EN] Verifies memory read/write and MemoryAccessed events.
    /// [JA] memory read/write と MemoryAccessed event を検証します。
    /// </summary>
    [Fact]
    public void MemoryProvider_ReadsWritesAndPublishesEvents()
    {
        var eventBus = new EventBusProvider();
        var accessCount = 0;
        eventBus.Subscribe<Dictionary<string, string>>("MemoryAccessed", _ =>
        {
            accessCount++;
            return Task.CompletedTask;
        });
        var context = new WasmRuntimeContext(eventBus: eventBus);
        var provider = new WasmMemoryProvider(context);

        provider.WriteByte(10, 42);

        Assert.Equal(42, provider.ReadByte(10));
        Assert.Equal(2, accessCount);
    }

    /// <summary>
    /// [EN] Verifies runtime memory failures are returned as Result values.
    /// [JA] runtime memory failure が Result 値として返ることを検証します。
    /// </summary>
    [Fact]
    public void RuntimeContext_TryReadMemoryFailsClosedForOutOfRange()
    {
        var context = new WasmRuntimeContext(initialMemoryBytes: 8);

        var result = context.TryReadMemory(7, 2);

        Assert.True(result.IsFailure);
        Assert.Contains("WASM_MEMORY_RANGE_INVALID", result.Error!.Message);
    }

    /// <summary>
    /// [EN] Verifies stdin dispatch and event publication.
    /// [JA] stdin dispatch と event publication を検証します。
    /// </summary>
    [Fact]
    public async Task StdinProvider_ForwardsLines()
    {
        var context = new WasmRuntimeContext();
        var provider = new WasmStdinProvider(context);

        await provider.WriteLineAsync("command", TestContext.Current.CancellationToken);

        Assert.Equal(["command"], context.StdinLines());
    }

    /// <summary>
    /// [EN] Verifies WASI-style file reads and writes.
    /// [JA] WASI style file の read/write を検証します。
    /// </summary>
    [Fact]
    public void FileSystemProvider_ReadsAndWritesFiles()
    {
        var context = new WasmRuntimeContext();
        var provider = new WasmFileSystemProvider(context);

        provider.WriteFile("/app/config.txt", "ok"u8.ToArray());

        Assert.Equal("ok"u8.ToArray(), provider.ReadFile("/app/config.txt"));
        Assert.Equal(["/app/config.txt"], provider.ListFiles());
    }

    /// <summary>
    /// [EN] Verifies screenshot and audio providers expose runtime buffers.
    /// [JA] screenshot と audio Provider が runtime buffer を公開することを検証します。
    /// </summary>
    [Fact]
    public async Task ScreenshotAndAudioProviders_ReturnRuntimeBuffers()
    {
        var context = new WasmRuntimeContext();
        var screenshot = new WasmScreenshotProvider(context);
        var audio = new WasmAudioProvider(context);

        screenshot.SetFramebuffer(new byte[] { 1, 2, 3 });
        await audio.PlayAsync(new byte[] { 4, 5 }, TestContext.Current.CancellationToken);

        Assert.Equal([1, 2, 3], await screenshot.CaptureAsync(TestContext.Current.CancellationToken));
        Assert.Equal([4, 5], audio.LatestAudioBuffer());
    }

    /// <summary>
    /// [EN] Verifies save-state snapshot and restore.
    /// [JA] save-state snapshot と restore を検証します。
    /// </summary>
    [Fact]
    public async Task SaveStateProvider_SnapshotsAndRestoresMemory()
    {
        var context = new WasmRuntimeContext();
        var memory = new WasmMemoryProvider(context);
        var saveState = new WasmSaveStateProvider(context);

        memory.WriteByte(0, 7);
        var snapshot = await saveState.SaveAsync(TestContext.Current.CancellationToken);
        memory.WriteByte(0, 9);
        await saveState.RestoreAsync(snapshot, TestContext.Current.CancellationToken);

        Assert.Equal(7, memory.ReadByte(0));
    }

    /// <summary>
    /// [EN] Verifies deterministic runtime clock control.
    /// [JA] deterministic runtime clock control を検証します。
    /// </summary>
    [Fact]
    public void TimeProvider_ControlsRuntimeClock()
    {
        var context = new WasmRuntimeContext();
        var time = new WasmTimeProvider(context);

        time.SetScale(2.0);
        time.Advance(TimeSpan.FromSeconds(1));
        time.Pause();
        time.Advance(TimeSpan.FromSeconds(1));
        time.Resume();

        Assert.Equal(DateTimeOffset.UnixEpoch.AddSeconds(2), time.Now());
    }
}
