namespace AIKernel.Wasm.Compute;

using AIKernel.Abstractions.Compute;
using AIKernel.Dtos.Gpu;
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

/// <summary>
/// [EN] Optional rev3 canonical WebGPU pass bridge for HUD, Aisthesis, and Spatial Reasoning.
/// [JA] HUD、Aisthesis、Spatial Reasoning 用の任意の rev3 canonical WebGPU pass bridge です。
/// </summary>
public interface IWebGpuRev3Backend : IWebGpuBackend
{
    /// <summary>
    /// [EN] Dispatches the canonical GPU Aisthesis pass, or returns null when this backend has no native pass.
    /// [JA] canonical GPU Aisthesis pass を dispatch します。この backend に native pass がない場合は null を返します。
    /// </summary>
    Task<GpuAisthesisOutput?> DispatchAisthesisAsync(GpuAisthesisInput input, CancellationToken cancellationToken = default);

    /// <summary>
    /// [EN] Dispatches the canonical GPU Spatial Reasoning pass, or returns null when this backend has no native pass.
    /// [JA] canonical GPU Spatial Reasoning pass を dispatch します。この backend に native pass がない場合は null を返します。
    /// </summary>
    Task<GpuSpatialReasoningOutput?> DispatchSpatialReasoningAsync(GpuSpatialReasoningInput input, CancellationToken cancellationToken = default);

    /// <summary>
    /// [EN] Dispatches the canonical GPU HUD composite pass, or returns null when this backend has no native pass.
    /// [JA] canonical GPU HUD composite pass を dispatch します。この backend に native pass がない場合は null を返します。
    /// </summary>
    Task<GpuFrameTarget?> DispatchHudCompositeAsync(GpuHudInput input, CancellationToken cancellationToken = default);
}

internal sealed class NullWebGpuBackend : IWebGpuBackend
{
    /// <summary>
    /// EN: Executes Instance.
    /// [EN] Documents this public package API member. [JA] Instance を実行します。
    /// </summary>
    public static readonly NullWebGpuBackend Instance = new();

    private NullWebGpuBackend()
    {
    }
    /// <summary>
    /// EN: Gets IsAvailable.
    /// [EN] Documents this public package API member. [JA] IsAvailable を取得します。
    /// </summary>

    public bool IsAvailable => false;
    /// <summary>
    /// EN: Executes InitializeAsync.
    /// [EN] Documents this public package API member. [JA] InitializeAsync を実行します。
    /// </summary>

    public Task InitializeAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
    /// <summary>
    /// EN: Executes CreateBufferAsync.
    /// [EN] Documents this public package API member. [JA] CreateBufferAsync を実行します。
    /// </summary>

    public Task<object?> CreateBufferAsync(int size)
        => Task.FromResult<object?>(null);
    /// <summary>
    /// EN: Executes WriteBufferAsync.
    /// [EN] Documents this public package API member. [JA] WriteBufferAsync を実行します。
    /// </summary>

    public Task WriteBufferAsync(ComputeBuffer buffer, ReadOnlyMemory<byte> data)
        => Task.CompletedTask;
    /// <summary>
    /// EN: Executes ReadBufferAsync.
    /// [EN] Documents this public package API member. [JA] ReadBufferAsync を実行します。
    /// </summary>

    public Task ReadBufferAsync(ComputeBuffer buffer, Memory<byte> destination)
        => Task.CompletedTask;
    /// <summary>
    /// EN: Executes ExecuteKernelAsync.
    /// [EN] Documents this public package API member. [JA] ExecuteKernelAsync を実行します。
    /// </summary>

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
public class WebGpuWasmBackend(IWebGpuJsInterop? jsInterop = null) : IWebGpuBackend, IWebGpuRev3Backend
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

