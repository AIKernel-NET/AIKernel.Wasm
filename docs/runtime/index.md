# Runtime Providers

[日本語](index-ja.md)

`AIKernel.Wasm.Runtime` provides the WASM process and runtime service surface.
The current implementation is deterministic and testable on Windows and Linux,
while browser-specific bindings can be supplied at the boundary.

## Runtime Context

`WasmRuntimeContext` is the shared state object. It tracks:

- loaded module bytes
- linear memory
- import and export functions
- stdin lines
- deterministic in-memory file surface
- framebuffer bytes
- audio buffer bytes
- save-state snapshots
- controllable runtime clock

Public operations are available in both direct and fail-closed forms where the
runtime uses `Result<T>` internally.

Runtime state transitions use monadic boundaries:

- process creation and lifecycle operations expose `Try*` methods returning
  `Result<T>`
- optional runtime services, event buses, exports, and process handles are
  represented with `Option<T>`
- WebGPU backend versus CPU fallback selection is a pure `Either<L,R>` decision
- host/browser interop calls are isolated behind `Try.RunAsync`

## Process Model

`WasmRuntime` implements Core `IProcessHost`. `WasmProcessProvider` creates and
starts WASM process handles using `WasmProcessOptions`.

Process states follow the Core model:

- `Starting`
- `Running`
- `Stopped`
- `Error`

Lifecycle transitions publish process events through `IEventBus` when an event
bus is available.

## Provider Surface

The runtime package exposes:

- `WasmRuntime`
- `WasmRuntimeContext`
- `WasmProcessProvider`
- `WasmMemoryProvider`
- `WasmStdinProvider`
- `WasmFileSystemProvider`
- `WasmEventProvider`
- `WasmAudioProvider`
- `WasmScreenshotProvider`
- `WasmSaveStateProvider`
- `WasmTimeProvider`

## File and Memory Semantics

The runtime file surface is deterministic and in-memory. It is intended to
bridge WASI-style reads and writes into Core abstractions without performing
host file I/O directly.

Memory operations perform range checks before reading or writing. Out-of-range
access fails closed and can publish `MemoryAccessed` metadata when instrumentation
is attached.

## Save State and Time

Save state stores deterministic snapshots of linear memory. Time control
supports pause, resume, scale, and deterministic tick advancement, making tests
repeatable without wall-clock dependence.
