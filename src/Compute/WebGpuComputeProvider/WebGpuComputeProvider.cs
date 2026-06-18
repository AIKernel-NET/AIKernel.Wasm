using AIKernel.Abstractions.Compute;
using AIKernel.Abstractions.Events;
using AIKernel.Abstractions.Models;
using AIKernel.Abstractions.Providers;
using AIKernel.Common.Results;
using AIKernel.Dtos.Capabilities;
using AIKernel.Dtos.Core;
using AIKernel.Dtos.Routing;
using AIKernel.Providers.Standard.Compute;

namespace AIKernel.Wasm.Compute;

/// <summary>
/// [EN] Official AIKernel compute provider for WebGPU with deterministic CPU fallback.
/// [JA] deterministic CPU fallback を備えた WebGPU 向け AIKernel 公式 compute Provider です。
/// </summary>
public class WebGpuComputeProvider(
    WebGpuComputeSettings settings,
    IWebGpuBackend? backend = null,
    IComputeProvider? cpuFallback = null,
    IEventBus? eventBus = null) : IProvider, IComputeProvider
{
    private static readonly WebGpuComputeProviderCapabilities Capabilities = new();
    private readonly IWebGpuBackend _backend = backend ?? NullWebGpuBackend.Instance;
    private readonly IComputeProvider _cpuFallback = cpuFallback ?? new CpuComputeProvider();
    private readonly IEventBus? _eventBus = eventBus;
    private readonly SemaphoreSlim _initializeLock = new(1, 1);
    private readonly WebGpuComputeSettings _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    private volatile bool _initialized;
    private volatile bool _usingCpuFallback = true;

    /// <summary>
    /// [EN] Initializes the provider with default dynamic-loading settings.
    /// [JA] dynamic loading 用の default settings で Provider を初期化します。
    /// </summary>
    public WebGpuComputeProvider()
        : this(new WebGpuComputeSettings())
    {
    }

    /// <summary>[EN] Provider id. [JA] Provider id です。</summary>
    public string ProviderId => _settings.ProviderId;

    /// <summary>[EN] Human-readable provider name. [JA] 人間可読な Provider 名です。</summary>
    public string Name => _settings.Name;

    /// <summary>[EN] Provider version. [JA] Provider version です。</summary>
    public string Version => _settings.Version;

    /// <summary>[EN] Returns true when CPU fallback is currently active. [JA] CPU fallback が現在有効な場合 true を返します。</summary>
    public bool UsingCpuFallback => _usingCpuFallback;

    /// <summary>
    /// [EN] Returns provider capabilities for AIKernel provider registration.
    /// [JA] AIKernel Provider 登録用の provider capabilities を返します。
    /// </summary>
    public IProviderCapabilities GetCapabilities() => Capabilities;

    /// <summary>
    /// [EN] Returns provider availability. CPU fallback makes this provider available in unsupported browsers.
    /// [JA] Provider availability を返します。CPU fallback により未対応 browser でも利用可能です。
    /// </summary>
    public Task<bool> IsAvailableAsync() => Task.FromResult(IsAvailable());

    /// <summary>
    /// [EN] Returns compute availability for the public compute contract.
    /// [JA] public compute contract 用の compute availability を返します。
    /// </summary>
    public bool IsAvailable() => true;

    /// <summary>
    /// [EN] Lazily initializes WebGPU adapter, device, and queue when a backend is available.
    /// [JA] backend が利用可能な場合に WebGPU adapter、device、queue を lazy に初期化します。
    /// </summary>
    public async Task InitializeAsync()
    {
        await EnsureInitializedAsync(CancellationToken.None).ConfigureAwait(false);
    }

    /// <summary>
    /// [EN] Shuts down the provider and releases backend resources.
    /// [JA] Provider を停止し backend resource を解放します。
    /// </summary>
    public async Task ShutdownAsync()
    {
        if (_backend is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else if (_backend is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _initialized = false;
        _usingCpuFallback = true;
    }

    /// <summary>
    /// [EN] Returns safe health metadata without exposing raw backend objects.
    /// [JA] raw backend object を公開せず safe health metadata を返します。
    /// </summary>
    public Task<ProviderHealthStatus> GetHealthAsync()
        => Task.FromResult(new ProviderHealthStatus(
            true,
            MonadicDecision.SelectText(
                !_usingCpuFallback,
                "Provider available with CPU fallback.",
                "Provider available with WebGPU backend."),
            DateTime.UtcNow,
            0));

    /// <summary>
    /// [EN] Creates a compute buffer using WebGPU storage/copy usage or CPU fallback storage.
    /// [JA] WebGPU storage/copy usage または CPU fallback storage を使用して compute buffer を作成します。
    /// </summary>
    public async Task<ComputeBuffer> CreateBufferAsync(int size)
    {
        await EnsureInitializedAsync(CancellationToken.None).ConfigureAwait(false);
        var native = await BackendSelection(_usingCpuFallback)
            .Match(
                _ => Task.FromResult<object?>(null),
                async backend => await backend.CreateBufferAsync(size).ConfigureAwait(false))
            .ConfigureAwait(false);
        return new ComputeBuffer(size, native);
    }

    /// <summary>
    /// [EN] Writes bytes into a compute buffer and validates the buffer size.
    /// [JA] compute buffer へ byte を書き込み、buffer size を検証します。
    /// </summary>
    public async Task WriteBufferAsync(ComputeBuffer buffer, ReadOnlyMemory<byte> data)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (data.Length > buffer.Size)
        {
            throw new ArgumentException("Input data is larger than the compute buffer. ErrorCode=WEBGPU_BUFFER_WRITE_SIZE_MISMATCH", nameof(data));
        }

        await EnsureInitializedAsync(CancellationToken.None).ConfigureAwait(false);
        buffer.Write(data);
        await BackendSelection(_usingCpuFallback)
            .Match(
                _ => Task.CompletedTask,
                backend => backend.WriteBufferAsync(buffer, data))
            .ConfigureAwait(false);
    }

    /// <summary>
    /// [EN] Reads bytes from a compute buffer and validates the destination size.
    /// [JA] compute buffer から byte を読み取り、destination size を検証します。
    /// </summary>
    public async Task ReadBufferAsync(ComputeBuffer buffer, Memory<byte> destination)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (destination.Length < buffer.Size)
        {
            throw new ArgumentException("Destination is smaller than the compute buffer. ErrorCode=WEBGPU_BUFFER_READ_SIZE_MISMATCH", nameof(destination));
        }

        await EnsureInitializedAsync(CancellationToken.None).ConfigureAwait(false);
        await BackendSelection(_usingCpuFallback)
            .Match(
                _ =>
                {
                    buffer.Read(destination);
                    return Task.CompletedTask;
                },
                backend => backend.ReadBufferAsync(buffer, destination))
            .ConfigureAwait(false);
    }

    /// <summary>
    /// [EN] Executes a WGSL compute kernel through WebGPU or deterministic CPU fallback.
    /// [JA] WebGPU または deterministic CPU fallback で WGSL compute kernel を実行します。
    /// </summary>
    public async Task ExecuteKernelAsync(ComputeKernel kernel, params ComputeBuffer[] buffers)
    {
        ArgumentNullException.ThrowIfNull(kernel);
        ArgumentNullException.ThrowIfNull(buffers);
        await EnsureInitializedAsync(CancellationToken.None).ConfigureAwait(false);

        await BackendSelection(_usingCpuFallback)
            .Match(
                async _ =>
                {
                    await _cpuFallback.ExecuteKernelAsync(kernel, buffers).ConfigureAwait(false);
                    await PublishKernelExecutedAsync(kernel, "cpu-fallback").ConfigureAwait(false);
                },
                async backend =>
                {
                    await backend.ExecuteKernelAsync(kernel, buffers).ConfigureAwait(false);
                    await PublishKernelExecutedAsync(kernel, "webgpu").ConfigureAwait(false);
                })
            .ConfigureAwait(false);
    }

    /// <summary>
    /// [EN] Creates a provider capability descriptor from current settings.
    /// [JA] 現在の設定から provider capability descriptor を作成します。
    /// </summary>
    public CapabilityModuleDescriptor ToCapabilityDescriptor()
        => WebGpuComputeCapabilityContracts.ToContract(
            new WebGpuComputeCapabilityDescriptor(
                ProviderId,
                _settings.AdapterProfile,
                _settings.ToMetadata()));

    private Either<IComputeProvider, IWebGpuBackend> BackendSelection(bool usingCpuFallback)
        => usingCpuFallback
            ? Either<IComputeProvider, IWebGpuBackend>.FromLeft(_cpuFallback)
            : Either<IComputeProvider, IWebGpuBackend>.FromRight(_backend);

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_initialized)
        {
            return;
        }

        await _initializeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_initialized)
            {
                return;
            }

            if (!_settings.ForceCpuFallback)
            {
                _usingCpuFallback = (await Try.RunAsync(async () =>
                {
                    await _backend.InitializeAsync(cancellationToken).ConfigureAwait(false);
                    return !_backend.IsAvailable;
                }).ConfigureAwait(false))
                .Match(_ => true, useFallback => useFallback);
            }

            _initialized = true;
        }
        finally
        {
            _initializeLock.Release();
        }
    }

    private Task PublishKernelExecutedAsync(ComputeKernel kernel, string backend)
        => _eventBus is null
            ? Task.CompletedTask
            : _eventBus.PublishAsync(
                "GpuKernelExecuted",
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["providerId"] = ProviderId,
                    ["backend"] = backend,
                    ["dispatch"] = $"{kernel.DispatchX},{kernel.DispatchY},{kernel.DispatchZ}",
                    ["workgroupSize"] = $"{kernel.WorkgroupSizeX},{kernel.WorkgroupSizeY},{kernel.WorkgroupSizeZ}"
                });
}

