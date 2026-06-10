namespace AIKernel.Wasm.Tests;

using AIKernel.Abstractions.Processes;
using AIKernel.Providers.Standard.EventBus;
using AIKernel.Wasm.Runtime;

/// <summary>
/// [EN] Tests WASM process integration with Core process abstractions and EventBus.
/// [JA] Core process 抽象および EventBus と WASM process の統合を検証します。
/// </summary>
public sealed class WasmProcessProviderTests
{
    /// <summary>
    /// [EN] Verifies start and stop state transitions publish OS events.
    /// [JA] start / stop の state transition が OS event を publish することを検証します。
    /// </summary>
    [Fact]
    public async Task ProcessStartAndStop_PublishEvents()
    {
        var eventBus = new EventBusProvider();
        var events = new List<string>();
        eventBus.Subscribe<Dictionary<string, string>>("ProcessStarted", payload =>
        {
            events.Add($"start:{payload["processName"]}:{payload["state"]}");
            return Task.CompletedTask;
        });
        eventBus.Subscribe<Dictionary<string, string>>("ProcessStopped", payload =>
        {
            events.Add($"stop:{payload["processName"]}:{payload["state"]}");
            return Task.CompletedTask;
        });

        var provider = new WasmProcessProvider(eventBus);
        var process = await provider.CreateProcessAsync("sample");

        Assert.Equal(ProcessState.Starting, process.State);
        await process.StartAsync();
        Assert.Equal(ProcessState.Running, process.State);
        await process.StopAsync();
        Assert.Equal(ProcessState.Stopped, process.State);

        Assert.Equal(["start:sample:Running", "stop:sample:Stopped"], events);
    }

    /// <summary>
    /// [EN] Verifies crash handling publishes ProcessCrashed.
    /// [JA] crash handling が ProcessCrashed を publish することを検証します。
    /// </summary>
    [Fact]
    public async Task CrashHandling_PublishesProcessCrashed()
    {
        var eventBus = new EventBusProvider();
        var reason = "";
        eventBus.Subscribe<Dictionary<string, string>>("ProcessCrashed", payload =>
        {
            reason = payload["reason"];
            return Task.CompletedTask;
        });

        var provider = new WasmProcessProvider(eventBus);
        var process = Assert.IsType<WasmProcess>(await provider.CreateProcessAsync("sample"));

        await process.CrashAsync("runtime trap");

        Assert.Equal(ProcessState.Error, process.State);
        Assert.Equal("runtime trap", reason);
    }
}
