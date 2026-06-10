# AIKernel.Wasm Documentation

[日本語](README-ja.md)

AIKernel.Wasm is the browser/WebAssembly runtime layer for AIKernel. It keeps
WASM process execution, WebGPU compute, browser-bound runtime services, and
Python wrapper coverage outside Core and host-side Providers.

These docs describe Wasm as the AIOS SDK sandboxed runtime layer. It acts as a
lightweight VM surface for browser/WebAssembly processes, isolated memory,
WASI-style services, and WebGPU boundaries.

AIKernel.Monolith is the official AIOS distribution now in development. It is
planned as the standard reference distribution that integrates the WASM sandbox
with kernel runtime, providers, control, and tools after the 0.1.x line stabilizes.

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

## Which Page Should I Read?

- Read Getting Started when you want the shortest explanation of how the WASM
  runtime fits beside Core and Providers.Standard.
- Read User Guide when you want host setup, package installation, and the
  deterministic runtime/provider workflow.
- Read Runtime Providers when working with process, memory, stdin, file system,
  event, audio, screenshot, save-state, or time surfaces.
- Read WebGPU Compute when validating browser WebGPU integration or CPU
  fallback behavior.
- Read Testing before adding runtime changes so Windows/Linux deterministic
  tests remain separate from manual browser GPU checks.

## First Validation

Use deterministic tests first. Real browser GPU validation is intentionally a
separate manual step:

```powershell
dotnet build AIKernel.Wasm.slnx -c Release
dotnet test AIKernel.Wasm.slnx -c Release --no-build
```

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
