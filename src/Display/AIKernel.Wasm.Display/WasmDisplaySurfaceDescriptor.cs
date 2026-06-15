namespace AIKernel.Wasm.Display;

using AIKernel.Dtos.Frame;
using AIKernel.Enums;

/// <summary>
/// [EN] Describes a WASM framebuffer surface without exposing browser canvas objects.
/// [JA] browser canvas object を公開せずに WASM framebuffer surface を記述します。
/// </summary>
public sealed record WasmDisplaySurfaceDescriptor
{
    /// <summary>[EN] Gets the surface identifier. [JA] surface identifier を取得します。</summary>
    public string SurfaceId { get; init; } = "wasm.surface";

    /// <summary>[EN] Gets the frame width. [JA] frame width を取得します。</summary>
    public int Width { get; init; }

    /// <summary>[EN] Gets the frame height. [JA] frame height を取得します。</summary>
    public int Height { get; init; }

    /// <summary>[EN] Gets the stride in bytes. [JA] byte 単位の stride を取得します。</summary>
    public int Stride { get; init; }

    /// <summary>[EN] Gets the pixel format. [JA] pixel format を取得します。</summary>
    public FramePixelFormat PixelFormat { get; init; } = FramePixelFormat.Rgba32;

    /// <summary>[EN] Gets optional surface metadata. [JA] 任意の surface metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// [EN] Converts this WASM descriptor to the AIKernel frame-buffer descriptor contract.
    /// [JA] この WASM descriptor を AIKernel frame-buffer descriptor contract へ変換します。
    /// </summary>
    public FrameBufferDescriptor ToFrameBufferDescriptor()
        => new()
        {
            Width = Width,
            Height = Height,
            Stride = Stride,
            PixelFormat = PixelFormat,
            ZeroCopyHint = false
        };
}
