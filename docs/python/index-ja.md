# Python Wrapper

[English](index.md)

`aikernel-wasm` は public な AIKernel.Wasm C# surface を薄い pythonnet wrapper
として Python へ公開します。

## Package Policy

local validation では `0.1.3.dev{buildNumber}` wheel を使います。stable `0.1.3`
publication は共有の Trusted Publishing flow を使い、release task が明示的に
publication を開始した場合だけ行います。

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

Assembly は次の順で解決します。

1. `aikernel_wasm/native` 配下の同梱 file
2. local development 中の repository Release build output
3. local NuGet package cache
4. `AIKERNEL_WASM_ASSEMBLY_PATH` に列挙された path

必要な assembly が不足している場合、wrapper は明確な `FileNotFoundError` で
fail-closed します。

## Contract Coverage

package が網羅する範囲:

- runtime provider descriptor
- runtime provider construction wrapper
- WebGPU capability descriptor creation
- WebGPU provider / invoker construction wrapper
- assembly discovery と pythonnet runtime loading
- generated managed API catalog helper

Wrapper は WASM execution、WebGPU dispatch、Core provider semantics を
再実装しません。
## Trusted Publisher 設定

aikernel-wasm project の PyPI Trusted Publisher は、この repository が発行する GitHub OIDC claims と一致している必要があります。

| Field | Value |
| --- | --- |
| PyPI project | aikernel-wasm |
| Owner | AIKernel-NET |
| Repository | AIKernel.Wasm |
| Workflow | publish-pypi.yml |
| Environment | pypi |

PyPI が `invalid-publisher` を返す場合、workflow を token credential 方式へ戻してはいけません。PyPI project 側の Trusted Publisher entry を上記の値に合わせて修正し、失敗した publish job を rerun します。
