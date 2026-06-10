# Operations and Release Checklist

[日本語](index-ja.md)

Use this checklist before publishing AIKernel.Wasm packages.

## Build and Test

```powershell
dotnet build AIKernel.Wasm.slnx -c Release -p:WarningsAsErrors=1591
dotnet test AIKernel.Wasm.slnx -c Release --no-build
py -m compileall python/src
py -c "import sys, pytest; sys.path[:0]=['python/src']; raise SystemExit(pytest.main(['python/tests']))"
```

Success criteria:

- no XML documentation warnings for public C# members
- all runtime tests pass
- all WebGPU provider tests pass
- Python wrapper imports compile and pass tests

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

Check Python package metadata:

- package name: `aikernel-wasm`
- version
- license
- project URLs
- README
- typed package marker: `py.typed`

## Documentation

Required documentation:

- repository README in English and Japanese
- docs index in English and Japanese
- architecture guide
- runtime provider guide
- WebGPU guide
- Python wrapper guide
- testing guide
- licensing guide
- manifest and metadata guide

## Browser GPU Validation

Automated Release tests do not require a physical GPU. Browser WebGPU validation
should be tracked separately as E2E validation because it depends on browser,
driver, adapter, and platform state.

## Safety Checks

Before release, scan docs and Python sources for:

- stale capability IDs
- temporary or incomplete wording
- unsafe scheduler examples
- sensitive metadata
- browser implementation objects leaking into public descriptors
