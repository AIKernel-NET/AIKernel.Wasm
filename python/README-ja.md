# aikernel-wasm

AIKernel.Wasm runtime と WebGPU Provider の public surface を公開するための
reference-only Python wrapper 資料です。0.1.1.1 update line は NuGet-only です。
この directory を PyPI package として build / install / publish しません。

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

将来の Python release では、managed assembly を `aikernel_wasm/native` に同梱するか、
NuGet/local package cache から解決します。追加の assembly 探索 root は
`AIKERNEL_WASM_ASSEMBLY_PATH` で指定できます。
