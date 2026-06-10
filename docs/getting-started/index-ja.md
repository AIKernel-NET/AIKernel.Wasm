# Getting Started

[English](index.md)

この guide では、AIKernel.Wasm が build / test でき、runtime と WebGPU package
surface を期待通り公開していることを確認します。

## Prerequisites

- .NET 10 SDK
- `aikernel-wasm` wrapper test 用の Python 3.10 以降
- AIKernel.Core / AIKernel.Providers package または local repository build

default check では実 browser WebGPU validation は不要です。automated test は
deterministic CPU fallback を使うため、物理 GPU が無い Windows / Linux でも実行
できます。

## Build

repository root から実行します。

```powershell
dotnet build AIKernel.Wasm.slnx -c Release
```

solution は次を build します。

- `AIKernel.Wasm.Runtime`
- `WebGpuComputeProvider`
- `AIKernel.Wasm.Tests`
- `WebGpuComputeProvider.Tests`

## Test

```powershell
dotnet test AIKernel.Wasm.slnx -c Release --no-build
```

期待する coverage:

- runtime context lifecycle
- process start/stop behavior
- EventBus publication
- checked linear-memory operation
- WebGPU capability descriptor mapping
- vector-add execution
- CPU fallback execution

## Python Wrapper Check

```powershell
py -c "import sys, pytest; sys.path[:0]=['python/src']; raise SystemExit(pytest.main(['python/tests']))"
```

Python wrapper test は runtime Provider と WebGPU compute wrapper の import
coverage を確認します。

## Minimal Python Usage

```python
from aikernel_wasm import WebGpuComputeCapability, wasm_provider_contracts

capability = WebGpuComputeCapability().to_contract()
print(capability.capability_id)
print(capability.provided_operations)

for provider in wasm_provider_contracts():
    print(provider.provider_id, provider.name)
```

C# runtime、process、memory、stdin、file、save-state、time、WebGPU、Python の
利用例は [User Guide](../user-guide/index-ja.md) を参照してください。

## Package Outputs

package line は次の通りです。

- NuGet: `AIKernel.Wasm.Runtime`
- NuGet: `AIKernel.Wasm.WebGpuComputeProvider`
- PyPI: `aikernel-wasm`

Version 0.1.1 は AIKernel.Wasm の初回公開 release line です。
