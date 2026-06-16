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

### WasmScreenshotProvider

`WasmScreenshotProvider` returns framebuffer bytes for capture and diagnostic
flows.

### WasmSaveStateProvider

`WasmSaveStateProvider` snapshots and restores runtime linear memory.

### WasmTimeProvider

`WasmTimeProvider` controls deterministic runtime time through pause, resume,
scale, and tick operations.

## Browser Boundary Providers

### WasmAudioProvider

`WasmAudioProvider` stores runtime audio bytes and keeps optional
`IWebAudioJsInterop` behind `AIKernel.Wasm.Audio`. Public APIs use neutral
buffer and timing records rather than platform audio SDK types.

### WasmFramebufferProvider

`WasmFramebufferProvider` exposes the current runtime framebuffer with explicit
width, height, stride, pixel format, observed timestamp, and content hash
metadata.

### WasmFrameSourceProvider

`WasmFrameSourceProvider` implements frame capture and virtual surface listing
for WASM runtime surfaces.

### WasmInputProvider

`WasmInputProvider` accepts decomposed keyboard, pointer, drag, gamepad, and
input-state requests. It records virtual input packets and publishes runtime
events, but it does not emit Council votes or Gate decisions.

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

## Python Wrapper

The `aikernel-wasm` package is part of the 0.1.2 Python wrapper family. It
exposes wrappers and descriptors without re-implementing WASM, WebGPU,
WebAudio, or perception logic in Python. Stable wheels are created only after
the 0.1.2 publication task opens; local validation uses
`0.1.2.dev<build-number>`.
