namespace AIKernel.Wasm.Tests;

using AIKernel.Dtos.Input;
using AIKernel.Dtos.Providers;
using AIKernel.Enums;
using AIKernel.Wasm.Input;

/// <summary>
/// EN: Verifies WASM input providers accept governed virtual input packets.
/// JA: WASM input Provider が governance 済み virtual input packet を受理することを検証します。
/// </summary>
public sealed class WasmInputProviderTests
{
    /// <summary>
    /// EN: Sends decomposed key input into the WASM input boundary.
    /// JA: decomposed key input を WASM input 境界へ送信します。
    /// </summary>
    [Fact]
    public async Task SendKeysAsync_KeysProvided_RecordsKeyboardPacket()
    {
        var provider = new WasmInputProvider();

        var result = await provider.SendKeysAsync(
            new SendKeysRequest { Keys = ["A", "Enter"] },
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.Single(provider.Packets);
        Assert.Equal(VirtualInputKind.Keyboard, provider.Packets[0].Packet.Kind);
    }

    /// <summary>
    /// EN: Rejects unknown input kind without throwing.
    /// JA: unknown input kind を例外ではなく failure result として拒否します。
    /// </summary>
    [Fact]
    public async Task SendAsync_UnknownKind_ReturnsFailure()
    {
        var provider = new WasmInputProvider();

        var result = await provider.SendAsync(
            new VirtualInputPacket { InputId = "input", Kind = VirtualInputKind.Unknown },
            new ProviderExecutionContext { ExecutionId = "input-test" },
            CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("WASM_INPUT_KIND_UNKNOWN", result.FailureCode);
    }

    /// <summary>
    /// EN: Maps virtual input packets to deterministic event metadata.
    /// JA: virtual input packet を deterministic event metadata へ map します。
    /// </summary>
    [Fact]
    public void ToEventMetadata_ValidPacket_ReturnsStableFields()
    {
        var mapper = new WasmInputEventMapper();

        var metadata = mapper.ToEventMetadata(new VirtualInputPacket
        {
            InputId = "input",
            Kind = VirtualInputKind.Pointer
        });

        Assert.Equal("input", metadata["inputId"]);
        Assert.Equal("Pointer", metadata["kind"]);
    }
}