    /// <inheritdoc />
    public async Task<GpuAisthesisOutput?> DispatchAisthesisAsync(
        GpuAisthesisInput input,
        CancellationToken cancellationToken = default)
    {
        EnsureAvailable();
        cancellationToken.ThrowIfCancellationRequested();
        if (_jsInterop is IWebGpuRev3EnvelopeJsInterop envelopeInterop)
        {
            var envelope = WebGpuRev3InteropEnvelope.ForAisthesis(input);
            EnsureValidDispatchEnvelope(envelope);
            return await envelopeInterop
                .DispatchAisthesisEnvelopeAsync(envelope, cancellationToken)
                .ConfigureAwait(false);
        }

        if (_jsInterop is not IWebGpuRev3JsInterop rev3)
        {
            return null;
        }

        return await rev3.DispatchAisthesisAsync(input, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<GpuSpatialReasoningOutput?> DispatchSpatialReasoningAsync(
        GpuSpatialReasoningInput input,
        CancellationToken cancellationToken = default)
    {
        EnsureAvailable();
        cancellationToken.ThrowIfCancellationRequested();
        if (_jsInterop is IWebGpuRev3EnvelopeJsInterop envelopeInterop)
        {
            var envelope = WebGpuRev3InteropEnvelope.ForSpatialReasoning(input);
            EnsureValidDispatchEnvelope(envelope);
            return await envelopeInterop
                .DispatchSpatialReasoningEnvelopeAsync(envelope, cancellationToken)
                .ConfigureAwait(false);
        }

        if (_jsInterop is not IWebGpuRev3JsInterop rev3)
        {
            return null;
        }

        return await rev3.DispatchSpatialReasoningAsync(input, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<GpuFrameTarget?> DispatchHudCompositeAsync(
        GpuHudInput input,
        CancellationToken cancellationToken = default)
    {
        EnsureAvailable();
        cancellationToken.ThrowIfCancellationRequested();
        if (_jsInterop is IWebGpuRev3EnvelopeJsInterop envelopeInterop)
        {
            var envelope = WebGpuRev3InteropEnvelope.ForHudComposite(input);
            EnsureValidDispatchEnvelope(envelope);
            return await envelopeInterop
                .DispatchHudCompositeEnvelopeAsync(envelope, cancellationToken)
                .ConfigureAwait(false);
        }

        if (_jsInterop is not IWebGpuRev3JsInterop rev3)
        {
            return null;
        }

        return await rev3.DispatchHudCompositeAsync(input, cancellationToken).ConfigureAwait(false);
    }

    private void EnsureAvailable()
    {
        if (!IsAvailable)
        {
            throw new NotSupportedException("WASM WebGPU backend is not available.");
        }
    }

    private static void EnsureValidDispatchEnvelope(WebGpuRev3DispatchEnvelope envelope)
    {
        var validation = WebGpuRev3InteropEnvelope.ValidateDispatchEnvelope(envelope);
        if (validation.IsValid)
        {
            return;
        }

        var summary = string.Join(
            "; ",
            validation.Errors.Select(static issue =>
                string.IsNullOrWhiteSpace(issue.Path)
                    ? $"{issue.Code}: {issue.Message}"
                    : $"{issue.Code}@{issue.Path}: {issue.Message}"));
        throw new InvalidOperationException($"Invalid WebGPU rev3 dispatch envelope. {summary}");
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
/// [EN] Browser JavaScript interop boundary for canonical rev3 GPU passes.
/// [JA] canonical rev3 GPU pass 用の browser JavaScript interop 境界です。
/// </summary>
public interface IWebGpuRev3JsInterop : IWebGpuJsInterop
{
    /// <summary>[EN] Dispatches GPU Aisthesis over a raw framebuffer target. [JA] raw framebuffer target に対して GPU Aisthesis を dispatch します。</summary>
    Task<GpuAisthesisOutput?> DispatchAisthesisAsync(GpuAisthesisInput input, CancellationToken cancellationToken = default);

    /// <summary>[EN] Dispatches GPU Spatial Reasoning over canonical matrices. [JA] canonical matrix に対して GPU Spatial Reasoning を dispatch します。</summary>
    Task<GpuSpatialReasoningOutput?> DispatchSpatialReasoningAsync(GpuSpatialReasoningInput input, CancellationToken cancellationToken = default);

    /// <summary>[EN] Dispatches GPU HUD composition to an offscreen HUD target. [JA] offscreen HUD target へ GPU HUD composition を dispatch します。</summary>
    Task<GpuFrameTarget?> DispatchHudCompositeAsync(GpuHudInput input, CancellationToken cancellationToken = default);
}

/// <summary>
/// [EN] Browser JavaScript interop boundary that accepts stable rev3 dispatch envelopes.
/// [JA] 安定した rev3 dispatch envelope を受け取る browser JavaScript interop 境界です。
/// </summary>
public interface IWebGpuRev3EnvelopeJsInterop : IWebGpuJsInterop
{
    /// <summary>[EN] Dispatches GPU Aisthesis using a JS/Dawn friendly rev3 envelope. [JA] JS/Dawn で扱いやすい rev3 envelope で GPU Aisthesis を dispatch します。</summary>
    Task<GpuAisthesisOutput?> DispatchAisthesisEnvelopeAsync(WebGpuRev3DispatchEnvelope envelope, CancellationToken cancellationToken = default);

    /// <summary>[EN] Dispatches GPU Spatial Reasoning using a JS/Dawn friendly rev3 envelope. [JA] JS/Dawn で扱いやすい rev3 envelope で GPU Spatial Reasoning を dispatch します。</summary>
    Task<GpuSpatialReasoningOutput?> DispatchSpatialReasoningEnvelopeAsync(WebGpuRev3DispatchEnvelope envelope, CancellationToken cancellationToken = default);

    /// <summary>[EN] Dispatches GPU HUD composition using a JS/Dawn friendly rev3 envelope. [JA] JS/Dawn で扱いやすい rev3 envelope で GPU HUD composition を dispatch します。</summary>
    Task<GpuFrameTarget?> DispatchHudCompositeEnvelopeAsync(WebGpuRev3DispatchEnvelope envelope, CancellationToken cancellationToken = default);
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
