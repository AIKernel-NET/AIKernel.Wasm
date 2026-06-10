# Licensing

[日本語](index-ja.md)

AIKernel.Wasm follows the repository license declared for the package release.
NuGet and Python package metadata should use the same license expression as the
other AIKernel public packages.

## Package Metadata

The public package metadata should include:

- package license expression
- project URL
- repository URL
- package icon
- bilingual README references where supported

## Dependency Notes

AIKernel.Wasm uses AIKernel contracts, Core runtime packages, Providers.Standard
CPU fallback, and pythonnet for the Python wrapper. Browser WebGPU bindings are
kept behind the `IWebGpuJsInterop` boundary.

When adding new browser or native dependencies, keep license information visible
in release notes and package metadata.
