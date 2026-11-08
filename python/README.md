# aikernel-wasm

Python wrapper material for the public AIKernel.Wasm runtime and WebGPU provider
surface. Starting with the 0.1.3 canon line, this directory is packaged as
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

The v0.1.3 package exposes the generated managed API catalog through
`managed_api_catalog()`, `managed_api_summary()`, `managed_type_names()`, and
`find_managed_type(full_name)`.

## 0.1.3 Release Verification

The development bundle uses eight AIKernel assemblies from the
`0.1.3-dev3` NuGet feed and the graph's
`Microsoft.Extensions.DependencyInjection.Abstractions` 10.0.8 assembly.
`WasmRuntime.create()`, `WasmRuntimeContext.create()`, and
`WebGpuWasmBackend.create()` use the optional constructor defaults defined
by .NET; a default runtime context has 64 KiB of linear memory.

Build the development wheel as `0.1.3.dev3` from a staging copy. Before
building a stable `0.1.3` wheel, refresh the DLLs from the stable package
graph and repeat the installed-wheel tests. See [the release notes](../RELEASE_NOTES.md).