internal sealed class WebGpuComputeProviderCapabilities : IProviderCapabilities
{
    private static readonly string[] Operations =
    [
        "compute.dispatch",
        "compute.vector_add"
    ];

    private static readonly string[] DataTypes =
    [
        "buffer",
        "float32",
        "uint8"
    ];

    /// <summary>[EN] Supported provider operations. [JA] 対応する Provider operation です。</summary>
    public IReadOnlyList<string> SupportedOperations => Operations;

    /// <summary>[EN] Supported data types. [JA] 対応する data type です。</summary>
    public IReadOnlyList<string> SupportedDataTypes => DataTypes;

    /// <summary>[EN] Maximum concurrent connections. [JA] 最大同時 connection 数です。</summary>
    public int MaxConcurrentConnections => 1;

    /// <summary>[EN] Optional rate-limit information. [JA] 任意の rate-limit 情報です。</summary>
    public RateLimitInfo? RateLimit => null;

    /// <summary>[EN] Static capacity vector. [JA] static capacity vector です。</summary>
    public ModelCapacityVector Vector => new();

    /// <summary>[EN] Returns dynamic capacities. [JA] dynamic capacity を返します。</summary>
    public IDictionary<string, float>? GetDynamicCapacities(IExecutionConstraints constraints) => null;

