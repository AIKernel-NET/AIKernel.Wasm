# AIKernel.Wasm リリースノート

[English](RELEASE_NOTES.md)

## 0.1.3

**2026年6月23日 - Canonical GPU rev3 boundary。**

AIKernel.Wasm 0.1.3 は browser/WASM runtime family 向けの canonical GPU
integration release line です。WebGPU Provider を rev3 GPU HUD、GPU
Aisthesis、Spatial Reasoning、zero-copy diagnostics、deterministic CPU fallback
metadata に合わせます。

- rev3 browser dispatch bridge を
  `runtime/browser/webgpu-rev3-envelope-bridge.js` として package に含めます。
- HUD composite、Aisthesis raw-frame feature mask、Spatial Reasoning 向けの
  GPU resident WGSL asset を package に含めます。
- canonical GPU layout metadata を `buffers/layouts/gpu-layouts.rev3.json` として含めます。
- release 前の NuGet package asset surface を確認する
  `scripts/verify-webgpu-package.ps1` を追加します。
- Python wrapper 資料を 0.1.3 NuGet package surface と native verification
  metadata に同期します。

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
