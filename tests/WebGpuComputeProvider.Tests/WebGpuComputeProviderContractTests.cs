using AIKernel.Dtos.Capabilities;
using AIKernel.Enums;
using AIKernel.Abstractions.Compute;
using AIKernel.Providers.Standard.EventBus;
using AIKernel.Wasm.Comput;
using System.Runtime.InteropServices;

namespace AIKernel.Wasm.Tests;

public sealed class WebGpuComputeProviderContractTests
{
    [Fact]
    public void ToContract_ExposesWebGpuProviderOperations()
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["adapter_profile"] = "webgpu-browser",
            ["backend"] = "browser-webgpu",
            ["fallback"] = "webgpu-or-cpu",
            ["version"] = "0.1.0"
        };

        var contract = WebGpuComputeCapabilityContracts.ToContract(
            new WebGpuComputeCapabilityDescriptor(
                "providers.webgpu",
                "webgpu-browser",
                metadata));

        Assert.Equal("providers.webgpu", contract.CapabilityId);
        Assert.Equal("WebGPU Compute Provider", contract.Name);
        Assert.Equal(CapabilityModuleKind.NativeLibrary, contract.Kind);
        Assert.Equal(CapabilityInvocationMode.Direct, contract.InvocationMode);
        Assert.Equal("webgpu_dispatch", contract.EntryPoint);
        Assert.Equal(["compute.dispatch", "compute.vector_add"], contract.ProvidedOperations);
        Assert.Equal(["compute.execute", "buffer.read", "buffer.write"], contract.RequiredPermissions);
    }

    [Fact]
    public void Provider_DefaultIdentityMatchesWasmManifest()
    {
        var provider = new global::AIKernel.Wasm.Comput.WebGpuComputeProvider();

        Assert.Equal("webgpu.compute", provider.ProviderId);
        Assert.True(provider.GetCapabilities().SupportsOperation("compute.dispatch"));
    }

    [Fact]
    public async Task Provider_LifecycleAndCapabilitiesRemainContractPure()
    {
        var provider = new global::AIKernel.Wasm.Comput.WebGpuComputeProvider(new WebGpuComputeSettings
        {
            ProviderId = "providers.webgpu",
            ForceCpuFallback = true
        });

        Assert.True(provider.IsAvailable());
        Assert.True(await provider.IsAvailableAsync());
        await provider.InitializeAsync();

        Assert.True(await provider.IsAvailableAsync());
        Assert.True(provider.GetCapabilities().SupportsOperation("compute.dispatch"));
        Assert.True(provider.GetCapabilities().SupportsOperation("compute.vector_add"));
        Assert.Equal("providers.webgpu", provider.ToCapabilityDescriptor().CapabilityId);

        await provider.ShutdownAsync();
        Assert.True(provider.IsAvailable());
    }

    [Fact]
    public async Task CpuFallback_VectorAdd1000ElementsMatchesExpected()
    {
        const int count = 1000;
        var provider = new global::AIKernel.Wasm.Comput.WebGpuComputeProvider(new WebGpuComputeSettings
        {
            ForceCpuFallback = true
        });

        var left = Enumerable.Range(0, count).Select(x => (float)x).ToArray();
        var right = Enumerable.Range(0, count).Select(x => (float)(x * 2)).ToArray();
        var expected = left.Zip(right, static (a, b) => a + b).ToArray();

        using var a = await provider.CreateBufferAsync(count * sizeof(float));
        using var b = await provider.CreateBufferAsync(count * sizeof(float));
        using var output = await provider.CreateBufferAsync(count * sizeof(float));

        await provider.WriteBufferAsync(a, MemoryMarshal.AsBytes<float>(left.AsSpan()).ToArray());
        await provider.WriteBufferAsync(b, MemoryMarshal.AsBytes<float>(right.AsSpan()).ToArray());
        await provider.ExecuteKernelAsync(ComputeKernel.CreateVectorAdd(count), a, b, output);

        var actualBytes = new byte[count * sizeof(float)];
        await provider.ReadBufferAsync(output, actualBytes);
        var actual = MemoryMarshal.Cast<byte, float>(actualBytes).ToArray();

        Assert.Equal(expected, actual);
        Assert.True(provider.UsingCpuFallback);
    }

    [Fact]
    public async Task GpuRequested_UsesCpuFallbackWhenBackendUnavailableAndMatchesExpected()
    {
        const int count = 1000;
        var provider = new global::AIKernel.Wasm.Comput.WebGpuComputeProvider(new WebGpuComputeSettings
        {
            ForceCpuFallback = false
        });

        var left = Enumerable.Range(0, count).Select(x => (float)(x + 1)).ToArray();
        var right = Enumerable.Range(0, count).Select(x => (float)(x + 3)).ToArray();
        var expected = left.Zip(right, static (a, b) => a + b).ToArray();

        using var a = await provider.CreateBufferAsync(count * sizeof(float));
        using var b = await provider.CreateBufferAsync(count * sizeof(float));
        using var output = await provider.CreateBufferAsync(count * sizeof(float));

        await provider.WriteBufferAsync(a, MemoryMarshal.AsBytes<float>(left.AsSpan()).ToArray());
        await provider.WriteBufferAsync(b, MemoryMarshal.AsBytes<float>(right.AsSpan()).ToArray());
        await provider.ExecuteKernelAsync(ComputeKernel.CreateVectorAdd(count), a, b, output);

        var actualBytes = new byte[count * sizeof(float)];
        await provider.ReadBufferAsync(output, actualBytes);
        var actual = MemoryMarshal.Cast<byte, float>(actualBytes).ToArray();

        Assert.Equal(expected, actual);
        Assert.True(provider.UsingCpuFallback);
    }

    [Fact]
    public async Task WasmBackend_WhenInteropAvailable_ExecutesVectorAddPipeline()
    {
        const int count = 4;
        var provider = new global::AIKernel.Wasm.Comput.WebGpuComputeProvider(
            new WebGpuComputeSettings { ForceCpuFallback = false },
            new WebGpuWasmBackend(new FakeWebGpuJsInterop()));
        var left = new[] { 1.0f, 2.0f, 3.0f, 4.0f };
        var right = new[] { 10.0f, 20.0f, 30.0f, 40.0f };

        using var a = await provider.CreateBufferAsync(count * sizeof(float));
        using var b = await provider.CreateBufferAsync(count * sizeof(float));
        using var output = await provider.CreateBufferAsync(count * sizeof(float));

        await provider.WriteBufferAsync(a, MemoryMarshal.AsBytes<float>(left.AsSpan()).ToArray());
        await provider.WriteBufferAsync(b, MemoryMarshal.AsBytes<float>(right.AsSpan()).ToArray());
        await provider.ExecuteKernelAsync(ComputeKernel.CreateVectorAdd(count), a, b, output);

        var actualBytes = new byte[count * sizeof(float)];
        await provider.ReadBufferAsync(output, actualBytes);
        var actual = MemoryMarshal.Cast<byte, float>(actualBytes).ToArray();

        Assert.Equal([11.0f, 22.0f, 33.0f, 44.0f], actual);
        Assert.False(provider.UsingCpuFallback);
    }

    [Fact]
    public async Task KernelExecution_PublishesGpuKernelExecutedEvent()
    {
        var eventBus = new EventBusProvider();
        var backend = "";
        eventBus.Subscribe<Dictionary<string, string>>("GpuKernelExecuted", payload =>
        {
            backend = payload["backend"];
            return Task.CompletedTask;
        });

        var provider = new global::AIKernel.Wasm.Comput.WebGpuComputeProvider(
            new WebGpuComputeSettings { ForceCpuFallback = true },
            eventBus: eventBus);
        using var left = await provider.CreateBufferAsync(sizeof(float));
        using var right = await provider.CreateBufferAsync(sizeof(float));
        using var output = await provider.CreateBufferAsync(sizeof(float));

        await provider.WriteBufferAsync(left, MemoryMarshal.AsBytes<float>(new[] { 1.0f }.AsSpan()).ToArray());
        await provider.WriteBufferAsync(right, MemoryMarshal.AsBytes<float>(new[] { 2.0f }.AsSpan()).ToArray());
        await provider.ExecuteKernelAsync(ComputeKernel.CreateVectorAdd(1), left, right, output);

        Assert.Equal("cpu-fallback", backend);
    }

    [Fact]
    public async Task BufferSizeMismatch_Throws()
    {
        var provider = new global::AIKernel.Wasm.Comput.WebGpuComputeProvider();
        using var buffer = await provider.CreateBufferAsync(4);

        await Assert.ThrowsAsync<ArgumentException>(
            () => provider.WriteBufferAsync(buffer, new byte[8]));
    }

    [Fact]
    public async Task Invoker_RejectsUnsupportedOperationFailClosed()
    {
        var invoker = new WebGpuComputeInvoker();

        var result = await invoker.InvokeAsync(new CapabilityInvocationRequest(
            "invoke-1",
            "webgpu.compute",
            "unknown.operation",
            new Dictionary<string, string>(),
            null,
            "sha256:replay",
            new Dictionary<string, string>()),
            TestContext.Current.CancellationToken);

        Assert.False(result.Succeeded);
        Assert.Equal("WEBGPU_OPERATION_NOT_SUPPORTED", result.ErrorCode);
    }

    [Fact]
    public void ProviderManifest_IncludesCliSettings()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "webgpu.provider.json");

        Assert.True(File.Exists(path));

        var json = File.ReadAllText(path);
        Assert.Contains("\"cli\"", json);
        Assert.Contains("\"defaultOperation\": \"compute.vector_add\"", json);
        Assert.Contains("\"command\": \"gpu\"", json);
    }

    [Fact]
    public void Settings_MetadataIsDeterministicallyOrdered()
    {
        var settings = new WebGpuComputeSettings
        {
            ForceCpuFallback = true
        };

        Assert.Equal(
            ["adapter_profile", "backend", "fallback", "version"],
            settings.ToMetadata().Keys.ToArray());
    }

    private sealed class FakeWebGpuJsInterop : IWebGpuJsInterop
    {
        private readonly Dictionary<object, byte[]> _buffers = new();

        public bool IsWebGpuSupported() => true;

        public Task WriteBufferAsync(object? buffer, ReadOnlyMemory<byte> data)
        {
            _buffers[RequireBuffer(buffer)] = data.ToArray();
            return Task.CompletedTask;
        }

        public Task<byte[]> ReadBufferAsync(object? buffer, int length)
            => Task.FromResult(_buffers[RequireBuffer(buffer)].Take(length).ToArray());

        public Task ExecuteKernelAsync(string wgsl, IReadOnlyList<object?> buffers, int x, int y, int z)
        {
            var left = MemoryMarshal.Cast<byte, float>(_buffers[RequireBuffer(buffers[0])]);
            var right = MemoryMarshal.Cast<byte, float>(_buffers[RequireBuffer(buffers[1])]);
            var output = new byte[_buffers[RequireBuffer(buffers[0])].Length];
            var outputFloats = MemoryMarshal.Cast<byte, float>(output.AsSpan());
            for (var index = 0; index < left.Length; index++)
            {
                outputFloats[index] = left[index] + right[index];
            }

            _buffers[RequireBuffer(buffers[2])] = output;
            return Task.CompletedTask;
        }

        private static object RequireBuffer(object? buffer)
            => buffer ?? throw new ArgumentNullException(nameof(buffer));
    }
}
