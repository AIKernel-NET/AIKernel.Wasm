namespace AIKernel.Wasm.Tests;

using AIKernel.Dtos.Frame;
using AIKernel.Dtos.Providers;
using AIKernel.Dtos.Runtime;
using AIKernel.Enums;
using AIKernel.Wasm.Display;
using AIKernel.Wasm.Runtime;

/// <summary>
/// EN: Verifies WASM display providers expose framebuffer snapshots without browser objects.
/// JA: WASM display Provider が browser object を公開せず framebuffer snapshot を提供することを検証します。
/// </summary>
public sealed class WasmDisplayProviderTests
{
    /// <summary>
    /// EN: Captures framebuffer bytes as frame snapshots.
    /// JA: framebuffer byte を frame snapshot として取得します。
    /// </summary>
    [Fact]
    public async Task CaptureAsync_FramebufferAvailable_ReturnsFrameSnapshot()
    {
        var context = new WasmRuntimeContext();
        context.SetFramebuffer(new byte[] { 1, 2, 3, 4 });
        var provider = new WasmFrameSourceProvider(
            context,
            new WasmDisplaySurfaceDescriptor
            {
                SurfaceId = "surface",
                Width = 1,
                Height = 1,
                Stride = 4,
                PixelFormat = FramePixelFormat.Rgba32
            });
        var request = new FrameCaptureRequest { SourceId = "surface", MaxFrames = 1 };
        var execution = new ProviderExecutionContext { ExecutionId = "display-test" };
        var frames = new List<FrameSnapshot>();

        await foreach (var frame in provider.CaptureAsync(request, execution, CancellationToken.None))
        {
            frames.Add(frame);
        }

        Assert.Single(frames);
        Assert.Equal("surface", frames[0].SourceId);
        Assert.Equal("4", frames[0].Metadata["byteLength"]);
        Assert.False(string.IsNullOrWhiteSpace(frames[0].FrameHash));
    }

    /// <summary>
    /// EN: Lists and binds the deterministic virtual framebuffer surface.
    /// JA: deterministic virtual framebuffer surface を列挙し bind します。
    /// </summary>
    [Fact]
    public async Task ListSurfacesAndBindAsync_SurfaceMatches_ReturnsBinding()
    {
        var provider = new WasmFrameSourceProvider(
            descriptor: new WasmDisplaySurfaceDescriptor
            {
                SurfaceId = "surface",
                Width = 320,
                Height = 200,
                Stride = 1280
            });

        var surfaces = await provider.ListSurfacesAsync(
            new SandboxInstanceHandle { InstanceId = "sandbox" },
            CancellationToken.None);
        var binding = await provider.BindAsync(
            new FrameSurfaceBindingRequest { SurfaceId = "surface", SourceId = "source" },
            new ProviderExecutionContext { ExecutionId = "display-bind" },
            CancellationToken.None);

        Assert.Single(surfaces);
        Assert.True(binding.Succeeded);
        Assert.Equal("surface", binding.SurfaceId);
    }
}
