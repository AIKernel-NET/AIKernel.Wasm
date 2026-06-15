namespace AIKernel.Wasm.Audio;

/// <summary>
/// [EN] Normalizes browser WebAudio timing into deterministic carrier values.
/// [JA] browser WebAudio timing を deterministic な carrier value へ正規化します。
/// </summary>
public sealed class WebAudioTimingNormalizer
{
    /// <summary>
    /// [EN] Creates normalized timing information from provider-neutral counters.
    /// [JA] provider-neutral counter から正規化済み timing information を作成します。
    /// </summary>
    public AudioTimingInfo Normalize(long frameIndex, long sampleOffset, DateTimeOffset timestamp)
        => new()
        {
            FrameIndex = frameIndex,
            SampleOffset = sampleOffset,
            Timestamp = timestamp
        };
}
