namespace AIKernel.Wasm.Input;

using AIKernel.Dtos.Input;
using AIKernel.Enums;

/// <summary>
/// [EN] Maps AIKernel virtual input packets to deterministic WASM runtime event metadata.
/// [JA] AIKernel virtual input packet を deterministic な WASM runtime event metadata へ map します。
/// </summary>
public sealed class WasmInputEventMapper
{
    /// <summary>
    /// [EN] Converts a virtual input packet into runtime event metadata.
    /// [JA] virtual input packet を runtime event metadata へ変換します。
    /// </summary>
    public IReadOnlyDictionary<string, string> ToEventMetadata(VirtualInputPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["inputId"] = packet.InputId,
            ["kind"] = packet.Kind.ToString(),
            ["isKnownKind"] = (packet.Kind != VirtualInputKind.Unknown).ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
    }
}
