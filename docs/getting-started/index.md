# Getting Started

[日本語](index-ja.md)

This guide verifies that AIKernel.Wasm builds, tests, and exposes the expected
runtime and WebGPU package surfaces.

## Prerequisites

- .NET 10 SDK
- AIKernel.NET contract packages `0.1.1.1`
- AIKernel.Core local package `0.1.1.1-dev1`
- AIKernel.Providers local package `0.1.1.1-dev2`

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

Expected coverage:

- runtime context lifecycle
- process start/stop behavior
- EventBus publication
- checked linear-memory operations
- browser audio, frame-source, framebuffer, and virtual input boundaries
- WebGPU capability descriptor mapping
- vector-add execution
- CPU fallback execution

For C# runtime, process, memory, stdin, file, save-state, time, audio, display,
input, and WebGPU usage examples, continue to the
[User Guide](../user-guide/index.md).

## Package Outputs

The package line is:

- NuGet: `AIKernel.Wasm.Runtime`
- NuGet: `AIKernel.Wasm.Audio`
- NuGet: `AIKernel.Wasm.Display`
- NuGet: `AIKernel.Wasm.Input`
- NuGet: `AIKernel.Wasm.WebGpuComputeProvider`

Version 0.1.1.1 is the current NuGet-only development line. Python wrapper
materials remain reference-only and are not built, installed, or published as a
PyPI package in this line. Prepare refreshed Python wrapper packages for the
next official v0.1.2 canonical series together with the NuGet package family.
