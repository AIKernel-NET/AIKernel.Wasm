# aikernel-wasm

AIKernel.Wasm runtime と WebGPU Provider の public surface を公開するための
Python wrapper 資料です。0.1.3 正典系列から、この directory は
`aikernel-wasm` として PyPI package 化され、managed C# assembly の薄い wrapper
として公開されます。

```python
from aikernel_wasm import (
    WebGpuComputeCapability,
    WebGpuComputeProvider,
    WasmProcessProvider,
    wasm_provider_contracts,
)

capability = WebGpuComputeCapability().to_contract()
process_provider = WasmProcessProvider.create()
```

## 対応する C# Surface

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
- `WebGpuComputeProvider`
- `WebGpuComputeInvoker`
- `WebGpuComputeCapabilityContracts`

managed assembly は `aikernel_wasm/native` に同梱するか、NuGet/local package cache
から解決します。追加の assembly 探索 root は `AIKERNEL_WASM_ASSEMBLY_PATH` で指定できます。

## Managed API Catalog

v0.1.3 package では generated managed API catalog を公開します。
`managed_api_catalog()`、`managed_api_summary()`、`managed_type_names()`、
`find_managed_type(full_name)` で確認できます。
