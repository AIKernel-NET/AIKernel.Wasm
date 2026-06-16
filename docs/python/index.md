# Python Wrapper

[日本語](index-ja.md)

`aikernel-wasm` exposes the public AIKernel.Wasm C# surface to Python through a
thin pythonnet wrapper.

## Package Policy

Use `0.1.2.dev{buildNumber}` wheels for local validation. Stable `0.1.2`
publication uses the shared Trusted Publishing flow and starts only when the
release task explicitly opens publication.

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

The package covers:

- runtime provider descriptors
- runtime provider construction wrappers
- WebGPU capability descriptor creation
- WebGPU provider and invoker construction wrappers
- assembly discovery and pythonnet runtime loading
- generated managed API catalog helpers

The wrapper does not re-implement WASM execution, WebGPU dispatch, or Core
provider semantics.
