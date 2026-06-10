using AIKernel.Abstractions.Providers;
using AIKernel.Wasm.Runtime;

namespace AIKernel.Wasm.Tests;

public sealed class WasmRuntimeContractTests
{
    [Fact]
    public async Task WasmProviders_ExposeProviderLifecycle()
    {
        IProvider[] providers =
        [
            new WasmRuntime(),
            new WasmProcessProvider(),
            new WasmMemoryProvider(),
            new WasmStdinProvider(),
            new WasmFileSystemProvider(),
            new WasmEventProvider(),
            new WasmAudioProvider(),
            new WasmScreenshotProvider(),
            new WasmSaveStateProvider(),
            new WasmTimeProvider()
        ];

        foreach (var provider in providers)
        {
            Assert.False(await provider.IsAvailableAsync());
            await provider.InitializeAsync();
            Assert.True(await provider.IsAvailableAsync());
            Assert.NotEmpty(provider.GetCapabilities().SupportedOperations);
            await provider.ShutdownAsync();
            Assert.False(await provider.IsAvailableAsync());
        }
    }

}
