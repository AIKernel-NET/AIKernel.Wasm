# User Guide

[English](index.md)

この guide は、AIKernel.Wasm を application や operator workflow から利用する
developer 向けです。内部 architecture ではなく、public runtime、process、
WebGPU、Python surface の使い方に焦点を当てます。

Wasm は AIOS SDK の sandboxed runtime layer です。AIOS distribution に軽量 VM 的な
process isolation、linear memory、WASI-style file bridge、screenshot / audio
surface、save state、WebGPU boundary が必要な場合に追加します。

公式 AIOS ディストリビューション **AIKernel.Monolith** の開発も開始されています。
Monolith は 0.1.x 系の安定化後に sandboxed runtime service をより広い SDK と
統合する標準 reference distribution として位置づけられます。

## What You Can Do

AIKernel.Wasm により、AIKernel host は次を実行できます。

- WASM runtime context の作成
- Core `IProcessHost` 経由の WASM process 作成 / 管理
- deterministic linear memory の read / write
- WASM process への stdin-style intervention text 送信
- in-memory WASI-style file surface の公開
- framebuffer / audio buffer の capture
- memory state の save / restore
- deterministic CPU fallback 付き WebGPU compute
- Python から同じ public surface を inspect

default automated path は browser GPU なしで動作します。Browser WebGPU E2E
validation は別の operator check です。

## Install Packages

NuGet package:

```powershell
dotnet add package AIKernel.Wasm.Runtime --version 0.1.1
dotnet add package AIKernel.Wasm.WebGpuComputeProvider --version 0.1.1
```

Python package:

```powershell
py -m pip install aikernel-wasm
```

local development では repository build output を使うか、
`AIKERNEL_WASM_ASSEMBLY_PATH` を設定し、Python wrapper が managed assembly を
見つけられるようにします。

## Create a Runtime Context

```csharp
using AIKernel.Wasm.Runtime;

using var runtime = new WasmRuntime();
await runtime.BootAsync();

var context = runtime.CreateContext(initialMemoryBytes: 65536);
await context.LoadModuleAsync(moduleBytes);
```

`WasmRuntimeContext` は module byte、memory、import、export、stdin line、file、
framebuffer、audio buffer、save-state、clock state を含む deterministic runtime
state を所有します。

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

Process state は Core process model に従います。

- `Starting`
- `Running`
- `Stopped`
- `Error`

runtime または provider に `IEventBus` が supplied された場合、`ProcessStarted`、
`ProcessStopped`、`ProcessCrashed` などの lifecycle event が publish されます。

## Use the Process Provider

Provider-style process host が必要な場合は `WasmProcessProvider` を使います。

```csharp
using AIKernel.Wasm.Runtime;

var provider = new WasmProcessProvider();
var process = await provider.CreateProcessAsync(
    "sample",
    new WasmProcessOptions(ModuleBytes: moduleBytes));

await process.StartAsync();
var processes = provider.ListProcesses();
```

fail-closed な `Result<T>` workflow には `TryCreateProcessAsync` と `TryStartAsync`
を利用できます。

## Read and Write Linear Memory

```csharp
using AIKernel.Wasm.Runtime;

var context = new WasmRuntimeContext(initialMemoryBytes: 1024);
var memory = new WasmMemoryProvider(context);

memory.Write(0, new byte[] { 1, 2, 3, 4 });
var bytes = memory.Read(0, 4);
```

Memory access は range check されます。範囲外 access は runtime の `Try*` API では
fail-closed し、直接 API では例外になります。

## Send Stdin Text

```csharp
using AIKernel.Wasm.Runtime;

var context = new WasmRuntimeContext();
var stdin = new WasmStdinProvider(context);

await stdin.WriteLineAsync("hello");
```

runtime は stdin line を deterministic に記録します。stdin export が接続されている
場合、runtime boundary は text をその export へ route できます。

## Work with Runtime Files

```csharp
using AIKernel.Wasm.Runtime;

var context = new WasmRuntimeContext();
var files = new WasmFileSystemProvider(context);

files.WriteFile("/input.txt", "hello"u8.ToArray());
var content = files.ReadFile("/input.txt");
```

runtime file surface は in-memory で deterministic です。WASI-style bridge と test
用途であり、host file write を直接行いません。

## Save and Restore State

```csharp
using AIKernel.Wasm.Runtime;

var context = new WasmRuntimeContext(initialMemoryBytes: 1024);
var saveState = new WasmSaveStateProvider(context);

var snapshot = await saveState.SaveAsync();
await saveState.RestoreAsync(snapshot);
```

Save state は linear memory を capture し、deterministic replay / rollback flow が
既知の runtime point へ復元できるようにします。

## Control Runtime Time

```csharp
using AIKernel.Wasm.Runtime;

var context = new WasmRuntimeContext();
var time = new WasmTimeProvider(context);

time.Pause();
time.SetScale(0.5);
time.Resume();
```

Time control は deterministic で、test が wall-clock に依存しないようにします。

## Capture Screenshot and Audio Buffers

```csharp
using AIKernel.Wasm.Runtime;

var context = new WasmRuntimeContext();
var screenshot = new WasmScreenshotProvider(context);
var audio = new WasmAudioProvider(context);

byte[] frame = await screenshot.CaptureAsync();
byte[] audioBytes = audio.LatestAudioBuffer();
```

これらの Provider は runtime context に保存された byte を公開します。Browser
rendering や WebAudio integration は host application の責務です。

## Run WebGPU Compute with Fallback

```csharp
using AIKernel.Abstractions.Compute;
using AIKernel.Wasm.Comput;

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

WebGPU backend が利用できない場合、Provider は `CpuComputeProvider` へ委譲します。
これにより test と local validation は deterministic に保たれます。

## Inspect the Capability Descriptor

```csharp
using AIKernel.Wasm.Comput;

var provider = new WebGpuComputeProvider();
var descriptor = provider.ToCapabilityDescriptor();

Console.WriteLine(descriptor.CapabilityId);
Console.WriteLine(string.Join(", ", descriptor.ProvidedOperations));
```

期待される capability:

- `webgpu.compute`
- `compute.dispatch`
- `compute.vector_add`

## Python Usage

```python
from aikernel_wasm import WebGpuComputeCapability, wasm_provider_contracts

capability = WebGpuComputeCapability().to_contract()
print(capability.capability_id)
print(capability.provided_operations)

for provider in wasm_provider_contracts():
    print(provider.provider_id, provider.name)
```

Python wrapper は managed wrapper です。Python 側で WASM execution や WebGPU logic
を再実装しません。

## Common Failure Modes

| Symptom | Likely Cause | Action |
| --- | --- | --- |
| Python が assembly を見つけられない | package 未 install または build output 不足 | Release build するか `AIKERNEL_WASM_ASSEMBLY_PATH` を設定 |
| WebGPU が CPU fallback になる | browser/backend binding が利用不可 | CI では fallback を使い、必要なら `IWebGpuJsInterop` を提供 |
| Memory read が失敗する | offset/length が memory range 外 | access 前に range を検証 |
| Process が stopped のまま | module 未 load または process 未 start | `WasmProcessOptions` で作成し `StartAsync` を呼ぶ |

## Next Steps

- runtime surface の詳細は [Runtime Providers](../runtime/index-ja.md) を参照してください。
- backend / fallback の詳細は [WebGPU Compute](../webgpu/index-ja.md) を参照してください。
- browser E2E coverage を追加する前に [Testing](../testing/index-ja.md) を確認してください。
