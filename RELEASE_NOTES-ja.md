# AIKernel.Wasm リリースノート

[English](RELEASE_NOTES.md)

## 0.1.2

**2026年6月16日 - Browser runtime package line。**

AIKernel.Wasm 0.1.2 は browser / WebAssembly runtime package を AIKernel 0.1.2 dependency chain に揃えます。

- runtime、audio、display、input、HUD、perception、spatial、model、WebGPU compute provider surface を公開します。
- Wasm execution と WebGPU / WebAudio concern は Providers と Control から隔離します。
- documentation を同期 NuGet / Python wrapper release flow に揃えます。
- 将来の正典 Interface 昇格に備えた thin surface を維持します。