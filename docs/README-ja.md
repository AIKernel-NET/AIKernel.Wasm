# AIKernel.Wasm Documentation

[English](README.md)

AIKernel.Wasm は AIKernel の browser / WebAssembly runtime layer です。WASM
process execution、WebGPU compute、browser 境界の runtime service、Python
wrapper coverage を Core や host 側 Providers から分離します。

## Sections

- [Getting Started](getting-started/index-ja.md)
- [User Guide](user-guide/index-ja.md)
- [Architecture](architecture/index-ja.md)
- [Provider Catalog](providers/index-ja.md)
- [Runtime Providers](runtime/index-ja.md)
- [WebGPU Compute](webgpu/index-ja.md)
- [Manifests and Metadata](manifests/index-ja.md)
- [Python Wrapper](python/index-ja.md)
- [Testing](testing/index-ja.md)
- [Operations and Release Checklist](operations/index-ja.md)
- [Licensing](licensing/index-ja.md)

## Release Scope

Version 0.1.1 は AIKernel.Wasm の初回公開 package line です。次を提供します。

- `AIKernel.Wasm.Runtime`
- `WebGpuComputeProvider`
- `aikernel-wasm` Python wrapper
- WASM process、memory、stdin、file system、event、audio、screenshot、
  save-state、time Provider surface
- `AIKernel.Providers.Standard` 経由の deterministic CPU fallback 付き
  WebGPU compute

AIKernel.Wasm は AIKernel Core contract と Providers.Standard fallback driver に
依存しますが、browser / WASM 固有の実装責務を Core へ戻しません。
