# WebGPU Compute

[日本語](index-ja.md)

`WebGpuComputeProvider` is the WASM-owned compute provider for browser/WebGPU
execution. It implements `AIKernel.Abstractions.Compute.IComputeProvider` and
keeps GPU-specific behavior outside Core.

## Capability Surface

The provider exposes:

- capability: `webgpu.compute`
- operations:
  - `compute.dispatch`
  - `compute.vector_add`

The capability descriptor records adapter profile, backend name, provider ID,
and deterministic metadata for host validation.

## Backend Model

The provider separates backend binding from AIKernel compute semantics:

- `IWebGpuBackend` defines adapter/device/queue operations.
- `WebGpuNativeBackend` is the host-side native binding adapter boundary.
- `WebGpuWasmBackend` is the browser/WASM backend and can use `IWebGpuJsInterop`.

The browser bridge owns WebGPU API calls such as buffer transfer, pipeline
creation, and dispatch. The provider owns AIKernel contract validation and
fallback routing.

## CPU Fallback

When WebGPU is unavailable, compute falls back to
`AIKernel.Providers.Standard.Compute.CpuComputeProvider`.

Fallback is deterministic and used by CI tests so the package can be verified
without a physical GPU. Real browser GPU validation remains an E2E/browser test
concern.

## Vector Add Example

The built-in sample WGSL kernel supports vector addition:

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

Kernel execution can publish `GpuKernelExecuted` through `IEventBus`. The event
contains deterministic metadata and does not expose browser implementation
objects.
