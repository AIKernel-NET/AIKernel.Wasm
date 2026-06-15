namespace AIKernel.Wasm.Compute;

using AIKernel.Abstractions.Compute;
using System.Runtime.InteropServices;

/// <summary>
/// [EN] Abstraction for WebGPU adapter/device/queue bindings.
/// [JA] WebGPU adapter/device/queue binding の抽象です。
/// </summary>
public interface IWebGpuBackend
{
    /// <summary>[EN] Returns whether a native WebGPU backend is available. [JA] native WebGPU backend が利用可能かどうかを返します。</summary>
    bool IsAvailable { get; }

    /// <summary>[EN] Initializes adapter, device, and queue. [JA] adapter、device、queue を初期化します。</summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>[EN] Creates a native WebGPU buffer. [JA] native WebGPU buffer を作成します。</summary>
    Task<object?> CreateBufferAsync(int size);

    /// <summary>[EN] Writes data to a native WebGPU buffer. [JA] native WebGPU buffer へ data を書き込みます。</summary>
    Task WriteBufferAsync(ComputeBuffer buffer, ReadOnlyMemory<byte> data);

    /// <summary>[EN] Reads data from a native WebGPU buffer. [JA] native WebGPU buffer から data を読み取ります。</summary>
    Task ReadBufferAsync(ComputeBuffer buffer, Memory<byte> destination);

    /// <summary>[EN] Executes a native WebGPU compute pass. [JA] native WebGPU compute pass を実行します。</summary>
    Task ExecuteKernelAsync(ComputeKernel kernel, IReadOnlyList<ComputeBuffer> buffers);
}

internal sealed class NullWebGpuBackend : IWebGpuBackend
{
    public static readonly NullWebGpuBackend Instance = new();

    private NullWebGpuBackend()
    {
    }

    public bool IsAvailable => false;

    public Task InitializeAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<object?> CreateBufferAsync(int size)
        => Task.FromResult<object?>(null);

    public Task WriteBufferAsync(ComputeBuffer buffer, ReadOnlyMemory<byte> data)
        => Task.CompletedTask;

    public Task ReadBufferAsync(ComputeBuffer buffer, Memory<byte> destination)
        => Task.CompletedTask;

    public Task ExecuteKernelAsync(ComputeKernel kernel, IReadOnlyList<ComputeBuffer> buffers)
        => throw new NotSupportedException("A WebGPU backend binding is not available.");
}

/// <summary>
/// [EN] Native WebGPU backend adapter for host-side bindings.
/// [JA] host-side binding 向けの native WebGPU backend adapter です。
/// </summary>
public class WebGpuNativeBackend : IWebGpuBackend
{
    /// <summary>[EN] Returns whether the native backend is available. [JA] native backend が利用可能かどうかを返します。</summary>
    public bool IsAvailable { get; private set; }

    /// <summary>[EN] Initializes native WebGPU bindings. [JA] native WebGPU binding を初期化します。</summary>
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // Native host bindings are supplied by platform-specific packages outside the WASM provider.
        IsAvailable = false;
        return Task.CompletedTask;
    }

    /// <summary>[EN] Creates a native WebGPU buffer. [JA] native WebGPU buffer を作成します。</summary>
    public Task<object?> CreateBufferAsync(int size)
        => throw new NotSupportedException("Native WebGPU backend is not bound.");

    /// <summary>[EN] Writes data to a native WebGPU buffer. [JA] native WebGPU buffer へ data を書き込みます。</summary>
    public Task WriteBufferAsync(ComputeBuffer buffer, ReadOnlyMemory<byte> data)
        => throw new NotSupportedException("Native WebGPU backend is not bound.");

    /// <summary>[EN] Reads data from a native WebGPU buffer. [JA] native WebGPU buffer から data を読み取ります。</summary>
    public Task ReadBufferAsync(ComputeBuffer buffer, Memory<byte> destination)
        => throw new NotSupportedException("Native WebGPU backend is not bound.");

    /// <summary>[EN] Executes a native WebGPU compute pass. [JA] native WebGPU compute pass を実行します。</summary>
    public Task ExecuteKernelAsync(ComputeKernel kernel, IReadOnlyList<ComputeBuffer> buffers)
        => throw new NotSupportedException("Native WebGPU backend is not bound.");
}

/// <summary>
/// [EN] Browser/WASM WebGPU backend adapter for JavaScript interop.
/// [JA] JavaScript interop 向けの browser/WASM WebGPU backend adapter です。
/// </summary>
public class WebGpuWasmBackend(IWebGpuJsInterop? jsInterop = null) : IWebGpuBackend
{
    private readonly IWebGpuJsInterop? _jsInterop = jsInterop;
    private readonly Dictionary<ComputeBuffer, byte[]> _buffers = new();

    /// <summary>[EN] Returns whether the browser WebGPU backend is available. [JA] browser WebGPU backend が利用可能かどうかを返します。</summary>
    public bool IsAvailable { get; private set; }

