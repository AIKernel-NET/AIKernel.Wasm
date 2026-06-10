# aikernel-wasm

AIKernel.Wasm runtime と WebGPU Provider の public surface を公開する Python
wrapper です。C# の契約境界を pythonnet 経由で公開し、WASM runtime semantics
は Python 側で再実装しません。

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

managed assembly は `aikernel_wasm/native` に同梱するか、NuGet/local package
cache から解決します。追加の assembly 探索 root は
`AIKERNEL_WASM_ASSEMBLY_PATH` で指定できます。
