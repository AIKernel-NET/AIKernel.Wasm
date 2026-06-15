namespace AIKernel.Wasm.Input;

using AIKernel.Abstractions.Input;
using AIKernel.Dtos.Input;
using AIKernel.Dtos.Providers;
using AIKernel.Enums;
using AIKernel.Wasm.Runtime;

/// <summary>
/// [EN] Technical WASM virtual input provider for keyboard, pointer, gamepad, and decomposed input packets.
/// [JA] keyboard、pointer、gamepad、decomposed input packet を扱う技術的な WASM virtual input Provider です。
/// </summary>
public class WasmInputProvider : WasmKernelProviderBase, IDecomposedInputProvider
{
    private readonly List<WasmVirtualInputPacket> _packets = [];
    private readonly WasmRuntimeContext _context;

    /// <summary>
    /// [EN] Initializes a WASM virtual input provider.
    /// [JA] WASM virtual input Provider を初期化します。
    /// </summary>
    public WasmInputProvider(WasmRuntimeContext? context = null)
        : base(
            "wasm.input",
            "WASM Virtual Input Provider",
            ["wasm.input.keyboard", "wasm.input.pointer", "wasm.input.gamepad", "wasm.input.state"],
            ["input", "keyboard", "pointer", "gamepad"],
            Capabilities())
    {
        _context = context ?? new WasmRuntimeContext();
    }

    /// <summary>[EN] Gets the accepted input packets. [JA] 受理済み input packet を取得します。</summary>
    public IReadOnlyList<WasmVirtualInputPacket> Packets => _packets.ToArray();

    /// <summary>[EN] Sends virtual input. [JA] virtual input を送信します。</summary>
    public ValueTask<VirtualInputResult> SendAsync(
        VirtualInputPacket packet,
        ProviderExecutionContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(packet);
        ArgumentNullException.ThrowIfNull(context);
        return ValueTask.FromResult(Accept(packet));
    }

    /// <summary>[EN] Sends keyboard input. [JA] keyboard input を送信します。</summary>
    public ValueTask<VirtualInputResult> SendKeyboardAsync(
        KeyboardInputPacket packet,
        ProviderExecutionContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(packet);
        return SendAsync(new VirtualInputPacket
        {
            InputId = CreateInputId("keyboard"),
            Kind = VirtualInputKind.Keyboard,
            Keyboard = packet
        }, context, cancellationToken);
    }

    /// <summary>[EN] Sends pointer input. [JA] pointer input を送信します。</summary>
    public ValueTask<VirtualInputResult> SendPointerAsync(
        PointerInputPacket packet,
        ProviderExecutionContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(packet);
        return SendAsync(new VirtualInputPacket
        {
            InputId = CreateInputId("pointer"),
            Kind = VirtualInputKind.Pointer,
            Pointer = packet
        }, context, cancellationToken);
    }

    /// <summary>[EN] Sends gamepad input. [JA] gamepad input を送信します。</summary>
    public ValueTask<VirtualInputResult> SendGamepadAsync(
        GamepadInputPacket packet,
        ProviderExecutionContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(packet);
        return SendAsync(new VirtualInputPacket
        {
            InputId = CreateInputId("gamepad"),
            Kind = VirtualInputKind.Gamepad,
            Gamepad = packet
        }, context, cancellationToken);
    }

    /// <summary>[EN] Sends key presses. [JA] key press を送信します。</summary>
    public ValueTask<VirtualInputResult> SendKeysAsync(
        SendKeysRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendKeyboardAsync(
            new KeyboardInputPacket
            {
                PressedKeys = request.Keys,
                Metadata = request.Metadata
            },
            DefaultContext(),
            cancellationToken);
    }

    /// <summary>[EN] Types text through virtual keyboard metadata. [JA] virtual keyboard metadata 経由で text を入力します。</summary>
    public ValueTask<VirtualInputResult> TypeTextAsync(
        TypeTextRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendKeyboardAsync(
            new KeyboardInputPacket
            {
                Metadata = new Dictionary<string, string>(request.Metadata, StringComparer.Ordinal)
                {
                    ["text"] = request.Text
                }
            },
            DefaultContext(),
            cancellationToken);
    }