    /// <summary>[EN] Returns a capability profile. [JA] capability profile を返します。</summary>
    public ICapabilityProfile? GetCapabilityProfile() => null;

    /// <summary>[EN] Returns whether an operation is supported. [JA] operation が対応しているかどうかを返します。</summary>
    public bool SupportsOperation(string operation) => Operations.Contains(operation, StringComparer.OrdinalIgnoreCase);

    /// <summary>[EN] Returns whether a data type is supported. [JA] data type が対応しているかどうかを返します。</summary>
    public bool SupportsDataType(string dataType) => DataTypes.Contains(dataType, StringComparer.OrdinalIgnoreCase);

    /// <summary>[EN] Returns whether quantization is supported. [JA] quantization が対応しているかどうかを返します。</summary>
    public bool SupportsQuantization(string quantizationLevel) => false;

    /// <summary>[EN] Query augmentation support flag. [JA] query augmentation 対応 flag です。</summary>
    public bool SupportsQueryAugmentation => false;

    /// <summary>[EN] Query decomposition support flag. [JA] query decomposition 対応 flag です。</summary>
    public bool SupportsQueryDecomposition => false;

    /// <summary>[EN] Query routing support flag. [JA] query routing 対応 flag です。</summary>
    public bool SupportsQueryRouting => false;

    /// <summary>[EN] Maximum query parts. [JA] 最大 query part 数です。</summary>
    public int MaxQueryParts => 0;

    /// <summary>[EN] Supported query-processing operations. [JA] 対応 query-processing operation です。</summary>
    public IReadOnlyList<string> SupportedQueryProcessingOperations => [];

    /// <summary>[EN] Returns whether a query-processing operation is supported. [JA] query-processing operation が対応しているかどうかを返します。</summary>
    public bool SupportsQueryProcessingOperation(string operation) => false;

    /// <summary>[EN] Embedding support flag. [JA] embedding 対応 flag です。</summary>
    public bool SupportsEmbedding => false;

    /// <summary>[EN] Embedding dimensions. [JA] embedding dimension です。</summary>
    public int? EmbeddingDimensions => null;

    /// <summary>[EN] Supported embedding models. [JA] 対応 embedding model です。</summary>
    public IReadOnlyList<string> SupportedEmbeddingModels => [];
}
