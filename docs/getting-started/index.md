# Getting Started

[日本語](index-ja.md)

This guide verifies that AIKernel.Wasm builds, tests, and exposes the expected
runtime and WebGPU package surfaces.

## Prerequisites

- .NET 10 SDK
- Python 3.10 or later for the `aikernel-wasm` wrapper tests
- AIKernel.Core and AIKernel.Providers packages or local repository builds

Real browser WebGPU validation is not required for the default checks. The
automated tests use deterministic CPU fallback so they can run on Windows and
Linux without a physical GPU.

## Build

From the repository root:

```powershell
dotnet build AIKernel.Wasm.slnx -c Release
```

The solution builds:

- `AIKernel.Wasm.Runtime`
- `WebGpuComputeProvider`
- `AIKernel.Wasm.Tests`
- `WebGpuComputeProvider.Tests`

## Test

```powershell
dotnet test AIKernel.Wasm.slnx -c Release --no-build
```

Expected coverage:

- runtime context lifecycle
- process start/stop behavior
- EventBus publication
- checked linear-memory operations
- WebGPU capability descriptor mapping
- vector-add execution
- CPU fallback execution

## Python Wrapper Check

```powershell
py -c "import sys, pytest; sys.path[:0]=['python/src']; raise SystemExit(pytest.main(['python/tests']))"
```

The Python wrapper validates import coverage for runtime providers and WebGPU
compute wrappers.

## Minimal Python Usage

```python
from aikernel_wasm import WebGpuComputeCapability, wasm_provider_contracts

capability = WebGpuComputeCapability().to_contract()
print(capability.capability_id)
print(capability.provided_operations)

for provider in wasm_provider_contracts():
    print(provider.provider_id, provider.name)
```

For C# runtime, process, memory, stdin, file, save-state, time, WebGPU, and
Python usage examples, continue to the [User Guide](../user-guide/index.md).

## Package Outputs

The package line is:

- NuGet: `AIKernel.Wasm.Runtime`
- NuGet: `AIKernel.Wasm.WebGpuComputeProvider`
- PyPI: `aikernel-wasm`

Version 0.1.1 is the first public AIKernel.Wasm release line.
