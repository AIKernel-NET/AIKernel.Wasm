# Licensing

[日本語](index-ja.md)

AIKernel.Wasm follows the repository license declared for the package release.
NuGet package metadata should use the same license expression as the other
AIKernel public packages.

## Package Metadata

The public package metadata should include:

- package license expression
- project URL
- repository URL
- package icon
- bilingual README references where supported

## Dependency Notes

AIKernel.Wasm uses AIKernel contracts, Core runtime packages, and
Providers.Standard CPU fallback. Browser WebGPU bindings are kept behind the
`IWebGpuJsInterop` boundary. Python wrapper materials are reference-only for the
0.1.1.1 validation line and are not packaged for PyPI. The next official
v0.1.2 canonical series is expected to publish refreshed PyPI packages together
with NuGet.

When adding new browser or native dependencies, keep license information visible
in release notes and package metadata.
