# AIKernel.Wasm リリースノート

[English](RELEASE_NOTES.md)

## 0.1.2.1

**2026年6月19日 - WASM perception patch。**

AIKernel.Wasm 0.1.2.1 は、AIKernel.Wasm 0.1.2 runtime family 向けの
NuGet 限定 patch release です。AIKernel.NET、AIKernel.Core、
AIKernel.Providers への依存は公開済み 0.1.2 package のまま維持します。

- Phainesis 現象と Nous 意味ベクトル向けの synthetic sensor pipeline 抽象を追加します。
- `AIKernel.Wasm.Perception` に `AIKernel.Common` の Result / Option / LINQ pipeline support を追加します。
- `IWasmSyntheticSensorKernelPlanner` により WebGPU resident kernel planning を再利用可能にします。
- WASM provider manifest を 0.1.2.1 NuGet patch version に揃えます。

## 0.1.2

**2026年6月16日 - Browser runtime package line。**

AIKernel.Wasm 0.1.2 は browser / WebAssembly runtime package を AIKernel 0.1.2 dependency chain に揃えます。

- runtime、audio、display、input、HUD、perception、spatial、model、WebGPU compute provider surface を公開します。
- Wasm execution と WebGPU / WebAudio concern は Providers と Control から隔離します。
- documentation を同期 NuGet / Python wrapper release flow に揃えます。
- 将来の正典 Interface 昇格に備えた thin surface を維持します。
