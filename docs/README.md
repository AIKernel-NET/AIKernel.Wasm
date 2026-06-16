# AIKernel.Wasm Documentation

[日本語](README-ja.md)

AIKernel.Wasm is the browser/WebAssembly runtime layer for AIKernel. It keeps
WASM process execution, WebGPU compute, and browser-bound runtime services
outside Core and host-side Providers.

These docs describe Wasm as the AIOS SDK sandboxed runtime layer. It acts as a
lightweight VM surface for browser/WebAssembly processes, isolated memory,
WASI-style services, and WebGPU boundaries.

AIKernel.Monolith is the official AIOS distribution now in development. It is
planned as the standard reference distribution that integrates the WASM sandbox
with kernel runtime, providers, control, and tools after the 0.1.x line stabilizes.

## Cross-Repository Alignment

Shared repository boundaries, v0.1.2 development versioning, dependency order,
PyPI Trusted Publishing, and Python wrapper scope are defined by
[Package Release Alignment v0.1.2](https://github.com/AIKernel-NET/AIKernel.NET/blob/main/docs/development/package-release-alignment-v0.1.2.md).
The historical v0.1.1.1 validation rules remain available in
[AIKernel Repository Alignment v0.1.1.1](https://github.com/AIKernel-NET/AIKernel.NET/blob/main/docs/development/repository-alignment-v0.1.1.1.md).
When a change crosses repositories, start with the
[Cross-Repository Developer Guide v0.1.1.1](https://github.com/AIKernel-NET/AIKernel.NET/blob/main/docs/development/cross-repository-developer-guide-v0.1.1.1.md).

Wasm owns browser/WebAssembly execution, WebGPU/WebAudio, display, input,
perception, spatial, HUD, and runtime surfaces. It must not own Doom semantics,
Providers substrate ownership, or Gate/CTG decisions.

## Sections

- [Getting Started](getting-started/index.md)
- [User Guide](user-guide/index.md)
- [Architecture](architecture/index.md)
- [Provider Catalog](providers/index.md)
- [Runtime Providers](runtime/index.md)
- [WebGPU Compute](webgpu/index.md)
- [Concept Elevation Notes](development/concept-elevation.md) /
  [概念昇格ノート](development/concept-elevation-ja.md)
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

Version 0.1.2 is the current canonical integration line. Use
`0.1.2-dev{build-number}` for local NuGet package references and
`0.1.2.dev{build-number}` for local `aikernel-wasm` wheel validation. It provides:

- `AIKernel.Wasm.Runtime`
- `AIKernel.Wasm.Audio`
- `AIKernel.Wasm.Display`
- `AIKernel.Wasm.Input`
- `AIKernel.Wasm.WebGpuComputeProvider`
- WASM process, memory, stdin, file system, event, audio, screenshot,
  save-state, and time provider surfaces
- Browser audio, frame-source, framebuffer, and decomposed virtual input
  boundary surfaces
- WebGPU compute with deterministic CPU fallback through
  `AIKernel.Providers.Standard`

Stable package artifacts are created later in dependency order. Do not create
stable `0.1.2` packages until the publication task explicitly requests them.

AIKernel.Wasm depends on AIKernel Core contracts and Providers.Standard fallback
drivers, but it does not move browser/WASM-specific implementation concerns
back into Core.
