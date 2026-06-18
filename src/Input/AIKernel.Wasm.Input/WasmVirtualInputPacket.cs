namespace AIKernel.Wasm.Input;

using AIKernel.Dtos.Input;

/// <summary>
/// [EN] Captures a virtual input packet accepted by the WASM runtime boundary.
/// [JA] WASM runtime 境界が受理した virtual input packet を記録します。
/// </summary>
public sealed record WasmVirtualInputPacket
{
    /// <summary>[EN] Gets the AIKernel virtual input packet. [JA] AIKernel virtual input packet を取得します。</summary>
    public required VirtualInputPacket Packet { get; init; }

    /// <summary>[EN] Gets the deterministic applied timestamp. [JA] deterministic applied timestamp を取得します。</summary>
    public DateTimeOffset AppliedAt { get; init; }

    /// <summary>[EN] Gets optional packet metadata. [JA] 任意の packet metadata を取得します。</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}
