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

Use `AIKernel.Wasm.Runtime.Abstractions.IWasmProcessRuntime` to manage process state, bounded memory, and host callbacks. An embedding engine implements `AIKernel.Abstractions.Runtime.IWasmRuntime` for invocation requests.
For bytecode execution, configure a browser or another WebAssembly engine.
The embedding host must enforce isolation and restrict imports before loading untrusted modules.

AIKernel also provides an official AIOS distribution, codenamed
**AIKernel.Monolith**. Monolith has begun development as the standard AIOS that
will integrate the runtime model with the broader SDK after the 0.1.x
line stabilizes.

## Browser weight paging

`AIKernel.Wasm.WebGpuComputeProvider` 0.1.5 includes `BrowserWeightMemory` and depends on the AIKernel 0.1.4 SDK. Save a verified immutable weight file at the origin's OPFS root and open it from .NET WebAssembly in a dedicated worker. Serve the packaged `runtime/browser/opfs-weight-memory.mjs` asset at the application's origin.

```csharp
using var weights = await BrowserWeightMemory.OpenAsync(moduleUrl, "weights.bin",
    pageSize: 1 << 20, ramPages: 8);
weights.UploadPage(pageIndex, device, uploadBuffer);
```

Pass JavaScript objects for the WebGPU device and a buffer on that device with `COPY_DST` and the intended compute usage. `ramPages` bounds resident pages. Use `GetRamUsage()` for total page-buffer allocation and `GetStatistics()` for OPFS page reads. The JavaScript transfer buffer uses one page of scratch space. The application owns its GPU buffers.

Build the smoke test with `dotnet build tests/browser/WeightMemorySmoke -c Release`. Serve the generated `AppBundle` from localhost or HTTPS and open `index.html` to exercise the pager and WebGPU on real hardware.

## Package

- `AIKernel.Wasm.Runtime`
- `AIKernel.Wasm.Audio`
- `AIKernel.Wasm.Display`
- `AIKernel.Wasm.Input`
- `AIKernel.Wasm.WebGpuComputeProvider`

AIKernel.Wasm 0.1.3 is the canonical GPU integration line for the AIKernel.Wasm
runtime family. It follows the same development policy as AIKernel.Core,
AIKernel.Control, and AIKernel.Providers 0.1.3. The line publishes NuGet
packages and synchronized Python wrappers, uses `0.1.3-dev{build-number}` for
local development packages, and keeps the Python wrapper as a synchronized
metadata/discovery surface rather than a separate PyPI runtime implementation.

## Quick Start

Install only the package surface your host needs. Runtime tests and CPU fallback
paths work on Windows and Linux; real browser WebGPU validation remains a
separate manual check.

```bash
dotnet add package AIKernel.Wasm.Runtime --version 0.1.3
dotnet add package AIKernel.Wasm.Audio --version 0.1.3
dotnet add package AIKernel.Wasm.Display --version 0.1.3
dotnet add package AIKernel.Wasm.Input --version 0.1.3
dotnet add package AIKernel.Wasm.WebGpuComputeProvider --version 0.1.3
```

Use `AIKernel.Wasm.Runtime` for process, memory, stdin, file system, event,
save-state, and time providers. Add `AIKernel.Wasm.Audio`,
`AIKernel.Wasm.Display`, and `AIKernel.Wasm.Input` when the host needs the
browser audio boundary, frame surfaces, or virtual input boundary. Add
`AIKernel.Wasm.WebGpuComputeProvider` only when the host needs the WebGPU compute
boundary.

Python materials in `python/` are synchronized wrapper materials for this
update line. They expose package metadata, provider descriptors, and native
verification hints for 0.1.3 without re-implementing browser/WASM execution.

## Documentation

- [Getting Started](docs/getting-started/index.md)
- [User Guide](docs/user-guide/index.md)
- [Architecture](docs/architecture/index.md)
- [Provider Catalog](docs/providers/index.md)
- [Runtime Providers](docs/runtime/index.md)
- [WebGPU Compute](docs/webgpu/index.md)
- [Concept Elevation Notes](docs/development/concept-elevation.md) /
  [概念昇格ノート](docs/development/concept-elevation-ja.md)
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

