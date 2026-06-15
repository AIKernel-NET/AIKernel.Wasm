# Getting Started

[English](index.md)

この guide では、AIKernel.Wasm が build / test でき、runtime と WebGPU package
surface を期待通り公開していることを確認します。

## Prerequisites

- .NET 10 SDK
- AIKernel.NET contract package `0.1.1.1`
- AIKernel.Core local package `0.1.1.1-dev1`
- AIKernel.Providers local package `0.1.1.1-dev2`

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
- `AIKernel.Wasm.Audio`
- `AIKernel.Wasm.Display`
- `AIKernel.Wasm.Input`
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
- browser audio、frame-source、framebuffer、virtual input boundary
- WebGPU capability descriptor mapping
- vector-add execution
- CPU fallback execution

C# runtime、process、memory、stdin、file、save-state、time、audio、display、
input、WebGPU の利用例は [User Guide](../user-guide/index-ja.md) を参照してください。

## Package Outputs

package line は次の通りです。

- NuGet: `AIKernel.Wasm.Runtime`
- NuGet: `AIKernel.Wasm.Audio`
- NuGet: `AIKernel.Wasm.Display`
- NuGet: `AIKernel.Wasm.Input`
- NuGet: `AIKernel.Wasm.WebGpuComputeProvider`

Version 0.1.1.1 は現在の NuGet-only development line です。Python wrapper
関連資料は reference-only とし、この line では PyPI package として build /
install / publish しません。
