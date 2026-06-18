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
`IWebGpuJsInterop` boundary. The v0.1.2 Python wrapper is packaged as
`aikernel-wasm`, remains thin over managed assemblies, and does not carry
browser implementation code.

When adding new browser or native dependencies, keep license information visible
in release notes and package metadata.
