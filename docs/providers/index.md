# Provider Catalog

[日本語](index-ja.md)

AIKernel.Wasm contains WASM-scoped runtime providers and the WebGPU compute
provider. These providers implement Core abstractions while keeping browser and
WASM implementation details out of Core.

## Runtime Providers

### WasmRuntime

`WasmRuntime` is the WASM process host. It implements Core `IProcessHost` and
creates `IProcess` handles from `WasmProcessOptions`.

Primary responsibilities:

- create runtime contexts
- create WASM process handles
- coordinate runtime lifecycle
- expose the lowest-level WASM runtime provider identity

### WasmProcessProvider

`WasmProcessProvider` owns WASM process lifecycle operations.

It supports:

- process creation from module bytes
- start and stop lifecycle transitions
- process listing
- EventBus publication for lifecycle events

### WasmMemoryProvider

`WasmMemoryProvider` exposes checked linear-memory operations. It validates
ranges before access and delegates to `WasmRuntimeContext`.

### WasmStdinProvider

`WasmStdinProvider` forwards deterministic stdin text into the runtime context.
When an exported stdin function is available, the runtime can call that export.

### WasmFileSystemProvider

`WasmFileSystemProvider` exposes the runtime's deterministic in-memory
WASI-style file surface. It does not perform host file-system writes directly.

### WasmEventProvider

`WasmEventProvider` bridges WASM-originated events into AIKernel `IEventBus`.

### WasmAudioProvider

`WasmAudioProvider` stores audio buffer bytes for a browser/WebAudio bridge.

### WasmScreenshotProvider

`WasmScreenshotProvider` returns framebuffer bytes for capture and diagnostic
flows.

### WasmSaveStateProvider

`WasmSaveStateProvider` snapshots and restores runtime linear memory.

### WasmTimeProvider

`WasmTimeProvider` controls deterministic runtime time through pause, resume,
scale, and tick operations.

## WebGPU Provider

### WebGpuComputeProvider

`WebGpuComputeProvider` implements Core `IComputeProvider`.

It owns:

- WebGPU backend routing
- CPU fallback routing through `CpuComputeProvider`
- compute buffer creation, write, read, and kernel execution
- WebGPU capability descriptor mapping
- `GpuKernelExecuted` EventBus publication

The provider exposes `webgpu.compute` with `compute.dispatch` and
`compute.vector_add`.

## Python Coverage

The `aikernel-wasm` Python package exposes wrappers and descriptors for every
provider in this catalog. It does not re-implement provider logic.
