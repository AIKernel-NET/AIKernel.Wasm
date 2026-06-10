# AIKernel.Wasm Architecture

[日本語](index-ja.md)

AIKernel.Wasm is the third layer in the AIKernel OS architecture:

- Core defines OS contracts such as process, compute, VFS, EventBus, and
  provider abstractions.
- Providers.Standard implements host-side OS drivers such as CPU compute,
  logging, file system, network, scheduler, profiler, and process supervision.
- AIKernel.Wasm implements browser/WebAssembly runtime services and the WASM
  WebGPU compute backend.

## Responsibility Boundary

AIKernel.Wasm owns implementation details that are specific to browser or WASM
execution:

- WASM module loading and runtime context state
- deterministic linear memory and save-state handling
- stdin, file, event, audio, screenshot, and time services for WASM processes
- browser/WebGPU backend integration
- process lifecycle mapping to Core `IProcess` / `IProcessHost`

AIKernel.Wasm does not own:

- Core abstractions
- host-side file system or logging drivers
- provider manifest loading for external non-WASM providers
- instrumentation and replay tools

## Dependency Direction

The intended dependency direction is:

```text
AIKernel.NET contracts
        ↓
AIKernel.Core
        ↓
AIKernel.Providers.Standard
        ↓
AIKernel.Wasm
```

AIKernel.Wasm can use Providers.Standard CPU fallback for compute, but Core
does not depend on AIKernel.Wasm.

## Event Flow

Runtime activity is reported through Core `IEventBus` when an event bus is
provided. Stable event names include:

- `ProcessStarted`
- `ProcessStopped`
- `ProcessCrashed`
- `MemoryAccessed`
- `StdinSent`
- `FileAccessed`
- `GpuKernelExecuted`

These events let Control/Bonsai and CLI diagnostics observe WASM behavior
without depending on browser-specific objects.

## Package Layout

```text
AIKernel.Wasm/
  src/
    Runtime/
      AIKernel.Wasm.Runtime/
    Compute/
      WebGpuComputeProvider/
  python/
    src/aikernel_wasm/
  tests/
    AIKernel.Wasm.Tests/
    WebGpuComputeProvider.Tests/
```
