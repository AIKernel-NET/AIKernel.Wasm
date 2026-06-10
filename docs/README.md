# AIKernel.Wasm Documentation

[日本語](README-ja.md)

AIKernel.Wasm is the browser/WebAssembly runtime layer for AIKernel. It keeps
WASM process execution, WebGPU compute, browser-bound runtime services, and
Python wrapper coverage outside Core and host-side Providers.

## Sections

- [Getting Started](getting-started/index.md)
- [User Guide](user-guide/index.md)
- [Architecture](architecture/index.md)
- [Provider Catalog](providers/index.md)
- [Runtime Providers](runtime/index.md)
- [WebGPU Compute](webgpu/index.md)
- [Manifests and Metadata](manifests/index.md)
- [Python Wrapper](python/index.md)
- [Testing](testing/index.md)
- [Operations and Release Checklist](operations/index.md)
- [Licensing](licensing/index.md)

## Release Scope

Version 0.1.1 is the first public AIKernel.Wasm package line. It provides:

- `AIKernel.Wasm.Runtime`
- `WebGpuComputeProvider`
- `aikernel-wasm` Python wrapper
- WASM process, memory, stdin, file system, event, audio, screenshot,
  save-state, and time provider surfaces
- WebGPU compute with deterministic CPU fallback through
  `AIKernel.Providers.Standard`

AIKernel.Wasm depends on AIKernel Core contracts and Providers.Standard fallback
drivers, but it does not move browser/WASM-specific implementation concerns
back into Core.
