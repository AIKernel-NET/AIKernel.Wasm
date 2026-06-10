# WebGPU Compute

[English](index.md)

`WebGpuComputeProvider` は browser / WebGPU execution 向けに WASM が所有する
compute Provider です。`AIKernel.Abstractions.Compute.IComputeProvider` を
実装し、GPU 固有 behavior を Core の外に保ちます。

## Capability Surface

Provider が公開する surface:

- capability: `webgpu.compute`
- operations:
  - `compute.dispatch`
  - `compute.vector_add`

Capability descriptor は adapter profile、backend name、provider ID、host
validation 用の deterministic metadata を記録します。

## Backend Model

Provider は backend binding と AIKernel compute semantics を分離します。

- `IWebGpuBackend` は adapter / device / queue operation を定義します。
- `WebGpuNativeBackend` は host 側 native binding adapter boundary です。
- `WebGpuWasmBackend` は browser / WASM backend で、`IWebGpuJsInterop` を使用できます。

Browser bridge は buffer transfer、pipeline creation、dispatch など WebGPU API
call を所有します。.NET Provider は AIKernel contract validation と fallback
routing を所有します。

## CPU Fallback

WebGPU が利用できない場合、compute は
`AIKernel.Providers.Standard.Compute.CpuComputeProvider` に fallback します。

Fallback は deterministic で、物理 GPU が無い CI でも package を検証できます。
実 browser GPU validation は E2E / browser test の責務です。

## Vector Add Example

組み込み sample WGSL kernel は vector addition を support します。

```wgsl
@group(0) @binding(0) var<storage, read> a: array<f32>;
@group(0) @binding(1) var<storage, read> b: array<f32>;
@group(0) @binding(2) var<storage, read_write> out: array<f32>;

@compute @workgroup_size(64)
fn main(@builtin(global_invocation_id) gid: vec3<u32>) {
  let i = gid.x;
  out[i] = a[i] + b[i];
}
```

## Events

Kernel execution は `IEventBus` 経由で `GpuKernelExecuted` を publish できます。
event は deterministic metadata を含み、browser 実装 object は公開しません。
