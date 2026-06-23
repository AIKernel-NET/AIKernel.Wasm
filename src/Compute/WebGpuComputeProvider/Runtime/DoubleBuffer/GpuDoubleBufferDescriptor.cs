using AIKernel.Dtos.Gpu;

namespace AIKernel.Wasm.Compute;

/// <summary>
/// [EN] Describes the canonical raw/HUD double-buffer pair used by GPU HUD and GPU Aisthesis.
/// [JA] GPU HUD と GPU Aisthesis が使用する canonical raw/HUD double-buffer pair を表します。
/// </summary>
public sealed record GpuDoubleBufferDescriptor
{
    /// <summary>[EN] Raw game framebuffer target. [JA] raw game framebuffer target です。</summary>
    public required GpuFrameTarget RawFramebuffer { get; init; }

    /// <summary>[EN] HUD-composited offscreen target. [JA] HUD 合成済み offscreen target です。</summary>
    public required GpuFrameTarget HudCompositeOffscreen { get; init; }

    /// <summary>[EN] Maximum HUD composite refresh rate. [JA] HUD composite の最大 refresh rate です。</summary>
    public int HudCompositeFpsCap { get; init; } = 30;

    /// <summary>[EN] True when raw analysis must bypass display/HUD targets. [JA] raw analysis が display/HUD target を bypass する必要がある場合 true です。</summary>
    public bool RequireRawAnalysisPath { get; init; } = true;
}
