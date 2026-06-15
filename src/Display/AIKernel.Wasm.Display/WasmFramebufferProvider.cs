namespace AIKernel.Wasm.Display;

using AIKernel.Dtos.Frame;
using AIKernel.Wasm.Runtime;

/// <summary>
/// [EN] Technical WASM provider for writing and reading framebuffer bytes.
/// [JA] framebuffer byte の書き込み / 読み取りを行う技術的な WASM Provider です。
/// </summary>
public sealed class WasmFramebufferProvider
{
    private readonly WasmRuntimeContext _context;

    /// <summary>
    /// [EN] Initializes a framebuffer provider backed by a WASM runtime context.
    /// [JA] WASM runtime context に支えられた framebuffer Provider を初期化します。
    /// </summary>
    public WasmFramebufferProvider(
        WasmRuntimeContext? context = null,
        WasmDisplaySurfaceDescriptor? descriptor = null)
    {
        _context = context ?? new WasmRuntimeContext();
        Descriptor = descriptor ?? new WasmDisplaySurfaceDescriptor();
    }

    /// <summary>[EN] Gets the display surface descriptor. [JA] display surface descriptor を取得します。</summary>
    public WasmDisplaySurfaceDescriptor Descriptor { get; }

    /// <summary>[EN] Writes framebuffer bytes into the runtime context. [JA] runtime context に framebuffer byte を書き込みます。</summary>
    public void SetFramebuffer(ReadOnlyMemory<byte> framebuffer)
        => _context.SetFramebuffer(framebuffer);

    /// <summary>[EN] Reads a defensive copy of framebuffer bytes. [JA] framebuffer byte の defensive copy を読み取ります。</summary>
    public byte[] GetFramebuffer()
        => _context.GetFramebuffer();

    /// <summary>[EN] Creates an AIKernel frame snapshot from the current framebuffer. [JA] 現在の framebuffer から AIKernel frame snapshot を作成します。</summary>
    public FrameSnapshot CreateSnapshot(long frameIndex, string? frameHash = null)
        => new()
        {
            FrameId = $"{Descriptor.SurfaceId}:{frameIndex}",
            SourceId = Descriptor.SurfaceId,
            FrameIndex = frameIndex,
            Buffer = Descriptor.ToFrameBufferDescriptor(),
            FrameHash = frameHash,
            Metadata = Descriptor.Metadata
        };
}
