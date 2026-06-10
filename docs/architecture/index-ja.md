# AIKernel.Wasm Architecture

[English](index.md)

AIKernel.Wasm は AIKernel OS architecture の第 3 layer です。

- Core は process、compute、VFS、EventBus、Provider abstraction などの OS
  contract を定義します。
- Providers.Standard は CPU compute、logging、file system、network、
  scheduler、profiler、process supervision など host 側 OS driver を実装します。
- AIKernel.Wasm は browser / WebAssembly runtime service と WASM WebGPU
  compute backend を実装します。

## Responsibility Boundary

AIKernel.Wasm は browser または WASM execution に固有の実装詳細を所有します。

- WASM module loading と runtime context state
- deterministic linear memory と save-state handling
- WASM process 向け stdin、file、event、audio、screenshot、time service
- browser / WebGPU backend integration
- Core `IProcess` / `IProcessHost` への process lifecycle mapping

AIKernel.Wasm が所有しないもの:

- Core abstraction
- host 側 file system / logging driver
- WASM 以外の external provider manifest loading
- instrumentation / replay tool

## Dependency Direction

想定する依存方向は次の通りです。

```text
AIKernel.NET contracts
        ↓
AIKernel.Core
        ↓
AIKernel.Providers.Standard
        ↓
AIKernel.Wasm
```

AIKernel.Wasm は compute fallback として Providers.Standard の CPU driver を使用
できますが、Core は AIKernel.Wasm に依存しません。

## Event Flow

EventBus が supplied された場合、runtime activity は Core `IEventBus` を通じて
報告されます。安定した event name は次の通りです。

- `ProcessStarted`
- `ProcessStopped`
- `ProcessCrashed`
- `MemoryAccessed`
- `StdinSent`
- `FileAccessed`
- `GpuKernelExecuted`

これにより Control/Bonsai や CLI diagnostics は、browser 固有 object に依存せず
WASM behavior を監視できます。

## Package Layout

```text
AIKernel.Wasm/
  src/
    Runtime/
      AIKernel.Wasm.Runtime/
    Compute/
      WebGpuComputeProvider/
  python/
    src/aikernel_wasm/
  tests/
    AIKernel.Wasm.Tests/
    WebGpuComputeProvider.Tests/
```
