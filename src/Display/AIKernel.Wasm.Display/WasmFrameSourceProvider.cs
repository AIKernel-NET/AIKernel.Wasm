namespace AIKernel.Wasm.Display;

using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using AIKernel.Abstractions.Frame;
using AIKernel.Dtos.Frame;
using AIKernel.Dtos.Providers;
using AIKernel.Dtos.Runtime;
using AIKernel.Enums;
using AIKernel.Wasm.Runtime;

/// <summary>
/// [EN] WASM virtual frame-source provider for framebuffer snapshots and virtual surface listing.
/// [JA] framebuffer snapshot と virtual surface listing を提供する WASM virtual frame-source Provider です。
/// </summary>
public sealed class WasmFrameSourceProvider : WasmKernelProviderBase, IVirtualFrameSourceProvider, IFrameSurfaceProvider
{
    private readonly WasmRuntimeContext _context;
    private readonly WasmDisplaySurfaceDescriptor _descriptor;
    private long _frameIndex;

    /// <summary>
    /// [EN] Initializes a WASM frame-source provider.
    /// [JA] WASM frame-source Provider を初期化します。
    /// </summary>
    public WasmFrameSourceProvider(
        WasmRuntimeContext? context = null,
        WasmDisplaySurfaceDescriptor? descriptor = null)
        : base(
            "wasm.frame-source",
            "WASM Frame Source Provider",
            ["wasm.frame.capture", "wasm.surface.list", "wasm.surface.bind"],
            ["frame", "image"],
            Capabilities())
    {
        _context = context ?? new WasmRuntimeContext();
        _descriptor = descriptor ?? new WasmDisplaySurfaceDescriptor();
    }

    /// <summary>
    /// [EN] Captures deterministic framebuffer snapshots from the WASM runtime context.
    /// [JA] WASM runtime context から deterministic な framebuffer snapshot を取得します。
    /// </summary>
    public async IAsyncEnumerable<FrameSnapshot> CaptureAsync(
        FrameCaptureRequest request,
        ProviderExecutionContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        var maxFrames = Math.Max(1, request.MaxFrames ?? 1);
        for (var index = 0; index < maxFrames; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var frame = _context.GetFramebuffer();
            var frameIndex = Interlocked.Increment(ref _frameIndex);
            yield return CreateSnapshot(request.SourceId, frameIndex, frame);
            await Task.Yield();
        }
    }

    /// <summary>
    /// [EN] Lists the virtual framebuffer surfaces exposed by the WASM runtime.
    /// [JA] WASM runtime が公開する virtual framebuffer surface を列挙します。
    /// </summary>
    public ValueTask<IReadOnlyList<VirtualSurfaceDescriptor>> ListSurfacesAsync(
        SandboxInstanceHandle handle,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<IReadOnlyList<VirtualSurfaceDescriptor>>(
        [
            new()
            {
                SurfaceId = _descriptor.SurfaceId,
                Name = "WASM Framebuffer Surface",
                Buffer = _descriptor.ToFrameBufferDescriptor(),
                Metadata = _descriptor.Metadata
            }
        ]);
    }

    /// <summary>
    /// [EN] Binds a logical frame source to the WASM framebuffer surface.
    /// [JA] logical frame source を WASM framebuffer surface へ bind します。
    /// </summary>
    public ValueTask<FrameSurfaceBinding> BindAsync(
        FrameSurfaceBindingRequest request,
        ProviderExecutionContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);

        var matched = string.Equals(request.SurfaceId, _descriptor.SurfaceId, StringComparison.Ordinal);
        return ValueTask.FromResult(new FrameSurfaceBinding
        {
            Succeeded = matched,
            SurfaceId = matched ? _descriptor.SurfaceId : null,
            UsingCpuFallback = true,
            ZeroCopy = false,
            Metadata = matched
                ? _descriptor.Metadata
                : new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["errorCode"] = "WASM_SURFACE_NOT_FOUND",
                    ["requestedSurfaceId"] = request.SurfaceId
                }
        });
    }

    private FrameSnapshot CreateSnapshot(string sourceId, long frameIndex, byte[] frame)
        => new()
        {
            FrameId = $"{sourceId}:{frameIndex}",
            SourceId = sourceId,
            FrameIndex = frameIndex,
            Buffer = _descriptor.ToFrameBufferDescriptor(),
            FrameHash = Convert.ToHexString(SHA256.HashData(frame)),
            Metadata = new Dictionary<string, string>(_descriptor.Metadata, StringComparer.Ordinal)
            {
                ["surfaceId"] = _descriptor.SurfaceId,
                ["observedAt"] = _context.Clock.ToString("O", System.Globalization.CultureInfo.InvariantCulture),
                ["byteLength"] = frame.Length.ToString(System.Globalization.CultureInfo.InvariantCulture)
            }
        };

    private static IReadOnlyList<ProviderCapability> Capabilities()
        =>
        [
            new()
            {
                LogicalCapabilityId = "wasm.frame-source",
                ProviderCapabilityId = "wasm.frame.capture",
                Flags = ProviderCapabilityFlags.FrameSource | ProviderCapabilityFlags.FrameSurface,
                Kind = ProviderKind.Observer,
                InputModalities = InputModalities.None,
                OutputModalities = OutputModalities.Frame,
                RiskLevel = ProviderRiskLevel.ReadOnly,
                Availability = new CapabilityAvailability
                {
                    IsAvailable = true,
                    Reason = ProviderAvailabilityReason.Available
                }
            }
        ];
}
