# aikernel-wasm

Python wrapper for the public AIKernel.Wasm runtime and WebGPU provider
surface. The package exposes the C# contract boundary through pythonnet and
does not re-implement WASM runtime semantics in Python.

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

## Covered C# Surface

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

Managed assemblies can be bundled in `aikernel_wasm/native` or resolved from
NuGet/local package cache. Set `AIKERNEL_WASM_ASSEMBLY_PATH` to add additional
assembly search roots.
