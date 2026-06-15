# User Guide

[日本語](index-ja.md)

This guide is for developers who want to consume AIKernel.Wasm from an
application or operator workflow. It focuses on the public runtime, process,
and WebGPU surfaces rather than internal architecture.

Wasm is the AIOS SDK sandboxed runtime layer. Add it when an AIOS distribution
needs lightweight VM-style process isolation, linear memory, WASI-style file
bridges, screenshot/audio surfaces, save state, or WebGPU boundaries.

AIKernel.Monolith is the official AIOS distribution now in development. It will
serve as the standard reference distribution that integrates sandboxed runtime
services with the broader SDK after the 0.1.x line stabilizes.

## What You Can Do

AIKernel.Wasm lets an AIKernel host:

- create a WASM runtime context
- create and manage WASM processes through Core `IProcessHost`
- read and write deterministic linear memory
- send stdin-style intervention text to a WASM process
- expose an in-memory WASI-style file surface
- capture framebuffer and audio buffers
- save and restore memory state
- run WebGPU compute with deterministic CPU fallback
- inspect deterministic runtime and WebGPU surfaces from C#

The default automated path works without a browser GPU. Browser WebGPU E2E
validation is a separate operator check.

## Install Packages

NuGet packages:

```powershell
dotnet add package AIKernel.Wasm.Runtime --version 0.1.1.1
dotnet add package AIKernel.Wasm.Audio --version 0.1.1.1
dotnet add package AIKernel.Wasm.Display --version 0.1.1.1
dotnet add package AIKernel.Wasm.Input --version 0.1.1.1
dotnet add package AIKernel.Wasm.WebGpuComputeProvider --version 0.1.1.1
```

For local development packages, use `0.1.1.1-dev{build-number}` from the shared
local NuGet source. The current baseline references AIKernel.Core
`0.1.1.1-dev1` and AIKernel.Providers `0.1.1.1-dev2`.

Python wrapper materials are reference-only for 0.1.1.1. Do not build, install,
or publish a PyPI package for this line.

## Create a Runtime Context

```csharp
using AIKernel.Wasm.Runtime;

using var runtime = new WasmRuntime();
await runtime.BootAsync();

var context = runtime.CreateContext(initialMemoryBytes: 65536);
await context.LoadModuleAsync(moduleBytes);
```

`WasmRuntimeContext` owns deterministic runtime state: module bytes, memory,
imports, exports, stdin lines, files, framebuffer, audio buffer, save-state, and
clock state.

## Create and Start a WASM Process

```csharp
using AIKernel.Abstractions.Processes;
using AIKernel.Wasm.Runtime;

var runtime = new WasmRuntime();
var process = await runtime.CreateProcessAsync(
    "sample",
    new WasmProcessOptions(
        ModuleBytes: moduleBytes,
        InitialMemoryBytes: 65536));

await process.StartAsync();
Console.WriteLine(process.State);
await process.StopAsync();
```

Process state follows the Core process model:

- `Starting`
- `Running`
- `Stopped`
- `Error`

If an `IEventBus` is supplied to the runtime or provider, lifecycle events such
as `ProcessStarted`, `ProcessStopped`, and `ProcessCrashed` are published.

## Use the Process Provider

Use `WasmProcessProvider` when you want a provider-style process host:

```csharp
using AIKernel.Wasm.Runtime;

var provider = new WasmProcessProvider();
var process = await provider.CreateProcessAsync(
    "sample",
    new WasmProcessOptions(ModuleBytes: moduleBytes));

await process.StartAsync();
var processes = provider.ListProcesses();
```

`TryCreateProcessAsync` and `TryStartAsync` are available for fail-closed
`Result<T>` workflows.

## Read and Write Linear Memory

```csharp
using AIKernel.Wasm.Runtime;

var context = new WasmRuntimeContext(initialMemoryBytes: 1024);
var memory = new WasmMemoryProvider(context);

memory.Write(0, new byte[] { 1, 2, 3, 4 });
var bytes = memory.Read(0, 4);
```

Memory access is range checked. Invalid access fails closed in the runtime
`Try*` APIs and throws through direct APIs.

## Send Stdin Text

```csharp
using AIKernel.Wasm.Runtime;

var context = new WasmRuntimeContext();
var stdin = new WasmStdinProvider(context);

await stdin.WriteLineAsync("hello");
```

The runtime records stdin lines deterministically. If a WASM export is connected
for stdin, the runtime boundary can route the text to that export.

## Work with Runtime Files