    /// <summary>[EN] Initializes browser WebGPU through JS interop when available. [JA] JS interop が利用可能な場合に browser WebGPU を初期化します。</summary>
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IsAvailable = _jsInterop is not null && _jsInterop.IsWebGpuSupported();
        return Task.CompletedTask;
    }

    /// <summary>[EN] Creates a browser WebGPU buffer. [JA] browser WebGPU buffer を作成します。</summary>
    public Task<object?> CreateBufferAsync(int size)
    {
        if (!IsAvailable)
        {
            throw new NotSupportedException("WASM WebGPU backend is not available.");
        }

        return Task.FromResult<object?>(new WebGpuWasmBuffer(Guid.NewGuid(), size));
    }

    /// <summary>[EN] Writes data to a browser WebGPU buffer. [JA] browser WebGPU buffer へ data を書き込みます。</summary>
    public async Task WriteBufferAsync(ComputeBuffer buffer, ReadOnlyMemory<byte> data)
    {
        EnsureAvailable();
        ArgumentNullException.ThrowIfNull(buffer);
        _buffers[buffer] = data.ToArray();
        if (_jsInterop is not null)
        {
            await _jsInterop.WriteBufferAsync(buffer.NativeBuffer, data).ConfigureAwait(false);
        }
    }

    /// <summary>[EN] Reads data from a browser WebGPU buffer. [JA] browser WebGPU buffer から data を読み取ります。</summary>
    public async Task ReadBufferAsync(ComputeBuffer buffer, Memory<byte> destination)
    {
        EnsureAvailable();
        ArgumentNullException.ThrowIfNull(buffer);
        if (_jsInterop is not null)
        {
            var data = await _jsInterop.ReadBufferAsync(buffer.NativeBuffer, destination.Length).ConfigureAwait(false);
            data.CopyTo(destination);
            return;
        }

        if (_buffers.TryGetValue(buffer, out var stored))
        {
            stored.CopyTo(destination);
        }
    }

    /// <summary>[EN] Executes a browser WebGPU compute pass. [JA] browser WebGPU compute pass を実行します。</summary>
    public async Task ExecuteKernelAsync(ComputeKernel kernel, IReadOnlyList<ComputeBuffer> buffers)
    {
        EnsureAvailable();
        ArgumentNullException.ThrowIfNull(kernel);
        ArgumentNullException.ThrowIfNull(buffers);
        if (_jsInterop is not null)
        {
            await _jsInterop.ExecuteKernelAsync(kernel.Wgsl, buffers.Select(static buffer => buffer.NativeBuffer).ToArray(), kernel.DispatchX, kernel.DispatchY, kernel.DispatchZ).ConfigureAwait(false);
            return;
        }

        ExecuteInMemoryVectorAdd(kernel, buffers);
    }

    private void EnsureAvailable()
    {
        if (!IsAvailable)
        {
            throw new NotSupportedException("WASM WebGPU backend is not available.");
        }
    }

    private void ExecuteInMemoryVectorAdd(ComputeKernel kernel, IReadOnlyList<ComputeBuffer> buffers)
    {
        if (!kernel.Wgsl.Contains("out[i] = a[i] + b[i]", StringComparison.Ordinal)
            && !kernel.Wgsl.Contains("out[i]=a[i]+b[i]", StringComparison.Ordinal))
        {
            throw new NotSupportedException("In-memory WebGPU backend currently supports vector-add only. ErrorCode=WEBGPU_KERNEL_NOT_SUPPORTED");
        }

        var left = MemoryMarshal.Cast<byte, float>(_buffers[buffers[0]]);
        var right = MemoryMarshal.Cast<byte, float>(_buffers[buffers[1]]);
        var output = new byte[buffers[2].Size];
        var outputFloats = MemoryMarshal.Cast<byte, float>(output.AsSpan());
        for (var index = 0; index < left.Length; index++)
        {
            outputFloats[index] = left[index] + right[index];
        }

        _buffers[buffers[2]] = output;
        output.AsSpan().CopyTo(buffers[2].AsMemory().Span);
    }
}

/// <summary>
/// [EN] Browser JavaScript interop boundary for WebGPU calls.
/// [JA] WebGPU call 用の browser JavaScript interop 境界です。
/// </summary>
public interface IWebGpuJsInterop
{
    /// <summary>[EN] Returns whether navigator.gpu is available. [JA] navigator.gpu が利用可能かどうかを返します。</summary>
    bool IsWebGpuSupported();

    /// <summary>[EN] Writes bytes into a JS-owned WebGPU buffer. [JA] JS 所有 WebGPU buffer に byte を書き込みます。</summary>
    Task WriteBufferAsync(object? buffer, ReadOnlyMemory<byte> data);

    /// <summary>[EN] Reads bytes from a JS-owned WebGPU buffer. [JA] JS 所有 WebGPU buffer から byte を読み取ります。</summary>
    Task<byte[]> ReadBufferAsync(object? buffer, int length);

    /// <summary>[EN] Dispatches a JS-owned WebGPU compute pipeline. [JA] JS 所有 WebGPU compute pipeline を dispatch します。</summary>
    Task ExecuteKernelAsync(string wgsl, IReadOnlyList<object?> buffers, int x, int y, int z);
}

/// <summary>
/// [EN] In-memory handle representing a WASM WebGPU buffer.
/// [JA] WASM WebGPU buffer を表す in-memory handle です。
/// </summary>
public class WebGpuWasmBuffer
{
    /// <summary>
    /// [EN] Initializes a WASM WebGPU buffer handle.
    /// [JA] WASM WebGPU buffer handle を初期化します。
    /// </summary>
    /// <param name="id">[EN] Stable buffer identifier. [JA] 安定した buffer 識別子です。</param>
    /// <param name="size">[EN] Buffer size in bytes. [JA] buffer size を byte 単位で表します。</param>
    public WebGpuWasmBuffer(Guid id, int size)
    {
        Id = id;
        Size = size;
    }

    /// <summary>
    /// [EN] Gets the stable buffer identifier.
    /// [JA] 安定した buffer 識別子を取得します。
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// [EN] Gets the buffer size in bytes.
    /// [JA] buffer size を byte 単位で取得します。
    /// </summary>
    public int Size { get; }
}
