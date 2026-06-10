# Python Wrapper

[日本語](index-ja.md)

`aikernel-wasm` exposes the public AIKernel.Wasm C# surface to Python through a
thin pythonnet wrapper.

## Package

```bash
pip install aikernel-wasm
```

During local development, run tests with the package source on `PYTHONPATH` or
install the package in editable mode.

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

Assemblies are resolved in this order:

1. bundled files under `aikernel_wasm/native`
2. repository Release build output during local development
3. local NuGet package cache
4. paths listed in `AIKERNEL_WASM_ASSEMBLY_PATH`

If a required assembly is missing, the wrapper fails closed with a clear
`FileNotFoundError`.

## Contract Coverage

Python covers:

- runtime provider descriptors
- runtime provider construction wrappers
- WebGPU capability descriptor creation
- WebGPU provider and invoker construction wrappers
- assembly discovery and pythonnet runtime loading

The wrapper does not re-implement WASM execution, WebGPU dispatch, or Core
provider semantics.
