# AIKernel.Wasm

[日本語 README](README-ja.md)

AIKernel.Wasm isolates browser/WebAssembly runtime services and WebGPU
boundaries for AIKernel.

AIKernel.Providers owns host-side OS standard drivers. AIKernel.Wasm owns
WASM-specific runtime, process, memory, stdin, file system, event, audio,
screenshot, save-state, time, and WebGPU compute boundaries.

Core abstractions such as `IProcess`, `IProcessHost`, `IComputeProvider`, and
`IProvider` are referenced from AIKernel.Abstractions. AIKernel.Wasm provides
browser/WebAssembly implementations without leaking WASM runtime concerns back
into Core or AIKernel.Providers.

## Package

- `AIKernel.Wasm.Runtime`
- `AIKernel.Wasm.WebGpuComputeProvider`
- `aikernel-wasm`

## Documentation

- [Getting Started](docs/getting-started/index.md)
- [User Guide](docs/user-guide/index.md)
- [Architecture](docs/architecture/index.md)
- [Provider Catalog](docs/providers/index.md)
- [Runtime Providers](docs/runtime/index.md)
- [WebGPU Compute](docs/webgpu/index.md)
- [Manifests and Metadata](docs/manifests/index.md)
- [Python Wrapper](docs/python/index.md)
- [Testing](docs/testing/index.md)
- [Operations and Release Checklist](docs/operations/index.md)
- [Licensing](docs/licensing/index.md)

## Runtime Providers

The runtime package defines WASM-scoped providers for:

- `WasmRuntime`
- `WasmProcessProvider`
- `WasmMemoryProvider`
- `WasmStdinProvider`
- `WasmFileSystemProvider`
- `WasmEventProvider`
- `WasmAudioProvider`
- `WasmScreenshotProvider`
- `WasmSaveStateProvider`
- `WasmTimeProvider`

`WasmProcessProvider` implements the Core `IProcessHost` contract and creates
`IProcess` handles with `Starting`, `Running`, `Stopped`, and `Error` state
transitions. Process lifecycle transitions publish OS events through
`IEventBus` when an event bus is supplied.

`WasmRuntimeContext` is the shared runtime state object used by the providers.
It tracks loaded modules, deterministic linear memory, imports and exports,
stdin lines, an in-memory WASI-style file surface, framebuffer bytes, audio
bytes, save-state snapshots, and a controllable deterministic clock.

The provider responsibilities are intentionally narrow:

- `WasmMemoryProvider` performs checked linear-memory read/write operations.
- `WasmStdinProvider` forwards intervention input to the runtime `stdin` export.
- `WasmFileSystemProvider` exposes deterministic runtime files and file access events.
- `WasmEventProvider` bridges WASM-originated events into AIKernel `IEventBus`.
- `WasmAudioProvider` stores the latest audio buffer for a browser/WebAudio bridge.
- `WasmScreenshotProvider` returns framebuffer bytes for capture pipelines.
- `WasmSaveStateProvider` snapshots and restores linear memory.
- `WasmTimeProvider` controls pause, resume, scale, and deterministic time advance.

The current runtime model is deterministic and testable on Windows and Linux.
Browser-specific host bindings can be supplied at the boundary without changing
the Core contracts.

## WebGPU Compute

`WebGpuComputeProvider` lives under `src/Compute/WebGpuComputeProvider` and
implements `AIKernel.Abstractions.Compute.IComputeProvider`.

The provider exposes:

- `compute.dispatch`
- `compute.vector_add`

Backend bindings are separated into:

- `WebGpuNativeBackend`
- `WebGpuWasmBackend`

`WebGpuWasmBackend` accepts an `IWebGpuJsInterop` bridge. In browser builds,
that bridge owns `navigator.gpu`, buffer transfer, pipeline creation, and
`dispatchWorkgroups`. The .NET provider keeps the AIKernel compute contract,
buffer size validation, fallback routing, and EventBus publication.

When WebGPU is unavailable, the provider delegates CPU fallback execution to
`AIKernel.Providers.Standard.Compute.CpuComputeProvider`. Kernel execution
publishes `GpuKernelExecuted` when an `IEventBus` is supplied.

## Python Wrapper

`python/` contains the `aikernel-wasm` Python package. It exposes the public
WASM runtime providers and WebGPU compute provider through pythonnet as a thin
managed wrapper:

- `WasmRuntime`, `WasmRuntimeContext`
- `WasmProcessProvider`, `WasmMemoryProvider`, `WasmStdinProvider`
- `WasmFileSystemProvider`, `WasmEventProvider`, `WasmAudioProvider`
- `WasmScreenshotProvider`, `WasmSaveStateProvider`, `WasmTimeProvider`
- `WebGpuComputeProvider`, `WebGpuComputeInvoker`, `WebGpuComputeCapability`

The wrapper does not re-implement WASM runtime semantics in Python. It resolves
bundled assemblies from `aikernel_wasm/native` or local NuGet packages and can
use `AIKERNEL_WASM_ASSEMBLY_PATH` for additional assembly roots.

## Control Integration

Core.Control and Bonsai observe WASM runtime behavior through `IEventBus`.
AIKernel.Wasm publishes process, memory, stdin, file, and GPU events using
stable event names such as `ProcessStarted`, `ProcessStopped`,
`ProcessCrashed`, `MemoryAccessed`, `StdinSent`, `FileAccessed`, and
`GpuKernelExecuted`. This keeps runtime automation decoupled from browser and
provider-specific implementation details.

## Running WASM Applications

WASM applications are represented as AIKernel processes. A host creates a
process through `IProcessHost.CreateProcessAsync`, optionally passing
`WasmProcessOptions` with module bytes and initial memory size. The resulting
`IProcess` can be started, stopped, listed, killed, restarted, observed through
logs, and connected to scheduler rules through the same OS command model used
by other providers.

## CLI Integration

The `aik` CLI treats GPU, process, logs, and scheduler operations as OS
commands:

```powershell
aik gpu list
aik gpu run vector-add --a a.bin --b b.bin
aik run sample
aik ps
aik kill <pid-or-name>
aik restart <pid-or-name>
aik logs sample
aik schedule add --every 1m "aik system info"
```

## Build

```powershell
dotnet build AIKernel.Wasm.slnx -c Release
dotnet test AIKernel.Wasm.slnx -c Release --no-build
```

## Contributor Guidelines

WASM runtime and WebGPU changes must follow the shared AIKernel development
discipline:

- [AIKernel Development Guidelines](../AIKernel.NET/docs/guidelines/AIKERNEL_DEVELOPMENT_GUIDELINES.md)
- [AIKernel 開発ガイドライン](../AIKernel.NET/docs/guidelines/AIKERNEL_DEVELOPMENT_GUIDELINES-jp.md)

Runtime/provider code should preserve deterministic process state, use
fail-closed Result/Try boundaries for host integration, keep browser-specific
interop behind abstractions, and keep Python wrappers aligned with public C#
contracts.