```csharp
using AIKernel.Wasm.Runtime;

var context = new WasmRuntimeContext();
var files = new WasmFileSystemProvider(context);

files.WriteFile("/input.txt", "hello"u8.ToArray());
var content = files.ReadFile("/input.txt");
```

The runtime file surface is in-memory and deterministic. It is intended for
WASI-style bridging and tests, not direct host file writes.

## Save and Restore State

```csharp
using AIKernel.Wasm.Runtime;

var context = new WasmRuntimeContext(initialMemoryBytes: 1024);
var saveState = new WasmSaveStateProvider(context);

var snapshot = await saveState.SaveAsync();
await saveState.RestoreAsync(snapshot);
```

Save state captures linear memory so deterministic replay and rollback flows can
restore a known runtime point.

## Control Runtime Time

```csharp
using AIKernel.Wasm.Runtime;

var context = new WasmRuntimeContext();
var time = new WasmTimeProvider(context);

time.Pause();
time.SetScale(0.5);
time.Resume();
```

Time control is deterministic and avoids wall-clock dependence in tests.

## Capture Screenshot, Display, Audio, and Input Surfaces

```csharp
using AIKernel.Wasm.Audio;
using AIKernel.Wasm.Display;
using AIKernel.Wasm.Input;
using AIKernel.Wasm.Runtime;
using AIKernel.Dtos.Frame;
using AIKernel.Dtos.Input;
using AIKernel.Dtos.Providers;

var context = new WasmRuntimeContext();
var screenshot = new WasmScreenshotProvider(context);
var audio = new WasmAudioProvider(context);
var frameSource = new WasmFrameSourceProvider(context);
var input = new WasmInputProvider(context);

byte[] frame = await screenshot.CaptureAsync();
byte[] audioBytes = audio.LatestAudioBuffer();
var execution = new ProviderExecutionContext { ExecutionId = "frame-1" };
FrameSnapshot? snapshot = null;
await foreach (var captured in frameSource.CaptureAsync(
    new FrameCaptureRequest { SourceId = "runtime", MaxFrames = 1 },
    execution,
    CancellationToken.None))
{
    snapshot = captured;
}

VirtualInputResult sent = await input.SendKeysAsync(new SendKeysRequest { Keys = ["Enter"] }, CancellationToken.None);
```

These providers expose runtime bytes, frame snapshots, and decomposed virtual
input at the WASM boundary. They do not create Council votes or Gate decisions.
Browser rendering or WebAudio integration remains the host application's
responsibility.

## Run WebGPU Compute with Fallback

```csharp
using AIKernel.Abstractions.Compute;
using AIKernel.Wasm.Compute;

var provider = new WebGpuComputeProvider();
await provider.InitializeAsync();

var a = await provider.CreateBufferAsync(16);
var b = await provider.CreateBufferAsync(16);
var output = await provider.CreateBufferAsync(16);

await provider.WriteBufferAsync(a, inputA);
await provider.WriteBufferAsync(b, inputB);

var kernel = new ComputeKernel(
    WebGpuSampleKernels.VectorAdd,
    dispatchX: 1,
    dispatchY: 1,
    dispatchZ: 1);

await provider.ExecuteKernelAsync(kernel, a, b, output);
```

If the WebGPU backend is unavailable, the provider delegates to
`CpuComputeProvider`. This keeps tests and local validation deterministic.

## Inspect the Capability Descriptor

```csharp
using AIKernel.Wasm.Compute;

var provider = new WebGpuComputeProvider();
var descriptor = provider.ToCapabilityDescriptor();

Console.WriteLine(descriptor.CapabilityId);
Console.WriteLine(string.Join(", ", descriptor.ProvidedOperations));
```

Expected capability:

- `webgpu.compute`
- `compute.dispatch`
- `compute.vector_add`

## Python Wrapper Materials

The Python wrapper material is a managed-wrapper reference. It does not execute
WASM or WebGPU logic in Python, and it is not packaged for PyPI in the 0.1.1.1
development line.

## Common Failure Modes

| Symptom | Likely Cause | Action |
| --- | --- | --- |
| WebGPU uses CPU fallback | Browser/backend binding unavailable | Use fallback for CI or provide `IWebGpuJsInterop` |
| Memory read fails | Offset/length outside memory range | Validate range before access |
| Process remains stopped | Module not loaded or process not started | Create with `WasmProcessOptions` and call `StartAsync` |

## Next Steps

- Read [Runtime Providers](../runtime/index.md) for runtime surface details.
- Read [WebGPU Compute](../webgpu/index.md) for backend and fallback details.
- Read [Testing](../testing/index.md) before adding browser E2E coverage.
