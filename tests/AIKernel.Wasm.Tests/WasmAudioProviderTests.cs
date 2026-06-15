namespace AIKernel.Wasm.Tests;

using AIKernel.Wasm.Audio;
using AIKernel.Wasm.Runtime;
using WasmAudioProvider = AIKernel.Wasm.Audio.WasmAudioProvider;

/// <summary>
/// EN: Verifies WASM audio providers keep WebAudio behind the Wasm audio boundary.
/// JA: WASM audio Provider が WebAudio を Wasm audio 境界の背後に閉じることを検証します。
/// </summary>
public sealed class WasmAudioProviderTests
{
    /// <summary>
    /// EN: Plays PCM bytes into the runtime audio buffer.
    /// JA: PCM byte を runtime audio buffer へ再生します。
    /// </summary>
    [Fact]
    public async Task PlayAsync_BufferProvided_StoresRuntimeAudioBuffer()
    {
        var context = new WasmRuntimeContext();
        var provider = new WasmAudioProvider(context);

        var result = await provider.PlayAsync(
            new WebAudioPcmBuffer { Payload = [1, 2, 3] },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Equal([1, 2, 3], context.GetAudioBuffer());
    }

    /// <summary>
    /// EN: Captures runtime audio bytes when WebAudio interop is absent.
    /// JA: WebAudio interop が無い場合に runtime audio byte を capture します。
    /// </summary>
    [Fact]
    public async Task CaptureAsync_NoInterop_ReturnsRuntimeAudioFrame()
    {
        var context = new WasmRuntimeContext();
        context.SetAudioBuffer(new byte[] { 4, 5, 6 });
        var provider = new WasmAudioProvider(context);
        var frames = new List<WebAudioCaptureFrame>();

        await foreach (var frame in provider.CaptureAsync(new WebAudioCaptureRequest { MaxFrames = 1 }, CancellationToken.None))
        {
            frames.Add(frame);
        }

        Assert.Single(frames);
        Assert.Equal([4, 5, 6], frames[0].Buffer.Payload);
    }

    /// <summary>
    /// EN: Returns a structured result when GPU audio upload interop is absent.
    /// JA: GPU audio upload interop が無い場合に structured result を返します。
    /// </summary>
    [Fact]
    public async Task UploadGpuBufferAsync_NoInterop_ReturnsStructuredFailureDescriptor()
    {
        var context = new WasmRuntimeContext();
        var provider = new WasmAudioProvider(context);

        var result = await provider.UploadGpuBufferAsync(
            new WebAudioGpuUploadRequest
            {
                BufferId = "audio-debug",
                Buffer = new WebAudioPcmBuffer
                {
                    Payload = [0, 0, 0, 0, 0, 0, 128, 63],
                    Channels = 2,
                    SampleFormat = "f32"
                }
            },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(WasmAudioDiagnostics.WebAudioGpuInteropUnavailable, result.ErrorCode);
        Assert.Equal("audio-debug", result.Descriptor.BufferId);
        Assert.Equal("f32", result.Descriptor.DType);
        Assert.Equal("1,2", result.Descriptor.Shape);
        Assert.False(result.Descriptor.ZeroCopyHint);
    }
}