    /// <summary>[EN] Moves a pointer. [JA] pointer を移動します。</summary>
    public ValueTask<VirtualInputResult> MoveAsync(
        PointerMoveRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendPointerAsync(
            new PointerInputPacket
            {
                X = request.X,
                Y = request.Y,
                Metadata = request.Metadata
            },
            DefaultContext(),
            cancellationToken);
    }

    /// <summary>[EN] Clicks a pointer button. [JA] pointer button を click します。</summary>
    public ValueTask<VirtualInputResult> ClickAsync(
        PointerClickRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendPointerAsync(
            new PointerInputPacket
            {
                X = request.X,
                Y = request.Y,
                Buttons = string.IsNullOrWhiteSpace(request.Button) ? [] : [request.Button],
                Metadata = request.Metadata
            },
            DefaultContext(),
            cancellationToken);
    }

    /// <summary>[EN] Drags a pointer from start to end coordinates. [JA] pointer を開始座標から終了座標へ drag します。</summary>
    public ValueTask<VirtualInputResult> DragAsync(
        PointerDragRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendPointerAsync(
            new PointerInputPacket
            {
                X = request.EndX,
                Y = request.EndY,
                Buttons = ["drag"],
                Metadata = new Dictionary<string, string>(request.Metadata, StringComparer.Ordinal)
                {
                    ["startX"] = request.StartX.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["startY"] = request.StartY.ToString(System.Globalization.CultureInfo.InvariantCulture)
                }
            },
            DefaultContext(),
            cancellationToken);
    }

    /// <summary>[EN] Sends an existing virtual input state packet. [JA] 既存 virtual input state packet を送信します。</summary>
    public ValueTask<VirtualInputResult> SendStateAsync(
        InputStateRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return SendAsync(request.Packet, DefaultContext(), cancellationToken);
    }

    private VirtualInputResult Accept(VirtualInputPacket packet)
    {
        if (packet.Kind == VirtualInputKind.Unknown)
        {
            return Failure("WASM_INPUT_KIND_UNKNOWN", "Virtual input kind must be specified.");
        }

        _packets.Add(new WasmVirtualInputPacket
        {
            Packet = packet,
            AppliedAt = _context.Clock,
            Metadata = packet.Metadata
        });

        _context.PublishEvent("VirtualInputSent", new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["inputId"] = packet.InputId,
            ["kind"] = packet.Kind.ToString()
        });

        return new VirtualInputResult
        {
            Succeeded = true,
            Metadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["inputId"] = packet.InputId,
                ["kind"] = packet.Kind.ToString()
            }
        };
    }

    private static VirtualInputResult Failure(string code, string message)
        => new()
        {
            Succeeded = false,
            FailureCode = code,
            FailureMessage = message
        };

    private static ProviderExecutionContext DefaultContext()
        => new()
        {
            ExecutionId = "wasm-input"
        };

    private static string CreateInputId(string kind)
        => $"wasm.{kind}.{Guid.NewGuid():N}";

    private static IReadOnlyList<ProviderCapability> Capabilities()
        =>
        [
            new()
            {
                LogicalCapabilityId = "wasm.virtual-input",
                ProviderCapabilityId = "wasm.input.state",
                Flags = ProviderCapabilityFlags.VirtualInput,
                Kind = ProviderKind.ActionProvider,
                InputModalities = InputModalities.VirtualInput | InputModalities.Control,
                OutputModalities = OutputModalities.VirtualInput,
                RiskLevel = ProviderRiskLevel.High,
                PrivilegedAction = true,
                Availability = new CapabilityAvailability
                {
                    IsAvailable = true,
                    Reason = ProviderAvailabilityReason.Available
                }
            }
        ];
}

/// <summary>[EN] Technical keyboard-only WASM input provider. [JA] keyboard 専用の技術的な WASM input Provider です。</summary>
public sealed class WasmKeyboardInputProvider(WasmRuntimeContext? context = null) : WasmInputProvider(context);

/// <summary>[EN] Technical pointer-only WASM input provider. [JA] pointer 専用の技術的な WASM input Provider です。</summary>
public sealed class WasmPointerInputProvider(WasmRuntimeContext? context = null) : WasmInputProvider(context);

/// <summary>[EN] Technical gamepad-only WASM input provider. [JA] gamepad 専用の技術的な WASM input Provider です。</summary>
public sealed class WasmGamepadInputProvider(WasmRuntimeContext? context = null) : WasmInputProvider(context);
