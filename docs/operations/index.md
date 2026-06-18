# Operations and Release Checklist

[日本語](index-ja.md)

Use this checklist before publishing AIKernel.Wasm packages.

## Build and Test

```powershell
dotnet build AIKernel.Wasm.slnx -c Release -p:WarningsAsErrors=1591
dotnet test AIKernel.Wasm.slnx -c Release --no-build
```

Success criteria:

- no XML documentation warnings for public C# members
- all runtime tests pass
- all audio, display, input, and concept-elevation boundary tests pass
- all WebGPU provider tests pass

## Package Metadata

Check NuGet package metadata:

- package ID
- version
- license expression
- project URL
- repository URL
- package icon
- README inclusion
- release notes

For v0.1.2 integration, validate the `aikernel-wasm` wheel with
`0.1.2.dev{buildNumber}`. Do not create stable `0.1.2` artifacts until the
publication task explicitly requests them.

## Documentation

Required documentation:

- repository README in English and Japanese
- docs index in English and Japanese
- architecture guide
- runtime provider guide
- browser boundary provider guide for audio, display, and input
- WebGPU guide
- concept elevation notes
- Python wrapper reference guide
- testing guide
- licensing guide
- manifest and metadata guide

## Browser GPU Validation

Automated Release tests do not require a physical GPU. Browser WebGPU validation
should be tracked separately as E2E validation because it depends on browser,
driver, adapter, and platform state.

## Safety Checks

Before release, scan docs and reference Python sources for:

- stale capability IDs
- temporary or incomplete wording
- unsafe scheduler examples
- sensitive metadata
- browser implementation objects leaking into public descriptors
