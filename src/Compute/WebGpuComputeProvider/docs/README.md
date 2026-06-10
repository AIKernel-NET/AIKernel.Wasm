# WebGpuComputeProvider

`WebGpuComputeProvider` is the official AIKernel compute provider for WebGPU
browser/WASM execution with deterministic CPU fallback.

The provider exposes:

- `compute.dispatch`
- `compute.vector_add`

It accepts WGSL compute kernels and models WebGPU initialization as:

1. Adapter
2. Device
3. Queue
4. Compute pipeline
5. Bind group
6. Dispatch

When a browser or host does not expose a WebGPU binding, the provider remains
available through CPU fallback. The fallback path is deterministic and supports
the bundled vector-add sample kernel.

Manifest file: `webgpu.provider.json`

Sample kernel: `samples/vector-add.wgsl`
