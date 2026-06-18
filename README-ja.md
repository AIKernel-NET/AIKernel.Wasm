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

AIOS SDK において、AIKernel.Wasm は sandboxed runtime layer です。browser /
WebAssembly process、isolated memory、WASI-style service、WebGPU boundary を
扱う軽量 VM surface として、WASM sandbox が必要な AIOS distribution に追加します。

AIKernel には、公式 AIOS ディストリビューションである **AIKernel.Monolith** もあります。
Monolith は 0.1.x 系の安定化後に、sandboxed runtime layer をより広い SDK と
統合する標準 AIOS として開発が開始されています。

## Package

- `AIKernel.Wasm.Runtime`
- `AIKernel.Wasm.Audio`
- `AIKernel.Wasm.Display`
- `AIKernel.Wasm.Input`
- `AIKernel.Wasm.WebGpuComputeProvider`

AIKernel.Wasm 0.1.2 は AIKernel.Core / AIKernel.Control /
AIKernel.Providers 0.1.2 と同じ開発方針に従います。この line は NuGet package と同期 Python wrapper を公開し、local development package には `0.1.2-dev{build-number}` を使います。
PyPI package は作成・公開しません。

## クイックスタート

host が必要とする package surface だけを導入してください。runtime test と CPU fallback
path は Windows / Linux で検証できます。実 browser WebGPU validation は、別の
manual check として扱います。

```bash
dotnet add package AIKernel.Wasm.Runtime --version 0.1.2
dotnet add package AIKernel.Wasm.Audio --version 0.1.2
dotnet add package AIKernel.Wasm.Display --version 0.1.2
dotnet add package AIKernel.Wasm.Input --version 0.1.2
dotnet add package AIKernel.Wasm.WebGpuComputeProvider --version 0.1.2
```

process、memory、stdin、file system、event、audio、screenshot、save-state、
time Provider を使う場合は `AIKernel.Wasm.Runtime` を導入してください。
browser audio boundary、frame surface、virtual input boundary が必要な場合は
`AIKernel.Wasm.Audio`、`AIKernel.Wasm.Display`、`AIKernel.Wasm.Input` を追加します。
WebGPU compute boundary が必要な host だけ `AIKernel.Wasm.WebGpuComputeProvider`
を追加します。

`python/` の Python 関連資料は、この update line の同期 wrapper 資料です。
0.1.2 release flow で同期 Python wrapper として検証・公開します。

## Documentation

- [Getting Started](docs/getting-started/index-ja.md)
- [User Guide](docs/user-guide/index-ja.md)
- [Architecture](docs/architecture/index-ja.md)
- [Provider Catalog](docs/providers/index-ja.md)
- [Runtime Providers](docs/runtime/index-ja.md)
- [WebGPU Compute](docs/webgpu/index-ja.md)
- [Concept Elevation Notes / 概念昇格ノート](docs/development/concept-elevation-ja.md)
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

## Browser Boundary Package

`AIKernel.Wasm.Audio`、`AIKernel.Wasm.Display`、`AIKernel.Wasm.Input` は、
browser-facing runtime surface を base runtime package から分離します。

- `WasmAudioProvider` は WebAudio interop を audio boundary の背後に閉じます。
- `WasmFramebufferProvider` と `WasmFrameSourceProvider` は frame index、hash、
  timestamp、dimension、pixel format metadata 付きの frame snapshot を公開します。
- `WasmInputProvider` は Gate / Council decision を生成せず、keyboard、pointer、
  drag、state request に分解された virtual input を受け取ります。

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

`python/` には同期された `aikernel-wasm` Python wrapper 資料を配置して
います。pythonnet 経由の薄い managed wrapper として、public な WASM runtime
Provider と WebGPU compute Provider を公開する方法を示します。

- `WasmRuntime`, `WasmRuntimeContext`
- `WasmProcessProvider`, `WasmMemoryProvider`, `WasmStdinProvider`
- `WasmFileSystemProvider`, `WasmEventProvider`, `WasmAudioProvider`
- `WasmScreenshotProvider`, `WasmSaveStateProvider`, `WasmTimeProvider`
- `WasmFramebufferProvider`, `WasmFrameSourceProvider`, `WasmInputProvider`
- `WebGpuComputeProvider`, `WebGpuComputeInvoker`, `WebGpuComputeCapability`

この wrapper は WASM runtime semantics を Python 側で再実装しません。0.1.2
development line では同期 Python wrapper として検証・公開します。

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
