# aikernel-wasm

Reference-only Python wrapper material for the public AIKernel.Wasm runtime and
WebGPU provider surface. The 0.1.1.1 update line is NuGet-only; do not build,
install, or publish this directory as a PyPI package.

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

For future Python releases, managed assemblies can be bundled in
`aikernel_wasm/native` or resolved from the NuGet/local package cache. Set
`AIKERNEL_WASM_ASSEMBLY_PATH` to add additional assembly search roots.
