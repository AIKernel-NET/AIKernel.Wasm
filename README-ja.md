# AIKernel.Wasm

[English README](README.md)

AIKernel.Wasm は、AIKernel の browser / WebAssembly runtime と WebGPU 境界を
隔離する repository です。

AIKernel.Providers が host OS 側の標準 driver を扱う一方で、AIKernel.Wasm は
WASM 固有の runtime、process、memory、stdin、file system、event、audio、
screenshot、save-state、time、WebGPU compute 境界を扱います。

`IProcess`、`IProcessHost`、`IComputeProvider`、`IProvider` などの Core 抽象は
AIKernel.Abstractions から参照します。AIKernel.Wasm は browser /
WebAssembly 実装を提供し、WASM runtime の関心事を Core や
AIKernel.Providers に逆流させません。

## Package

- `AIKernel.Wasm.Runtime`
- `AIKernel.Wasm.WebGpuComputeProvider`
- `aikernel-wasm`

## Documentation

- [Getting Started](docs/getting-started/index-ja.md)
- [User Guide](docs/user-guide/index-ja.md)
- [Architecture](docs/architecture/index-ja.md)
- [Provider Catalog](docs/providers/index-ja.md)
- [Runtime Providers](docs/runtime/index-ja.md)
- [WebGPU Compute](docs/webgpu/index-ja.md)
- [Manifests and Metadata](docs/manifests/index-ja.md)
- [Python Wrapper](docs/python/index-ja.md)
- [Testing](docs/testing/index-ja.md)
- [Operations and Release Checklist](docs/operations/index-ja.md)
- [Licensing](docs/licensing/index-ja.md)

## Runtime Provider

runtime package は WASM scope の Provider として以下を定義しています。

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

`WasmProcessProvider` は Core の `IProcessHost` contract を実装し、
`Starting`、`Running`、`Stopped`、`Error` の state transition を持つ
`IProcess` handle を作成します。EventBus が supplied された場合、process
lifecycle transition は OS event として publish されます。

`WasmRuntimeContext` は Provider 群が共有する runtime state object です。
load 済み module、deterministic linear memory、import/export、stdin line、
in-memory の WASI-style file surface、framebuffer byte、audio byte、
save-state snapshot、制御可能な deterministic clock を保持します。

各 Provider の責務は狭く保っています。

- `WasmMemoryProvider` は範囲 check 付きの linear-memory read/write を行います。
- `WasmStdinProvider` は介入入力を runtime の `stdin` export に転送します。
- `WasmFileSystemProvider` は deterministic runtime file と file access event を公開します。
- `WasmEventProvider` は WASM 起点の event を AIKernel `IEventBus` に bridge します。
- `WasmAudioProvider` は browser/WebAudio bridge 向けの最新 audio buffer を保持します。
- `WasmScreenshotProvider` は capture pipeline 向けに framebuffer byte を返します。
- `WasmSaveStateProvider` は linear memory の snapshot/restore を行います。
- `WasmTimeProvider` は pause、resume、scale、deterministic time advance を制御します。

現在の runtime model は deterministic で、Windows / Linux のどちらでも test 可能です。
browser 固有の host binding は Core contract を変更せずに境界へ差し込めます。

## WebGPU Compute

`WebGpuComputeProvider` は `src/Compute/WebGpuComputeProvider` 配下にあり、
`AIKernel.Abstractions.Compute.IComputeProvider` を実装します。

公開する capability:

- `compute.dispatch`
- `compute.vector_add`

backend binding は以下に分離されています。

- `WebGpuNativeBackend`
- `WebGpuWasmBackend`

`WebGpuWasmBackend` は `IWebGpuJsInterop` bridge を受け取ります。browser build では、
この bridge が `navigator.gpu`、buffer transfer、pipeline creation、
`dispatchWorkgroups` を所有します。.NET 側 Provider は AIKernel compute contract、
buffer size validation、fallback routing、EventBus publication を維持します。

WebGPU が利用できない場合、Provider は
`AIKernel.Providers.Standard.Compute.CpuComputeProvider` に CPU fallback 実行を
委譲します。`IEventBus` が supplied された場合、kernel execution は
`GpuKernelExecuted` を publish します。

## Python Wrapper

`python/` には `aikernel-wasm` Python package を配置しています。pythonnet
経由の薄い managed wrapper として、public な WASM runtime Provider と WebGPU
compute Provider を公開します。

- `WasmRuntime`, `WasmRuntimeContext`
- `WasmProcessProvider`, `WasmMemoryProvider`, `WasmStdinProvider`
- `WasmFileSystemProvider`, `WasmEventProvider`, `WasmAudioProvider`
- `WasmScreenshotProvider`, `WasmSaveStateProvider`, `WasmTimeProvider`
- `WebGpuComputeProvider`, `WebGpuComputeInvoker`, `WebGpuComputeCapability`

この wrapper は WASM runtime semantics を Python 側で再実装しません。
`aikernel_wasm/native` の同梱 assembly または local NuGet package から assembly
を解決し、追加 root は `AIKERNEL_WASM_ASSEMBLY_PATH` で指定できます。

## Control Integration

Core.Control と Bonsai は `IEventBus` 経由で WASM runtime behavior を監視します。
AIKernel.Wasm は `ProcessStarted`、`ProcessStopped`、`ProcessCrashed`、
`MemoryAccessed`、`StdinSent`、`FileAccessed`、`GpuKernelExecuted` といった
安定した event name で process、memory、stdin、file、GPU event を publish します。
これにより runtime automation は browser や Provider 固有実装から分離されます。

## WASM Application Execution

WASM application は AIKernel process として表現されます。host は
`IProcessHost.CreateProcessAsync` から process を作成し、必要に応じて
module byte と initial memory size を含む `WasmProcessOptions` を渡します。
返された `IProcess` は start、stop、list、kill、restart、log observation、
scheduler rule 連携を、他の Provider と同じ OS command model で扱えます。

## CLI Integration

`aik` CLI は GPU、process、logs、scheduler operation を OS command として扱います。

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

## コントリビュータ向けガイドライン

WASM runtime と WebGPU の変更は、AIKernel 共通の開発規律に従ってください。

- [AIKernel 開発ガイドライン](../AIKernel.NET/docs/guidelines/AIKERNEL_DEVELOPMENT_GUIDELINES-jp.md)
- [AIKernel Development Guidelines](../AIKernel.NET/docs/guidelines/AIKERNEL_DEVELOPMENT_GUIDELINES.md)

Runtime / Provider code は deterministic な process state を維持し、host
integration では fail-closed な Result / Try 境界を使い、browser 固有 interop
を abstraction の背後に閉じ、Python wrapper を public C# contract と整合させてください。
