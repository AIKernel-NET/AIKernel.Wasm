# aikernel-wasm

Python wrapper material for the public AIKernel.Wasm runtime and WebGPU provider
surface. Starting with the 0.1.2 canon line, this directory is packaged as
`aikernel-wasm` for PyPI and remains a thin wrapper over managed C# assemblies.

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

Managed assemblies can be bundled in `aikernel_wasm/native` or resolved from the
NuGet/local package cache. Set `AIKERNEL_WASM_ASSEMBLY_PATH` to add additional
assembly search roots.

## Managed API Catalog

The v0.1.2 package exposes the generated managed API catalog through
`managed_api_catalog()`, `managed_api_summary()`, `managed_type_names()`, and
`find_managed_type(full_name)`.
