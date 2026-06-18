# Getting Started

[日本語](index-ja.md)

This guide verifies that AIKernel.Wasm builds, tests, and exposes the expected
runtime and WebGPU package surfaces.

## Prerequisites

- .NET 10 SDK
- AIKernel.NET contract packages `0.1.2`
- AIKernel.Core package `0.1.2`
- AIKernel.Providers package `0.1.2`

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

Version 0.1.2 is the current canonical integration line. AIKernel.Wasm uses
the NuGet patch line `0.1.2.1` while depending on AIKernel.NET,
AIKernel.Core, and AIKernel.Providers `0.1.2`. Use
`0.1.2.1-dev{buildNumber}` NuGet packages for local Wasm validation and
`0.1.2.dev{buildNumber}` `aikernel-wasm` wheels unless a separate PyPI patch
task is opened.
