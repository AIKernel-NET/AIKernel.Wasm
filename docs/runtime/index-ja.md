# Runtime Providers

[English](index.md)

`AIKernel.Wasm.Runtime` は WASM process と runtime service surface を提供します。
現在の実装は deterministic で Windows / Linux 上で test 可能です。browser 固有
binding は境界に差し込めます。

## Runtime Context

`WasmRuntimeContext` は共有 state object です。次を追跡します。

- load 済み module byte
- linear memory
- import / export function
- stdin line
- deterministic in-memory file surface
- framebuffer byte
- audio buffer byte
- save-state snapshot
- 制御可能な runtime clock

runtime が内部で `Result<T>` を使用する箇所では、直接 operation と fail-closed
operation の両方を公開します。

Runtime state transition は monadic boundary で表現します。

- process creation / lifecycle operation は `Result<T>` を返す `Try*` method を公開する
- optional runtime service、EventBus、export、process handle は `Option<T>` で表す
- WebGPU backend と CPU fallback の選択は純粋な `Either<L,R>` decision とする
- host / browser interop call は `Try.RunAsync` 境界の内側へ隔離する

## Process Model

`WasmRuntime` は Core `IProcessHost` を実装します。`WasmProcessProvider` は
`WasmProcessOptions` を使って WASM process handle を作成し、起動します。

Process state は Core model に従います。

- `Starting`
- `Running`
- `Stopped`
- `Error`

EventBus が利用可能な場合、lifecycle transition は process event として publish
されます。

## Provider Surface

runtime package は次を公開します。

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

runtime file surface は deterministic な in-memory surface です。host file I/O を
直接実行せず、WASI-style read/write を Core abstraction へ bridge する用途です。

Memory operation は read/write 前に range check を行います。範囲外 access は
fail-closed し、instrumentation が接続されている場合は `MemoryAccessed` metadata
を publish できます。

## Save State and Time

Save state は linear memory の deterministic snapshot を保存します。Time control
は pause、resume、scale、deterministic tick advance を support し、wall-clock に
依存しない再現可能な test を可能にします。
