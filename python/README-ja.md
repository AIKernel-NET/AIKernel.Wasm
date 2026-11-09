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

## 0.1.3 の公開前検証

開発版では、NuGet の `0.1.3-dev3` フィード由来の AIKernel DLL 8個と、
依存グラフに含まれる `Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.8 を
同梱します。`WasmRuntime.create()`、`WasmRuntimeContext.create()`、
`WebGpuWasmBackend.create()` は .NET のコンストラクターの既定値を使います。
既定のランタイムコンテキストのリニアメモリは 64 KiB です。

開発版 wheel は作業用コピーから `0.1.3.dev3` としてビルドします。
安定版 `0.1.3` のビルド前は、安定版の依存グラフから DLL を差し替え、
wheel をインストールして再検証してください。[リリースノート](../RELEASE_NOTES-ja.md)
も参照してください。
