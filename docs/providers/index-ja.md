# Provider Catalog

[English](index.md)

AIKernel.Wasm は WASM scope の runtime Provider と WebGPU compute Provider を含みます。
これらの Provider は Core abstraction を実装しつつ、browser / WASM 実装詳細を
Core の外に保ちます。

## Runtime Providers

### WasmRuntime

`WasmRuntime` は WASM process host です。Core `IProcessHost` を実装し、
`WasmProcessOptions` から `IProcess` handle を作成します。

主な責務:

- runtime context の作成
- WASM process handle の作成
- runtime lifecycle の調整
- 最低層の WASM runtime Provider identity の公開

### WasmProcessProvider

`WasmProcessProvider` は WASM process lifecycle operation を所有します。

対応範囲:

- module byte からの process 作成
- start / stop lifecycle transition
- process listing
- lifecycle event の EventBus publication

### WasmMemoryProvider

`WasmMemoryProvider` は check 付き linear-memory operation を公開します。access 前に
range を検証し、`WasmRuntimeContext` へ委譲します。

### WasmStdinProvider

`WasmStdinProvider` は deterministic stdin text を runtime context へ転送します。
stdin export が存在する場合、runtime はその export を呼び出せます。

### WasmFileSystemProvider

`WasmFileSystemProvider` は runtime の deterministic in-memory WASI-style file
surface を公開します。host file-system write は直接実行しません。

### WasmEventProvider

`WasmEventProvider` は WASM 起点 event を AIKernel `IEventBus` へ bridge します。

### WasmScreenshotProvider

`WasmScreenshotProvider` は capture / diagnostic flow 向けに framebuffer byte を返します。

### WasmSaveStateProvider

`WasmSaveStateProvider` は runtime linear memory の snapshot / restore を行います。

### WasmTimeProvider

`WasmTimeProvider` は pause、resume、scale、tick operation を通じて deterministic
runtime time を制御します。

## Browser Boundary Provider

### WasmAudioProvider

`WasmAudioProvider` は runtime audio byte を保持し、任意の `IWebAudioJsInterop` を
`AIKernel.Wasm.Audio` の背後に閉じます。public API は platform audio SDK 型ではなく、
neutral な buffer / timing record を使います。

### WasmFramebufferProvider

`WasmFramebufferProvider` は現在の runtime framebuffer を、width、height、stride、
pixel format、observed timestamp、content hash metadata とともに公開します。

### WasmFrameSourceProvider

`WasmFrameSourceProvider` は WASM runtime surface 向けの frame capture と
virtual surface listing を実装します。

### WasmInputProvider

`WasmInputProvider` は keyboard、pointer、drag、gamepad、input-state request に
分解された virtual input を受け取ります。virtual input packet を記録し runtime event
を publish しますが、Council vote や Gate decision は出力しません。

## WebGPU Provider

### WebGpuComputeProvider

`WebGpuComputeProvider` は Core `IComputeProvider` を実装します。

所有する責務:

- WebGPU backend routing
- `CpuComputeProvider` 経由の CPU fallback routing
- compute buffer create / write / read / kernel execution
- WebGPU capability descriptor mapping
- `GpuKernelExecuted` EventBus publication

Provider は `compute.dispatch` と `compute.vector_add` を持つ `webgpu.compute` を
公開します。

## Python Wrapper

`aikernel-wasm` package は 0.1.3 Python wrapper family の一部です。Python 側で
WASM、WebGPU、WebAudio、perception logic を再実装せずに、wrapper / descriptor を
公開します。安定版 wheel は 0.1.3 公開タスク開始後に作成し、local validation では
`0.1.3.dev<build-number>` を使います。
