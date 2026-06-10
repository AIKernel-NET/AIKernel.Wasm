# Python Wrapper

[English](index.md)

`aikernel-wasm` は public な AIKernel.Wasm C# surface を薄い pythonnet wrapper
として Python へ公開します。

## Package

```bash
pip install aikernel-wasm
```

local development では、package source を `PYTHONPATH` に追加するか editable mode
で install して test します。

## Import Surface

```python
from aikernel_wasm import (
    WasmRuntime,
    WasmRuntimeContext,
    WasmProcessProvider,
    WasmMemoryProvider,
    WasmStdinProvider,
    WasmFileSystemProvider,
    WasmEventProvider,
    WasmAudioProvider,
    WasmScreenshotProvider,
    WasmSaveStateProvider,
    WasmTimeProvider,
    WebGpuComputeCapability,
    WebGpuComputeProvider,
    WebGpuComputeInvoker,
    wasm_provider_contracts,
)
```

## Managed Assembly Resolution

Assembly は次の順で解決されます。

1. `aikernel_wasm/native` 配下の同梱 file
2. local development 中の repository Release build output
3. local NuGet package cache
4. `AIKERNEL_WASM_ASSEMBLY_PATH` に列挙された path

必要な assembly が不足している場合、wrapper は明確な `FileNotFoundError` で
fail-closed します。

## Contract Coverage

Python が網羅する範囲:

- runtime provider descriptor
- runtime provider construction wrapper
- WebGPU capability descriptor creation
- WebGPU provider / invoker construction wrapper
- assembly discovery と pythonnet runtime loading

Wrapper は WASM execution、WebGPU dispatch、Core provider semantics を
再実装しません。