## Browser Boundary Packages

`AIKernel.Wasm.Audio`, `AIKernel.Wasm.Display`, and `AIKernel.Wasm.Input` split
browser-facing runtime surfaces from the base runtime package:

- `WasmAudioProvider` keeps WebAudio interop behind the audio boundary.
- `WasmFramebufferProvider` and `WasmFrameSourceProvider` expose frame snapshots
  with frame index, hash, timestamp, dimensions, and pixel format metadata.
- `WasmInputProvider` accepts decomposed keyboard, pointer, drag, and state
  requests without creating Gate or Council decisions.

## WebGPU Compute

`WebGpuComputeProvider` lives under `src/Compute/WebGpuComputeProvider` and
implements `AIKernel.Abstractions.Compute.IComputeProvider`.

The provider exposes:

- `compute.dispatch`
- `compute.vector_add`
- `gpu.hud.composite`
- `gpu.aisthesis.raw-frame`
- `gpu.spatial-reasoning`
- `gpu.zero-copy.raw-texture`

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

The v0.1.3 WebGPU provider also packages the canonical browser bridge and
GPU-resident assets:

- `runtime/browser/webgpu-envelope-bridge.js`
- `shaders/hud/hud-composite.wgsl`
- `shaders/aisthesis/aisthesis.wgsl`
- `shaders/spatial/spatial-reasoning.wgsl`
- `buffers/layouts/gpu-layouts.json`

Use `scripts/verify-webgpu-package.ps1` to pack
`AIKernel.Wasm.WebGpuComputeProvider` and assert that the canonical bridge, WGSL
shaders, buffer layouts, provider manifest, and vector-add sample are included
in the NuGet package.

## Python Wrapper

`python/` contains synchronized `aikernel-wasm` Python wrapper materials. They document how the public WASM runtime providers and WebGPU compute provider may
be exposed through pythonnet as a thin managed wrapper:

- `WasmRuntime`, `WasmRuntimeContext`
- `WasmProcessProvider`, `WasmMemoryProvider`, `WasmStdinProvider`
- `WasmFileSystemProvider`, `WasmEventProvider`, `WasmAudioProvider`
- `WasmScreenshotProvider`, `WasmSaveStateProvider`, `WasmTimeProvider`
- `WasmFramebufferProvider`, `WasmFrameSourceProvider`, `WasmInputProvider`
- `WebGpuComputeProvider`, `WebGpuComputeInvoker`, `WebGpuComputeCapability`

The wrapper does not re-implement WASM runtime semantics in Python. In the
0.1.3 development line these materials are synchronized with the NuGet package
surface and keep browser execution at the WASM/WebGPU boundary.

## Control Integration

Core.Control and Bonsai observe WASM runtime behavior through `IEventBus`.
AIKernel.Wasm publishes process, memory, stdin, file, and GPU events using
stable event names such as `ProcessStarted`, `ProcessStopped`,
`ProcessCrashed`, `MemoryAccessed`, `StdinSent`, `FileAccessed`, and
`GpuKernelExecuted`. This keeps runtime automation decoupled from browser and
provider-specific implementation details.

## Running WASM Applications

WASM applications are represented as AIKernel processes. A host creates a
process through `IProcessHost.CreateProcessAsync` and supplies a WebAssembly v1 module in
`WasmProcessOptions.ModuleBytes`. `InitialMemoryBytes` is optional and defaults to 65,536 bytes. The resulting
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

- [AIKernel Development Guidelines](https://github.com/AIKernel-NET/AIKernel.NET/blob/dev/docs/guidelines/AIKERNEL_DEVELOPMENT_GUIDELINES.md)
- [AIKernel 開発ガイドライン](https://github.com/AIKernel-NET/AIKernel.NET/blob/dev/docs/guidelines/AIKERNEL_DEVELOPMENT_GUIDELINES-jp.md)

Runtime/provider code should preserve deterministic process state, use
fail-closed Result/Try boundaries for host integration, keep browser-specific
interop behind abstractions, and keep Python wrappers aligned with public C#
contracts.
